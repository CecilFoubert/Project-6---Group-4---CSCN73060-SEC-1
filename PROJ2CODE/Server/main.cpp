#include <crow.h>
#include <nlohmann/json.hpp>
#include <iostream>
#include <string>
#include "FlightStore.h"

using json = nlohmann::json;

int main(int argc, char* argv[])
{
    int         port       = (argc > 1) ? std::stoi(argv[1]) : 9000;
    std::string outputFile = (argc > 2) ? argv[2] : "flight_records.csv";

    FlightStore store(outputFile);

    crow::SimpleApp app;  // Crow uses a thread pool internally (SYS-001)

    // ------------------------------------------------------------------ //
    // POST /telemetry                                                      //
    // Receives one telemetry data point from a client.                    //
    // Body JSON: { "planeId": "...", "time": <s>, "fuel": <gal> }        //
    // SYS-010: read data, parse timing/fuel, calculate consumption        //
    // SYS-030: identify plane by transmitted unique ID                    //
    // ------------------------------------------------------------------ //
    CROW_ROUTE(app, "/telemetry").methods(crow::HTTPMethod::Post)
    ([&store](const crow::request& req)
    {
        try
        {
            auto body   = json::parse(req.body);
            auto planeId = body.at("planeId").get<std::string>();
            double time  = body.at("time").get<double>();
            double fuel  = body.at("fuel").get<double>();

            double avgRate = store.update(planeId, time, fuel);

            CROW_LOG_INFO << "[" << planeId << "]"
                          << "  t=" << time << "s"
                          << "  fuel=" << fuel << " gal"
                          << "  avg=" << avgRate << " gal/s";

            json resp = { {"status", "ok"}, {"avgRate", avgRate} };
            return crow::response{ resp.dump() };
        }
        catch (const std::exception& e)
        {
            return crow::response{ 400, std::string("Bad request: ") + e.what() };
        }
    });

    // ------------------------------------------------------------------ //
    // POST /flight/end                                                     //
    // Signals that the client's flight is complete.                       //
    // Body JSON: { "planeId": "..." }                                     //
    // SYS-020: store final average fuel consumption for that flight       //
    // ------------------------------------------------------------------ //
    CROW_ROUTE(app, "/flight/end").methods(crow::HTTPMethod::Post)
    ([&store](const crow::request& req)
    {
        try
        {
            auto body    = json::parse(req.body);
            auto planeId = body.at("planeId").get<std::string>();

            auto result  = store.endFlight(planeId);
            if (!result.has_value())
                return crow::response{ 404, "No active flight for plane " + planeId };

            CROW_LOG_INFO << "[" << planeId << "] Flight ended."
                          << "  final_avg=" << result->finalAvgRate << " gal/s"
                          << "  lifetime_avg=" << result->lifetimeAvgRate << " gal/s";

            json resp = {
                { "status",            "ok"                       },
                { "finalAvgRate",      result->finalAvgRate       },
                { "totalFuelConsumed", result->totalFuelConsumed  },
                { "duration",          result->duration           },
                { "lifetimeAvgRate",   result->lifetimeAvgRate    }
            };
            return crow::response{ resp.dump() };
        }
        catch (const std::exception& e)
        {
            return crow::response{ 400, std::string("Bad request: ") + e.what() };
        }
    });

    // ------------------------------------------------------------------ //
    // GET /aircraft/<id>                                                   //
    // Returns lifetime statistics for a given plane.                      //
    // ------------------------------------------------------------------ //
    CROW_ROUTE(app, "/aircraft/<string>")
    ([&store](const std::string& planeId)
    {
        auto stats = store.getStats(planeId);
        if (!stats.has_value())
            return crow::response{ 404, "Aircraft not found: " + planeId };

        json resp = {
            { "planeId",         planeId              },
            { "flightCount",     stats->flightCount   },
            { "lifetimeAvgRate", stats->lifetimeAvgRate }
        };
        return crow::response{ resp.dump() };
    });

    std::cout << "[Server] Aircraft Telemetry Server starting on port " << port << "\n";
    std::cout << "[Server] Recording flights to: " << outputFile << "\n";

    // multithreaded() lets Crow spawn one thread per hardware core,
    // satisfying SYS-001 (unlimited parallel client connections).
    app.port(port).multithreaded().run();

    return 0;
}
