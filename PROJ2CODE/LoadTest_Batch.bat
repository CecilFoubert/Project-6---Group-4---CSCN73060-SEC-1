@echo off
REM -----------------------------------------------------------------------
REM  LoadTest_Batch.bat
REM  Spawns COUNT instances of Client.exe simultaneously.
REM
REM  USAGE: Place this file next to Client.exe and a data file, then run.
REM  Edit SERVER_IP, PORT, DATA_FILE and count as needed.
REM -----------------------------------------------------------------------

SET SERVER_IP=127.0.0.1
SET PORT=9000
SET DATA_FILE=katl-kefd-B737-700.txt

SET /A "index = 1"
SET /A "count = 25"

:while
if %index% leq %count% (
    START /MIN Client.exe %SERVER_IP% %PORT% %DATA_FILE%
    SET /A index = %index% + 1
    @echo Spawned client %index%
    goto :while
)
echo Done spawning %count% clients.
