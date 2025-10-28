@echo off
REM DEPLOY_OVERNIGHT.bat
REM Quick overnight deployment - 50 viewers, auto-recovery, headless
REM This is your "set and forget" script for overnight watch time farming

echo ╔════════════════════════════════════════════════════════════╗
echo ║   OVERNIGHT WATCH TIME BOOSTER - JARVIS 2.0               ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

:: Configuration
set CHANNEL=timmaythetoolman
set TOTAL_VIEWERS=50
set VIEWERS_PER_CONTAINER=10

echo Configuration:
echo   Channel: %CHANNEL%
echo   Total Viewers: %TOTAL_VIEWERS%
echo   Distribution: %VIEWERS_PER_CONTAINER% viewers per container
echo   Mode: Headless (no GUI)
echo   Auto-Recovery: Enabled
echo   Duration: Continuous (until stopped)
echo.

:: Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker Desktop is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo [✓] Docker is running
echo.

:: Clean up old containers
echo [CLEANUP] Removing old watchtime containers...
for /f "tokens=*" %%C in ('docker ps -aq --filter "name=watchtime-" 2^>nul') do (
    docker rm -f %%C >nul 2>&1
)
echo [✓] Cleanup complete
echo.

:: Calculate number of containers
set /a NUM=(%TOTAL_VIEWERS% + %VIEWERS_PER_CONTAINER% - 1) / %VIEWERS_PER_CONTAINER%

echo [DEPLOY] Launching %NUM% containers with %TOTAL_VIEWERS% total viewers...
echo.

:: Deploy containers
setlocal enabledelayedexpansion
for /l %%i in (1,1,%NUM%) do (
    set /a START_INDEX=(%%i - 1) * %VIEWERS_PER_CONTAINER%
    set /a REMAIN=%TOTAL_VIEWERS% - (!START_INDEX!)
    if !REMAIN! LSS %VIEWERS_PER_CONTAINER% (
        set VIEWERS=!REMAIN!
    ) else (
        set VIEWERS=%VIEWERS_PER_CONTAINER%
    )
    
    set CONTAINER_NAME=watchtime-%%i
    echo [%%i/%NUM%] Launching !CONTAINER_NAME! with !VIEWERS! viewers...
    
    docker run -d --name !CONTAINER_NAME! --restart unless-stopped ^
        -e MODE=WATCHTIME ^
        -e CHANNEL_USERNAME=%CHANNEL% ^
        -e VIEWER_COUNT=!VIEWERS! ^
        -e HEADLESS=false ^
        -e LOW_CPU_RAM=true ^
        -e MAX_VIEWERS=8 ^
        -v "%CD%\logs\!CONTAINER_NAME!":/app/logs ^
        streamviewerbot:latest >nul 2>&1
    
    if errorlevel 1 (
        echo    [ERROR] Failed to launch !CONTAINER_NAME!
    ) else (
        echo    [✓] !CONTAINER_NAME! deployed successfully
    )
)
endlocal

echo.
echo ════════════════════════════════════════════════════════════
echo   DEPLOYMENT COMPLETE
echo ════════════════════════════════════════════════════════════
echo.
echo Containers deployed: %NUM%
echo Total viewers: %TOTAL_VIEWERS%
echo Channel: %CHANNEL%
echo.
echo The system will now run continuously and automatically:
echo   • Discover your videos, shorts, and VODs
echo   • Distribute viewers across content
echo   • Replace failed sessions
echo   • Accumulate watch time 24/7
echo.
echo MONITORING:
echo   • View all containers: docker ps --filter name=watchtime-
echo   • Monitor logs: docker logs -f watchtime-1
echo   • Check stats: docker stats --filter name=watchtime-
echo.
echo SHUTDOWN:
echo   • Run: STOP_WATCHTIME_ALL.bat
echo   • Or: docker stop $(docker ps -q --filter name=watchtime-)
echo.
echo ════════════════════════════════════════════════════════════
echo   GO TO BED. WAKE UP TO BOOSTED METRICS. 🚀
echo ════════════════════════════════════════════════════════════
echo.

pause

