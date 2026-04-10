#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

#include <iostream>
#include <sstream>
#include <string>
#include <thread>
#include "FlightStore.h"

// ---------------------------------------------------------------------------
// Helper: read one '\n'-terminated line from a socket.
// Returns false if the connection is closed or an error occurs.
// ---------------------------------------------------------------------------
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
// Helper: send a complete line (appends '\n').
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

// ---------------------------------------------------------------------------
// Per-client thread handler  (SYS-001: one thread per connection)
// ---------------------------------------------------------------------------
static void handleClient(SOCKET clientSock, FlightStore* store)
{
    std::string line;

    // --- Handshake: read plane ID (SYS-030) ---
    if (!recvLine(clientSock, line))
    {
        closesocket(clientSock);
        return;
    }

    // Expected format: "PLANEID:<id>"
    std::string planeId;
    if (line.rfind("PLANEID:", 0) == 0)
        planeId = line.substr(8);
    else
        planeId = line;   // fallback: treat the whole line as the ID

    std::cout << "[Server] Client connected: plane " << planeId << "\n";
    sendLine(clientSock, "OK");

    // --- Telemetry loop (SYS-010) ---
    while (recvLine(clientSock, line))
    {
        if (line == "COMPLETE")
        {
            // SYS-020: finalise and store the average fuel consumption for this flight
            auto result = store->endFlight(planeId);
            if (result.has_value())
            {
                std::cout << "[Server] Flight ended for " << planeId
                          << "  final_avg="    << result->finalAvgRate    << " gal/s"
                          << "  lifetime_avg=" << result->lifetimeAvgRate << " gal/s\n";

                std::ostringstream oss;
                oss << std::fixed
                    << result->finalAvgRate      << ","
                    << result->totalFuelConsumed << ","
                    << result->duration          << ","
                    << result->lifetimeAvgRate;
                sendLine(clientSock, "RESULT:" + oss.str());
            }
            else
            {
                sendLine(clientSock, "RESULT:0,0,0,0");
            }
            break;
        }

        // Expected format: "DATA:<elapsed_seconds>,<fuel_gallons>"
        if (line.rfind("DATA:", 0) == 0)
        {
            std::string payload = line.substr(5);
            auto comma = payload.find(',');
            if (comma != std::string::npos)
            {
                try
                {
                    double t    = std::stod(payload.substr(0, comma));
                    double fuel = std::stod(payload.substr(comma + 1));

                    // SYS-010a: read the transmitted data
                    // SYS-010b: parse timing and remaining fuel
                    // SYS-010c: calculate and store current fuel consumption
                    store->update(planeId, t, fuel);
                }
                catch (...) { /* skip malformed packet */ }
            }
        }
    }

    closesocket(clientSock);
    std::cout << "[Server] Client disconnected: plane " << planeId << "\n";
}

// ---------------------------------------------------------------------------
// main
// ---------------------------------------------------------------------------
// Usage: Server [port] [output_csv]
int main(int argc, char* argv[])
{
    int         port       = (argc > 1) ? std::stoi(argv[1]) : 9000;
    std::string outputFile = (argc > 2) ? argv[2] : "flight_records.csv";

    // Initialise Winsock
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
    {
        std::cerr << "[Server] WSAStartup failed\n";
        return 1;
    }

    // Create the TCP listening socket
    SOCKET listenSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (listenSock == INVALID_SOCKET)
    {
        std::cerr << "[Server] socket() failed: " << WSAGetLastError() << "\n";
        WSACleanup();
        return 1;
    }

    // Allow address reuse so the server can restart immediately
    int opt = 1;
    setsockopt(listenSock, SOL_SOCKET, SO_REUSEADDR,
               reinterpret_cast<const char*>(&opt), sizeof(opt));

    sockaddr_in addr{};
    addr.sin_family      = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port        = htons(static_cast<u_short>(port));

    if (bind(listenSock, reinterpret_cast<sockaddr*>(&addr), sizeof(addr)) == SOCKET_ERROR)
    {
        std::cerr << "[Server] bind() failed: " << WSAGetLastError() << "\n";
        closesocket(listenSock);
        WSACleanup();
        return 1;
    }

    if (listen(listenSock, SOMAXCONN) == SOCKET_ERROR)
    {
        std::cerr << "[Server] listen() failed: " << WSAGetLastError() << "\n";
        closesocket(listenSock);
        WSACleanup();
        return 1;
    }

    FlightStore store(outputFile);

    std::cout << "[Server] Aircraft Telemetry Server listening on port " << port << "\n";
    std::cout << "[Server] Recording flights to: " << outputFile << "\n";

    // SYS-001: infinite accept loop — detach one thread per client (unlimited connections)
    while (true)
    {
        sockaddr_in clientAddr{};
        int         addrLen    = sizeof(clientAddr);
        SOCKET      clientSock = accept(listenSock,
                                        reinterpret_cast<sockaddr*>(&clientAddr),
                                        &addrLen);
        if (clientSock == INVALID_SOCKET)
        {
            std::cerr << "[Server] accept() failed: " << WSAGetLastError() << "\n";
            continue;
        }

        // Detach thread — it owns clientSock and closes it when done
        std::thread(handleClient, clientSock, &store).detach();
    }

    closesocket(listenSock);
    WSACleanup();
    return 0;
}
