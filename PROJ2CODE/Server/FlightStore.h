#pragma once
#include "AircraftRecord.h"
#include <unordered_map>
#include <mutex>
#include <string>
#include <optional>

struct EndFlightResult
{
    double finalAvgRate;
    double totalFuelConsumed;
    double duration;
    double lifetimeAvgRate;
};

struct AircraftStats
{
    int    flightCount;
    double lifetimeAvgRate;
};

/// Thread-safe in-memory store for all aircraft records.
/// Also persists completed flight data to a CSV file (SYS-020).
class FlightStore
{
public:
    explicit FlightStore(const std::string& outputFile);

    /// Process one telemetry data point (SYS-010). Returns current avg rate.
    double update(const std::string& planeId, double time, double fuel);

    /// Finalise the active flight for planeId and persist it.
    std::optional<EndFlightResult> endFlight(const std::string& planeId);

    /// Return statistics for a plane, if known.
    std::optional<AircraftStats> getStats(const std::string& planeId);

private:
    std::unordered_map<std::string, AircraftRecord> records_;
    std::mutex                                       storeMutex_;
    std::string                                      outputFile_;
    std::mutex                                       fileMutex_;

    void appendToFile(const AircraftRecord& record, const FlightSession& session);
};
