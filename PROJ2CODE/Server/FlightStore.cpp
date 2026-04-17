#include "FlightStore.h"
#include <fstream>
#include <filesystem>
#include <iomanip>
#include <sstream>

FlightStore::FlightStore(const std::string& outputFile)
{
    bool writeHeader = !std::filesystem::exists(outputFile);
    file_.open(outputFile, std::ios::app);
    if (writeHeader)
        file_ << "PlaneId,FlightNumber,StartFuel_gal,EndFuel_gal,"
                 "TotalFuelConsumed_gal,Duration_s,AvgConsumptionRate_galps,Timestamp\n";
}

double FlightStore::update(const std::string& planeId, double time, double fuel)
{
    std::lock_guard<std::mutex> lock(storeMutex_);

    auto [it, inserted] = records_.try_emplace(planeId, planeId);
    AircraftRecord& record = it->second;

    if (!record.activeFlight.has_value())
        record.beginFlight();
    return record.activeFlight->update(time, fuel);
}

std::optional<EndFlightResult> FlightStore::endFlight(const std::string& planeId)
{
    std::lock_guard<std::mutex> lock(storeMutex_);

    auto it = records_.find(planeId);
    if (it == records_.end()) return std::nullopt;

    AircraftRecord& record  = it->second;
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
    file_ << std::fixed << std::setprecision(4)
          << record.planeId            << ","
          << session.flightNumber      << ","
          << session.startFuel         << ","
          << session.endFuel           << ","
          << session.totalFuelConsumed << ","
          << session.duration          << ","
          << session.finalAvgRate      << ","
          << ts.str()                  << "\n";
    file_.flush();
}
