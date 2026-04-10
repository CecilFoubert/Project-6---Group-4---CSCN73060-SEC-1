#include "FlightStore.h"
#include <fstream>
#include <filesystem>
#include <iomanip>
#include <sstream>

FlightStore::FlightStore(const std::string& outputFile)
    : outputFile_(outputFile)
{
    // Write CSV header only if the file does not already exist
    if (!std::filesystem::exists(outputFile_))
    {
        std::ofstream f(outputFile_);
        f << "PlaneId,FlightNumber,StartFuel_gal,EndFuel_gal,"
             "TotalFuelConsumed_gal,Duration_s,AvgConsumptionRate_galps,Timestamp\n";
    }
}

double FlightStore::update(const std::string& planeId, double time, double fuel)
{
    std::lock_guard<std::mutex> lock(storeMutex_);

    // Identify or create the aircraft record (SYS-030)
    auto [it, inserted] = records_.try_emplace(planeId, planeId);
    AircraftRecord& record = it->second;

    // Begin a new session if this is the first packet for this flight
    if (!record.activeFlight.has_value())
        record.beginFlight();

    // Calculate and return the current average consumption (SYS-010c)
    return record.activeFlight->update(time, fuel);
}

std::optional<EndFlightResult> FlightStore::endFlight(const std::string& planeId)
{
    std::lock_guard<std::mutex> lock(storeMutex_);

    auto it = records_.find(planeId);
    if (it == records_.end()) return std::nullopt;

    AircraftRecord& record   = it->second;
    auto            finished = record.endFlight();  // SYS-020: stores final avg
    if (!finished.has_value()) return std::nullopt;

    appendToFile(record, *finished);

    return EndFlightResult{
        finished->finalAvgRate,
        finished->totalFuelConsumed,
        finished->duration,
        record.lifetimeAvgRate()
    };
}

std::optional<AircraftStats> FlightStore::getStats(const std::string& planeId)
{
    std::lock_guard<std::mutex> lock(storeMutex_);

    auto it = records_.find(planeId);
    if (it == records_.end()) return std::nullopt;

    const AircraftRecord& record = it->second;
    return AircraftStats{
        static_cast<int>(record.completedFlights.size()),
        record.lifetimeAvgRate()
    };
}

void FlightStore::appendToFile(const AircraftRecord& record, const FlightSession& session)
{
    // Capture UTC timestamp
    auto now = std::chrono::system_clock::now();
    auto tt  = std::chrono::system_clock::to_time_t(now);
    std::tm  tm{};
#ifdef _WIN32
    gmtime_s(&tm, &tt);
#else
    gmtime_r(&tt, &tm);
#endif
    std::ostringstream ts;
    ts << std::put_time(&tm, "%Y-%m-%dT%H:%M:%SZ");

    std::lock_guard<std::mutex> lock(fileMutex_);
    std::ofstream f(outputFile_, std::ios::app);
    f << std::fixed << std::setprecision(4)
      << record.planeId            << ","
      << session.flightNumber      << ","
      << session.startFuel         << ","
      << session.endFuel           << ","
      << session.totalFuelConsumed << ","
      << session.duration          << ","
      << session.finalAvgRate      << ","
      << ts.str()                  << "\n";
}
