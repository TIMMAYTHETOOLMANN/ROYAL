@echo off
echo ================================================================
echo   DEPLOYING TIMEOUT FIX - JARVIS 2.0 FULL REBUILD
echo ================================================================
echo.

echo [1/5] Stopping old containers...
docker stop bot-youtube bot-twitch bot-kick 2>nul
docker rm bot-youtube bot-twitch bot-kick 2>nul
echo Done.
echo.

echo [2/5] Building fixed Docker image (this may take 2-3 minutes)...
docker build -t streamviewerbot:latest .
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED! Check errors above.
    pause
    exit /b 1
)
echo Done.
echo.

echo [3/5] Verifying image built successfully...
docker images streamviewerbot:latest
echo.

echo [4/5] Deploying bots with AGGRESSIVE viewer counts...
echo.
echo Deploying YouTube bot (15 viewers to ensure 8+ active)...
docker run -d --name bot-youtube ^
    -e PLATFORM=YOUTUBE ^
    -e USERNAME=timmaythetoolman ^
    -e STREAM_URL=https://www.youtube.com/live/cpQCDstX000 ^
    -e VIEWER_COUNT=15 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

echo.
echo Deploying Twitch bot (12 viewers)...
docker run -d --name bot-twitch ^
    -e PLATFORM=TWITCH ^
    -e USERNAME=timmaythetoolman ^
    -e VIEWER_COUNT=12 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

echo.
echo Deploying Kick bot (12 viewers)...
docker run -d --name bot-kick ^
    -e PLATFORM=KICK ^
    -e USERNAME=timmaythetoolman ^
    -e VIEWER_COUNT=12 ^
    -e HEADLESS=false ^
    -e LOW_CPU_RAM=true ^
    -e CHAT_ENGAGEMENT=true ^
    --restart unless-stopped ^
    streamviewerbot:latest

echo Done.
echo.

echo [5/5] Verifying deployment...
timeout /t 20 /nobreak >nul
docker ps --filter "name=bot-"
echo.

echo ================================================================
echo   CHECKING YOUTUBE BOT LOGS (should show NO timeout errors)
echo ================================================================
timeout /t 10 /nobreak >nul
docker logs bot-youtube 2>&1 | findstr /C:"Live viewers" /C:"launched successfully" /C:"Error" /C:"Timeout"
echo.

echo ================================================================
echo   DEPLOYMENT COMPLETE
echo ================================================================
echo.
echo Monitor with:
echo   docker logs -f bot-youtube
echo   docker logs -f bot-twitch
echo   docker logs -f bot-kick
echo.
echo Check viewer count:
echo   docker logs bot-youtube 2^>^&1 ^| findstr "Live viewers"
echo.
pause

