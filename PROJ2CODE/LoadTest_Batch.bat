@echo off
SET SERVER_IP=10.192.148.68
SET PORT=9000
SET DATA_FILE=data\katl-kefd-B737-700.txt
SET /A "index = 1"
SET /A "count = 25"
:while
if %index% leq %count% (
    START /MIN x64\Release\Client.exe %SERVER_IP% %PORT% %DATA_FILE%
    SET /A index = %index% + 1
    echo Spawned client %index%
    goto :while
)
echo Done spawning %count% clients.
