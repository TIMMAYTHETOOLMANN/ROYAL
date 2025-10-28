@echo off
REM DEPLOY_WATCHTIME_CLUSTER.bat
REM Usage: DEPLOY_WATCHTIME_CLUSTER.bat [TOTAL_VIEWERS] [VIEWERS_PER_CONTAINER] [CHANNEL_USERNAME]
REM Example: DEPLOY_WATCHTIME_CLUSTER.bat 50 10 timmaythetoolman

:: Defaults
set TOTAL=%1
if "%TOTAL%"=="" set TOTAL=50
set PER=%2
if "%PER%"=="" set PER=10
set CHANNEL=%3
if "%CHANNEL%"=="" set CHANNEL=timmaythetoolman

echo Deploying %TOTAL% viewers across containers with %PER% per container for channel %CHANNEL%

:: Calculate number of containers (ceil)
set /a NUM=(%TOTAL% + %PER% - 1) / %PER%

:: Create logs dir
if not exist "logs" mkdir logs

:: Remove any existing watchtime containers (best-effort)
echo Cleaning up old containers named watchtime-*
for /f "tokens=*" %%C in ('docker ps -a --format "{{.Names}}" ^| findstr /b /i "watchtime-"') do (
    echo Removing %%C
    docker rm -f %%C 2>nul
)

setlocal enabledelayedexpansion
for /l %%i in (1,1,%NUM%) do (
    set /a START_INDEX=(%%i - 1) * %PER%
    set /a REMAIN=%TOTAL% - (!START_INDEX!)
    if !REMAIN! LSS %PER% (
        set VIEWERS=!REMAIN!
    ) else (
        set VIEWERS=%PER%
    )
    if !VIEWERS! LEQ 0 set VIEWERS=%PER%
    set CONTAINER_NAME=watchtime-%%i
    echo Launching !CONTAINER_NAME! with !VIEWERS! viewers...
    if not exist "logs\!CONTAINER_NAME!" mkdir "logs\!CONTAINER_NAME!"

    docker run -d --name !CONTAINER_NAME! --restart unless-stopped ^
        -e MODE=WATCHTIME ^
        -e CHANNEL_USERNAME=%CHANNEL% ^
        -e VIEWER_COUNT=!VIEWERS! ^
        -e HEADLESS=false ^
        -e LOW_CPU_RAM=true ^
        -e MAX_VIEWERS=8 ^
        -v "%CD%\logs\!CONTAINER_NAME!":/app/logs ^
        streamviewerbot:latest
)
endlocal

echo Deployed %NUM% containers. Total targeted viewers: %TOTAL%
echo Use "docker ps --filter name=watchtime-" to verify and "docker logs -f <container>" to monitor individual containers.

