@echo off
echo ================================================================
echo   ADAPTIVE WATCH TIME BOOSTER - JARVIS 2.0 DEPLOYMENT
echo ================================================================
echo.
echo Target: 50+ viewers on your YouTube channel content
echo Mode: 24/7 operation while you sleep
echo Content: VODs, Shorts, Videos
echo.

echo [1/4] Stopping any existing bots...
docker stop bot-watchtime 2>nul
docker rm bot-watchtime 2>nul
echo Done.
echo.

echo [2/4] Building Docker image...
docker build -t streamviewerbot:latest .
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED! Check errors above.
    pause
    exit /b 1
)
echo Done.
echo.

echo [3/4] Deploying Adaptive Watch Time Booster...
echo.
echo Configuration:
echo   - Channel: timmaythetoolman
echo   - Viewers: 50
echo   - Mode: Headless (optimized)
echo   - Low Resource: Enabled
echo   - Auto-restart: Yes
echo.

docker run -d --name bot-watchtime ^
    -e MODE=WATCHTIME ^
    -e CHANNEL_USERNAME=timmaythetoolman ^
    -e VIEWER_COUNT=50 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

if %ERRORLEVEL% EQU 0 (
    echo ✓ Watch Time Booster deployed successfully!
) else (
    echo ✗ Deployment failed!
    pause
    exit /b 1
)
echo.

echo [4/4] Verifying deployment...
timeout /t 15 /nobreak >nul
docker ps --filter "name=bot-watchtime"
echo.

echo ================================================================
echo   MONITORING WATCH TIME BOOSTER
echo ================================================================
timeout /t 10 /nobreak >nul
docker logs bot-watchtime 2>&1 | findstr /C:"Launching" /C:"bots launched" /C:"Active viewers" /C:"Found"
echo.

echo ================================================================
echo   DEPLOYMENT COMPLETE - RUNNING 24/7
echo ================================================================
echo.
echo The watch time booster is now running continuously.
echo It will automatically distribute 50+ viewers across your:
echo   - Previous live streams
echo   - YouTube Shorts
echo   - Regular videos
echo.
echo Monitor anytime with:
echo   docker logs -f bot-watchtime
echo.
echo Check status:
echo   docker ps --filter "name=bot-watchtime"
echo.
echo Stop when needed:
echo   docker stop bot-watchtime
echo.
pause

