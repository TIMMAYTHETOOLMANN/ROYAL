@echo off
REM STOP_WATCHTIME_ALL.bat
REM Stops and removes all watchtime-* containers

echo Stopping all Watch Time Booster containers...

for /f "tokens=*" %%C in ('docker ps -q --filter "name=watchtime-"') do (
    echo Stopping %%C...
    docker stop %%C 2>nul
)

for /f "tokens=*" %%C in ('docker ps -aq --filter "name=watchtime-"') do (
    echo Removing %%C...
    docker rm %%C 2>nul
)

echo.
echo All Watch Time Booster containers stopped and removed.
echo Use "docker ps -a" to verify.

