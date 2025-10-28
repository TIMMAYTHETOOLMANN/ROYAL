@echo off
echo ================================================================
echo   REDEPLOYING WITH TIMEOUT FIX - JARVIS 2.0
echo ================================================================
echo.

echo [1/4] Stopping existing containers...
docker stop bot-youtube bot-twitch bot-kick 2>nul
docker rm bot-youtube bot-twitch bot-kick 2>nul
echo Done.
echo.

echo [2/4] Building optimized Docker image...
docker build -t streamviewerbot:latest . -q
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED - Attempting recovery...
    docker build -t streamviewerbot:latest .
)
echo Done.
echo.

echo [3/4] Deploying multi-platform bots...
docker run -d --name bot-youtube ^
    -e PLATFORM=YOUTUBE ^
    -e USERNAME=timmaythetoolman ^
    -e STREAM_URL=https://www.youtube.com/live/cpQCDstX000 ^
    -e VIEWER_COUNT=8 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

docker run -d --name bot-twitch ^
    -e PLATFORM=TWITCH ^
    -e USERNAME=timmaythetoolman ^
    -e VIEWER_COUNT=8 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

docker run -d --name bot-kick ^
    -e PLATFORM=KICK ^
    -e USERNAME=timmaythetoolman ^
    -e VIEWER_COUNT=8 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

echo Done.
echo.

echo [4/4] Verifying deployment...
timeout /t 15 /nobreak >nul
docker ps --filter "name=bot-"
echo.

echo ================================================================
echo   DEPLOYMENT COMPLETE - All systems operational
echo ================================================================
echo.
echo Monitor logs with:
echo   docker logs -f bot-youtube
echo   docker logs -f bot-twitch
echo   docker logs -f bot-kick
echo.

