@echo off
REM -----------------------------------------------------------------------
REM  EnduranceTest_Batch.bat
REM  Continuously spawns COUNT clients every TIMEOUT seconds.
REM  Runs forever until manually terminated (Ctrl+C).
REM
REM  USAGE: Place next to Client.exe and a data file, then run.
REM  Edit SERVER_IP, PORT, DATA_FILE, count and timeout as needed.
REM
REM  TIP: Set timeout = (observed single-run duration in seconds) - 60
REM       so the next wave arrives just before the previous one finishes.
REM -----------------------------------------------------------------------

SET SERVER_IP=127.0.0.1
SET PORT=9000
SET DATA_FILE=katl-kefd-B737-700.txt

SET /A "index = 1"
SET /A "count = 100"

:while
@echo %time% -- Spawning wave of %count% clients...

:spawnloop
if %index% leq %count% (
    START /MIN Client.exe %SERVER_IP% %PORT% %DATA_FILE%
    SET /A index = %index% + 1
    @echo Spawned client %index%
    goto :spawnloop
)

timeout 250 > NUL
SET /A index = 1
goto :while
