@echo off
REM DEPLOY_WATCHTIME_50.bat
REM Usage: DEPLOY_WATCHTIME_50.bat [CHANNEL_USERNAME] [VIEWER_COUNT]
REM Example: DEPLOY_WATCHTIME_50.bat timmaythetoolman 50

:: Set defaults
set CHANNEL=%1
if "%CHANNEL%"=="" set CHANNEL=timmaythetoolman
set COUNT=%2
if "%COUNT%"=="" set COUNT=50

echo Starting Watch Time Booster for channel %CHANNEL% with %COUNT% viewers...

:: Ensure logs folder exists for container mapping
if not exist "logs" mkdir logs

:: Remove any previous container with the same name (best-effort)
docker rm -f watchtime-booster 2>nul

:: Run the container detached, restart unless stopped, headless mode
docker run -d --name watchtime-booster --restart unless-stopped ^
    -e MODE=WATCHTIME ^
    -e CHANNEL_USERNAME=%CHANNEL% ^
    -e VIEWER_COUNT=%COUNT% ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e MAX_VIEWERS=8 ^
    -v "%CD%\logs":/app/logs ^
    streamviewerbot:latest

echo Deployed watchtime-booster container.
echo To monitor logs: docker logs -f watchtime-booster
echo To stop: docker stop watchtime-booster && docker rm watchtime-booster

