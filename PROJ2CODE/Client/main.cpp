#include <httplib.h>
#include <nlohmann/json.hpp>
#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include <random>
#include <iomanip>
#include <algorithm>
#include <cctype>
#include <ctime>
#include <cstdio>

using json = nlohmann::json;

// ---------------------------------------------------------------------------
// Utility helpers
// ---------------------------------------------------------------------------

// SYS-050: generate a random 8-character hex plane ID on startup
static std::string generatePlaneId()
{
    std::random_device              rd;
    std::mt19937                    gen(rd());
    std::uniform_int_distribution<> dist(0, 255);
    std::ostringstream oss;
    for (int i = 0; i < 4; ++i)
        oss << std::hex << std::setw(2) << std::setfill('0') << dist(gen);
    std::string id = oss.str();
    std::transform(id.begin(), id.end(), id.begin(), ::toupper);
    return id;
}

static std::string trim(const std::string& s)
{
    auto first = s.find_first_not_of(" \t\r\n");
    if (first == std::string::npos) return {};
    auto last = s.find_last_not_of(" \t\r\n");
    return s.substr(first, last - first + 1);
}

// Split a string on the first occurrence of delim
static std::vector<std::string> splitCSV(const std::string& line)
{
    std::vector<std::string> cols;
    std::string col;
    for (char c : line)
    {
        if (c == ',') { cols.push_back(col); col.clear(); }
        else          { col += c; }
    }
    cols.push_back(col);   // last field (may be empty trailing comma)
    return cols;
}

// ---------------------------------------------------------------------------
// Timestamp parsing
// ---------------------------------------------------------------------------
// The data files use the format:  "D_M_YYYY H:MM:SS"  or  "DD_MM_YYYY HH:MM:SS"
// e.g. "3_3_2023 14:53:21"  or  " 12_3_2023 14:56:47"
// Returns true and sets `out` to a time_t value, or false on failure.
static bool parseTimestamp(const std::string& raw, std::time_t& out)
{
    int day = 0, mon = 0, year = 0, hour = 0, min = 0, sec = 0;
    if (std::sscanf(raw.c_str(), " %d_%d_%d %d:%d:%d",
                    &day, &mon, &year, &hour, &min, &sec) != 6)
        return false;

    std::tm tm{};
    tm.tm_mday  = day;
    tm.tm_mon   = mon - 1;       // tm_mon is 0-based
    tm.tm_year  = year - 1900;
    tm.tm_hour  = hour;
    tm.tm_min   = min;
    tm.tm_sec   = sec;
    tm.tm_isdst = -1;

    out = std::mktime(&tm);
    return (out != static_cast<std::time_t>(-1));
}

// ---------------------------------------------------------------------------
// TelemetryRecord — one parsed data point
// ---------------------------------------------------------------------------
struct TelemetryRecord
{
    std::time_t timestamp;
    double      fuel;        // gallons
};

// ---------------------------------------------------------------------------
// Parse one telemetry file.
//
// File format (CSV, from real flight recorder data):
//   Line 1  : FUEL TOTAL QUANTITY,<timestamp>,<fuel>,
//   Lines 2+ :  <timestamp>,<fuel>,
//
// Returns the list of parsed records in chronological order.
// ---------------------------------------------------------------------------
static std::vector<TelemetryRecord> parseFile(const std::string& path)
{
    std::ifstream file(path);
    if (!file.is_open())
    {
        std::cerr << "[Client] Cannot open data file: " << path << "\n";
        return {};
    }

    std::vector<TelemetryRecord> records;
    std::string line;
    bool firstLine = true;

    while (std::getline(file, line))
    {
        auto cols = splitCSV(line);

        std::string tsRaw, fuelRaw;

        if (firstLine)
        {
            firstLine = false;
            // Header line: "FUEL TOTAL QUANTITY,<ts>,<fuel>,"
            if (cols.size() < 3) continue;
            tsRaw   = trim(cols[1]);
            fuelRaw = trim(cols[2]);
        }
        else
        {
            // Regular line: " <ts>,<fuel>,"
            if (cols.size() < 2) continue;
            tsRaw   = trim(cols[0]);
            fuelRaw = trim(cols[1]);
        }

        if (tsRaw.empty() || fuelRaw.empty()) continue;

        std::time_t ts;
        if (!parseTimestamp(tsRaw, ts)) continue;

        double fuel = 0.0;
        try { fuel = std::stod(fuelRaw); }
        catch (...) { continue; }

        records.push_back({ ts, fuel });
    }

    return records;
}

// ---------------------------------------------------------------------------
// main
// ---------------------------------------------------------------------------
// Usage: Client [server_ip] [port] [data_file] [plane_id]
int main(int argc, char* argv[])
{
    const std::string serverIp = (argc > 1) ? argv[1] : "127.0.0.1";
    const int         port     = (argc > 2) ? std::stoi(argv[2]) : 9000;
    const std::string dataFile = (argc > 3) ? argv[3] : "katl-kefd-B737-700.txt";
    // SYS-050: unique ID assigned at startup; auto-generated if not provided
    const std::string planeId  = (argc > 4) ? argv[4] : generatePlaneId();

    std::cout << "[Client] Plane ID  : " << planeId  << "\n";
    std::cout << "[Client] Server    : " << serverIp << ":" << port << "\n";
    std::cout << "[Client] Data file : " << dataFile << "\n";

    // SYS-040a: open and parse the telemetry file
    auto records = parseFile(dataFile);
    if (records.empty())
    {
        std::cerr << "[Client] No valid records found in: " << dataFile << "\n";
        return 1;
    }

    std::cout << "[Client] Parsed " << records.size() << " telemetry records.\n";

    httplib::Client cli(serverIp, port);
    cli.set_connection_timeout(10, 0);
    cli.set_read_timeout(10, 0);

    const std::time_t startTime = records.front().timestamp;
    int sent = 0;

    for (const auto& rec : records)
    {
        // Convert absolute timestamp to elapsed seconds from start of flight
        double elapsedSeconds = static_cast<double>(rec.timestamp - startTime);

        // SYS-040b: packetise  (SYS-040c: transmit)
        json body = {
            { "planeId", planeId        },
            { "time",    elapsedSeconds },
            { "fuel",    rec.fuel       }
        };

        auto res = cli.Post("/telemetry", body.dump(), "application/json");
        if (!res || res->status != 200)
        {
            std::cerr << "[Client] Send failed at t=" << elapsedSeconds << "s\n";
        }
        else
        {
            ++sent;
        }
    }

    // Notify the server that the flight is complete (triggers SYS-020)
    json endBody = { { "planeId", planeId } };
    auto endRes  = cli.Post("/flight/end", endBody.dump(), "application/json");

    if (endRes && endRes->status == 200)
    {
        auto resp = json::parse(endRes->body);
        std::cout << "[Client] Flight complete. Sent " << sent << " / "
                  << records.size() << " packets.\n";
        std::cout << "[Client] Final avg consumption : "
                  << resp.value("finalAvgRate",      0.0) << " gal/s\n";
        std::cout << "[Client] Total fuel consumed   : "
                  << resp.value("totalFuelConsumed", 0.0) << " gal\n";
        std::cout << "[Client] Flight duration       : "
                  << resp.value("duration",          0.0) << " s\n";
        std::cout << "[Client] Lifetime avg rate     : "
                  << resp.value("lifetimeAvgRate",   0.0) << " gal/s\n";
    }
    else
    {
        std::cout << "[Client] Flight complete. Sent " << sent
                  << " / " << records.size() << " packets.\n";
    }

    return 0;
}
