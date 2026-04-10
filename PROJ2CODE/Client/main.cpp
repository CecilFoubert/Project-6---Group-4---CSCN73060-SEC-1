#define _CRT_SECURE_NO_WARNINGS


#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

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
// The data files use the format: "D_M_YYYY H:MM:SS" or "DD_MM_YYYY HH:MM:SS"
// e.g. "3_3_2023 14:53:21" or " 12_3_2023 14:56:47"
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
// File format (CSV):
//   Line 1  : FUEL TOTAL QUANTITY,<timestamp>,<fuel>,
//   Lines 2+ : <timestamp>,<fuel>,
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
// Socket helpers
// ---------------------------------------------------------------------------

static bool sendLine(SOCKET sock, const std::string& line)
{
    std::string msg = line + "\n";
    int total = 0;
    int len   = static_cast<int>(msg.size());
    while (total < len)
    {
        int n = send(sock, msg.c_str() + total, len - total, 0);
        if (n == SOCKET_ERROR) return false;
        total += n;
    }
    return true;
}

static bool recvLine(SOCKET sock, std::string& out)
{
    out.clear();
    char ch;
    while (true)
    {
        int n = recv(sock, &ch, 1, 0);
        if (n <= 0) return false;
        if (ch == '\n') return true;
        if (ch != '\r') out += ch;
    }
}

// ---------------------------------------------------------------------------
// main
// ---------------------------------------------------------------------------
// Usage: Client [server_ip] [port] [data_file] [plane_id]
int main(int argc, char* argv[])
{
    const std::string serverIp = (argc > 1) ? argv[1] : "10.192.148.31";
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

    // Initialise Winsock
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
    {
        std::cerr << "[Client] WSAStartup failed\n";
        return 1;
    }

    SOCKET sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (sock == INVALID_SOCKET)
    {
        std::cerr << "[Client] socket() failed: " << WSAGetLastError() << "\n";
        WSACleanup();
        return 1;
    }

    // Resolve server address and connect
    sockaddr_in serverAddr{};
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port   = htons(static_cast<u_short>(port));
    if (inet_pton(AF_INET, serverIp.c_str(), &serverAddr.sin_addr) != 1)
    {
        std::cerr << "[Client] Invalid server IP: " << serverIp << "\n";
        closesocket(sock);
        WSACleanup();
        return 1;
    }

    if (connect(sock, reinterpret_cast<sockaddr*>(&serverAddr), sizeof(serverAddr)) == SOCKET_ERROR)
    {
        std::cerr << "[Client] connect() failed: " << WSAGetLastError() << "\n";
        closesocket(sock);
        WSACleanup();
        return 1;
    }
    std::cout << "[Client] Connected to server.\n";

    // Handshake: send plane ID so the server can identify this aircraft (SYS-030, SYS-050)
    sendLine(sock, "PLANEID:" + planeId);

    std::string response;
    if (!recvLine(sock, response) || response != "OK")
    {
        std::cerr << "[Client] Handshake failed (got: " << response << ")\n";
        closesocket(sock);
        WSACleanup();
        return 1;
    }

    // SYS-040b/c: packetise and transmit each telemetry record
    const std::time_t startTime = records.front().timestamp;
    int sent = 0;

    for (const auto& rec : records)
    {
        // Convert absolute timestamp to elapsed seconds from start of flight
        double elapsedSeconds = static_cast<double>(rec.timestamp - startTime);

        std::ostringstream oss;
        oss << std::fixed << "DATA:" << elapsedSeconds << "," << rec.fuel;

        if (sendLine(sock, oss.str()))
            ++sent;
        else
        {
            std::cerr << "[Client] Send failed at t=" << elapsedSeconds << "s\n";
            break;
        }

        Sleep(1000); // 1 second between transmissions
    }

    // Signal end of flight (triggers SYS-020 on the server)
    sendLine(sock, "COMPLETE");

    // Read and display the result summary from the server
    std::string resultLine;
    if (recvLine(sock, resultLine) && resultLine.rfind("RESULT:", 0) == 0)
    {
        // Parse: "RESULT:<finalAvgRate>,<totalFuelConsumed>,<duration>,<lifetimeAvgRate>"
        std::istringstream ss(resultLine.substr(7));
        std::string tok;
        std::vector<double> vals;
        while (std::getline(ss, tok, ','))
        {
            try { vals.push_back(std::stod(tok)); } catch (...) { vals.push_back(0.0); }
        }
        while (vals.size() < 4) vals.push_back(0.0);

        std::cout << "[Client] Flight complete. Sent " << sent << " / "
                  << records.size() << " packets.\n";
        std::cout << "[Client] Final avg consumption : " << vals[0] << " gal/s\n";
        std::cout << "[Client] Total fuel consumed   : " << vals[1] << " gal\n";
        std::cout << "[Client] Flight duration       : " << vals[2] << " s\n";
        std::cout << "[Client] Lifetime avg rate     : " << vals[3] << " gal/s\n";
    }
    else
    {
        std::cout << "[Client] Flight complete. Sent " << sent
                  << " / " << records.size() << " packets.\n";
    }

    closesocket(sock);
    WSACleanup();
    return 0;
}
