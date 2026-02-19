@echo off
REM Quick start script for PC Part Picker (Windows)

echo ==========================================
echo   PC Part Picker - Starting Application
echo ==========================================
echo.

REM Check if Docker Desktop is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker Desktop is not running!
    echo.
    echo Please start Docker Desktop first, then run this script again.
    echo.
    pause
    exit /b 1
)

echo [1/3] Starting MySQL container on port 6701...
docker-compose up -d mysql

echo.
echo [2/3] Waiting for MySQL to be ready...
powershell -Command "Start-Sleep -Seconds 10"

echo.
echo [3/3] Starting ASP.NET application on port 6700...
echo.
echo The application will:
echo  - Auto-apply database migrations
echo  - Seed the database with initial data
echo  - Start the web server on http://localhost:6700
echo.
echo ==========================================
echo.

dotnet run

echo.
echo ==========================================
echo   Application stopped
echo ==========================================
pause

