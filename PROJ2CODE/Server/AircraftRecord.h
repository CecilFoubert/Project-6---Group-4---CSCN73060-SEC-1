#pragma once
#include "FlightSession.h"
#include <string>
#include <vector>
#include <optional>

/// Persistent record for one aircraft across all of its flights.
/// Callers must hold storeMutex_ (in FlightStore) before calling any method.
class AircraftRecord
{
public:
    std::string                  planeId;
    std::vector<FlightSession>   completedFlights;
    std::optional<FlightSession> activeFlight;

    explicit AircraftRecord(std::string id) : planeId(std::move(id)) {}

    /// Start a new flight session and return a reference to it.
    FlightSession& beginFlight()
    {
        activeFlight.emplace();
        activeFlight->flightNumber = static_cast<int>(completedFlights.size()) + 1;
        return *activeFlight;
    }

    /// End the active session, finalise it, move it to the completed list, and return it.
    std::optional<FlightSession> endFlight()
    {
        if (!activeFlight.has_value()) return std::nullopt;
        activeFlight->complete();
        lifetimeFuel_ += activeFlight->totalFuelConsumed;
        lifetimeTime_ += activeFlight->totalTimeElapsed;
        completedFlights.push_back(*activeFlight);
        auto finished = std::move(activeFlight);
        activeFlight.reset();
        return finished;
    }

    /// Lifetime average across all completed flights — O(1).
    double lifetimeAvgRate() const
    {
        return lifetimeTime_ > 0.0 ? lifetimeFuel_ / lifetimeTime_ : 0.0;
    }

private:
    double lifetimeFuel_ {0.0};
    double lifetimeTime_ {0.0};
};
