#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

#include <iostream>
#include <sstream>
#include <string>
#include <thread>
#include <mutex>
#include <charconv>
#include "FlightStore.h"

static std::mutex coutMutex_;

template<typename... Args>
static void syncPrint(Args&&... args)
{
    std::lock_guard<std::mutex> lock(coutMutex_);
    (std::cout << ... << args);
}

// ---------------------------------------------------------------------------
// Buffered reader — amortises recv() syscalls across a 4 KB chunk.
// ---------------------------------------------------------------------------
struct SocketBuffer
{
    static constexpr int BUF_SIZE = 4096;
    char buf[BUF_SIZE];
    int  pos {0};
    int  len {0};

    // Fill the buffer from the socket if it is empty.
    bool fill(SOCKET sock)
    {
        if (pos < len) return true;
        pos = 0;
        int n = recv(sock, buf, BUF_SIZE, 0);
        if (n <= 0) { len = 0; return false; }
        len = n;
        return true;
    }

    // Read one '\n'-terminated line. Returns false on disconnect/error.
    bool readLine(SOCKET sock, std::string& out)
    {
        out.clear();
        while (true)
        {
            if (!fill(sock)) return false;
            while (pos < len)
            {
                char ch = buf[pos++];
                if (ch == '\n') return true;
                if (ch != '\r') out += ch;
            }
        }
    }
};

// ---------------------------------------------------------------------------
// Helper: send a complete line (appends '\n') — no heap allocation.
// ---------------------------------------------------------------------------
static bool sendLine(SOCKET sock, const std::string& line)
{
    // Send the line content
    int total = 0, len = static_cast<int>(line.size());
    while (total < len)
    {
        int n = send(sock, line.c_str() + total, len - total, 0);
        if (n == SOCKET_ERROR) return false;
        total += n;
    }
    // Send the newline separately
    char nl = '\n';
    int  sent = 0;
    while (sent < 1)
    {
        int n = send(sock, &nl, 1, 0);
        if (n == SOCKET_ERROR) return false;
        sent += n;
    }
    return true;
}

// ---------------------------------------------------------------------------
// Per-client thread handler  (SYS-001: one thread per connection)
// ---------------------------------------------------------------------------
static void handleClient(SOCKET clientSock, FlightStore* store)
{
    SocketBuffer sbuf;
    std::string  line;

    // --- Handshake: read plane ID (SYS-030) ---
    if (!sbuf.readLine(clientSock, line))
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

    syncPrint("[Server] Client connected: plane ", planeId, "\n");
    sendLine(clientSock, "OK");

    // --- Telemetry loop (SYS-010) ---
    while (sbuf.readLine(clientSock, line))
    {
        if (line == "COMPLETE")
        {
            // SYS-020: finalise and store the average fuel consumption for this flight
            auto result = store->endFlight(planeId);
            if (result.has_value())
            {
                syncPrint("[Server] Flight ended for ", planeId,
                          "  final_avg=",    result->finalAvgRate,    " gal/s"
                          "  lifetime_avg=", result->lifetimeAvgRate, " gal/s\n");

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
            const char* begin = line.c_str() + 5;
            const char* end   = line.c_str() + line.size();
            double t = 0.0, fuel = 0.0;
            auto r1 = std::from_chars(begin, end, t);
            if (r1.ec == std::errc{} && r1.ptr < end && *r1.ptr == ',')
            {
                auto r2 = std::from_chars(r1.ptr + 1, end, fuel);
                if (r2.ec == std::errc{})
                {
                    // SYS-010a: read the transmitted data
                    // SYS-010b: parse timing and remaining fuel
                    // SYS-010c: calculate and store current fuel consumption
                    store->update(planeId, t, fuel);
                }
            }
        }
    }

    closesocket(clientSock);
    syncPrint("[Server] Client disconnected: plane ", planeId, "\n");
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

        // Detach thread — it owns clientSock and closes it when done (SYS-001)
        std::thread(handleClient, clientSock, &store).detach();
    }

    closesocket(listenSock);
    WSACleanup();
    return 0;
}
