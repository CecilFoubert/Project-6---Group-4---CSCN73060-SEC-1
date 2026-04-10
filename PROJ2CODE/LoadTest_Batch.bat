
@echo off
SET SERVER_IP=10.192.148.31
SET PORT=9000
SET DATA_FILE=data\Telem_2023_3_12 16_26_4.txt

SET /A "index = 1"
SET /A "count = 100"

:while
if %index% leq %count% (
    START /MIN x64\Release\Client.exe %SERVER_IP% %PORT% %DATA_FILE%
    SET /A index = %index% + 1
    echo Spawned client %index%
    goto :while
)