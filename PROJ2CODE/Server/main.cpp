#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

#include <iostream>
#include <sstream>
#include <string>
#include <thread>
#include <vector>
#include <queue>
#include <condition_variable>
#include "FlightStore.h"

// ---------------------------------------------------------------------------
// Bounded thread pool — fixes unbounded thread-per-client (SYS-001)
// ---------------------------------------------------------------------------
class ThreadPool
{
public:
    explicit ThreadPool(int numThreads)
    {
        for (int i = 0; i < numThreads; ++i)
            workers_.emplace_back([this] { workerLoop(); });
    }

    ~ThreadPool()
    {
        {
            std::lock_guard<std::mutex> lock(mutex_);
            shutdown_ = true;
        }
        cv_.notify_all();
        for (auto& t : workers_) t.join();
    }

    // Submit a connected socket. Blocks if all workers are busy and the
    // queue has reached capacity (backpressure).
    void submit(SOCKET sock)
    {
        std::unique_lock<std::mutex> lock(mutex_);
        cvFull_.wait(lock, [this] { return queue_.size() < MAX_QUEUE || shutdown_; });
        queue_.push(sock);
        cv_.notify_one();
    }

    void setHandler(void (*fn)(SOCKET, FlightStore*), FlightStore* store)
    {
        handler_ = fn;
        store_   = store;
    }

private:
    static constexpr std::size_t MAX_QUEUE = 256;

    void workerLoop()
    {
        while (true)
        {
            SOCKET sock;
            {
                std::unique_lock<std::mutex> lock(mutex_);
                cv_.wait(lock, [this] { return !queue_.empty() || shutdown_; });
                if (shutdown_ && queue_.empty()) return;
                sock = queue_.front();
                queue_.pop();
                cvFull_.notify_one();
            }
            handler_(sock, store_);
        }
    }

    std::vector<std::thread>  workers_;
    std::queue<SOCKET>        queue_;
    std::mutex                mutex_;
    std::condition_variable   cv_;
    std::condition_variable   cvFull_;
    bool                      shutdown_ {false};
    void (*handler_)(SOCKET, FlightStore*) {nullptr};
    FlightStore*              store_ {nullptr};
};

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

    std::cout << "[Server] Client connected: plane " << planeId << "\n";
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
// Usage: Server [port] [output_csv] [num_threads]
int main(int argc, char* argv[])
{
    int         port        = (argc > 1) ? std::stoi(argv[1]) : 9000;
    std::string outputFile  = (argc > 2) ? argv[2] : "flight_records.csv";
    int         numThreads  = (argc > 3) ? std::stoi(argv[3]) : 64;

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
    ThreadPool  pool(numThreads);
    pool.setHandler(handleClient, &store);

    std::cout << "[Server] Aircraft Telemetry Server listening on port " << port << "\n";
    std::cout << "[Server] Recording flights to: " << outputFile << "\n";
    std::cout << "[Server] Thread pool size: " << numThreads << "\n";

    // SYS-001: accept loop — dispatch to bounded thread pool
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

        pool.submit(clientSock);
    }

    closesocket(listenSock);
    WSACleanup();
    return 0;
}
