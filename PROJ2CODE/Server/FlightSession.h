#pragma once

/// Tracks fuel/time data for one continuous flight (one client connection).
struct FlightSession
{
    int    flightNumber  {0};
    double startFuel     {0.0};
    double startTime     {0.0};
    double prevFuel      {0.0};
    double prevTime      {0.0};
    double endFuel       {0.0};
    double endTime       {0.0};

    // Running totals for real-time average (SYS-010c)
    double totalFuelConsumed {0.0};
    double totalTimeElapsed  {0.0};

    // Populated by complete()
    double finalAvgRate  {0.0};
    double duration      {0.0};

    bool firstReading    {true};

    /// Returns the current real-time average consumption rate (gal/s).
    double currentAvgRate() const
    {
        return totalTimeElapsed > 0.0 ? totalFuelConsumed / totalTimeElapsed : 0.0;
    }

    /// Process one telemetry reading. Returns the updated average rate.
    double update(double time, double fuel)
    {
        if (firstReading)
        {
            startFuel = prevFuel = endFuel = fuel;
            startTime = prevTime = endTime = time;
            firstReading = false;
        }
        else
        {
            double dt = time - prevTime;
            double df = prevFuel - fuel;   // fuel consumed since last reading

            if (dt > 0.0 && df >= 0.0)
            {
                totalFuelConsumed += df;
                totalTimeElapsed  += dt;
            }

            prevFuel = fuel;
            prevTime = time;
            endFuel  = fuel;
            endTime  = time;
        }

        return currentAvgRate();
    }

    /// Finalise the session when the flight ends (SYS-020).
    void complete()
    {
        duration     = endTime - startTime;
        finalAvgRate = (duration > 0.0) ? (startFuel - endFuel) / duration : 0.0;
    }
};
