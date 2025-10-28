@echo off
REM ============================================
REM Pre-Stream Deployment System
REM Deploys all platforms in monitoring mode
REM ============================================

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║   PRE-STREAM AUTO-DEPLOY SYSTEM - ALL PLATFORMS           ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo This will deploy bots on ALL 5 platforms:
echo   - Trovo (15-35 viewers)
echo   - Kick (20-50 viewers)
echo   - YouTube (25-60 viewers)
echo   - Twitch (30-70 viewers)
echo   - Rumble (4-15 viewers, minimum 4)
echo.
echo Bots will check every 3 minutes and auto-deploy when streams go live.
echo.
pause

echo.
echo Creating logs directories...
if not exist logs mkdir logs
if not exist logs\trovo mkdir logs\trovo
if not exist logs\kick mkdir logs\kick
if not exist logs\youtube mkdir logs\youtube
if not exist logs\twitch mkdir logs\twitch
if not exist logs\rumble mkdir logs\rumble

echo.
echo [1/5] Deploying Trovo pre-stream monitor...
docker run -d --name trovo-prestream -e USERNAME="timmaythetoolman" -e PLATFORM="trovo" -e PRE_STREAM_MODE="true" -e MIN_VIEWERS="15" -e MAX_VIEWERS="35" -e ENABLE_CHAT="true" -e CHAT_ENGAGEMENT_PERCENT="35" -e HEADLESS="true" -e PROXY_LIST_PATH="/app/proxies.txt" -v "%cd%\proxies.txt:/app/proxies.txt" -v "%cd%\logs\trovo:/app/logs" stream-viewer-bot:latest

echo [2/5] Deploying Kick pre-stream monitor...
docker run -d --name kick-prestream -e USERNAME="timmaythetoolman" -e PLATFORM="kick" -e PRE_STREAM_MODE="true" -e MIN_VIEWERS="20" -e MAX_VIEWERS="50" -e ENABLE_CHAT="true" -e CHAT_ENGAGEMENT_PERCENT="40" -e HEADLESS="true" -e PROXY_LIST_PATH="/app/proxies.txt" -v "%cd%\proxies.txt:/app/proxies.txt" -v "%cd%\logs\kick:/app/logs" stream-viewer-bot:latest

echo [3/5] Deploying YouTube pre-stream monitor...
docker run -d --name youtube-prestream -e USERNAME="timmaythetoolman" -e PLATFORM="youtube" -e PRE_STREAM_MODE="true" -e MIN_VIEWERS="25" -e MAX_VIEWERS="60" -e ENABLE_CHAT="true" -e CHAT_ENGAGEMENT_PERCENT="25" -e HEADLESS="true" -e PROXY_LIST_PATH="/app/proxies.txt" -v "%cd%\proxies.txt:/app/proxies.txt" -v "%cd%\logs\youtube:/app/logs" stream-viewer-bot:latest

echo [4/5] Deploying Twitch pre-stream monitor...
docker run -d --name twitch-prestream -e USERNAME="timmaythetoolman" -e PLATFORM="twitch" -e PRE_STREAM_MODE="true" -e MIN_VIEWERS="30" -e MAX_VIEWERS="70" -e ENABLE_CHAT="true" -e CHAT_ENGAGEMENT_PERCENT="30" -e HEADLESS="true" -e PROXY_LIST_PATH="/app/proxies.txt" -v "%cd%\proxies.txt:/app/proxies.txt" -v "%cd%\logs\twitch:/app/logs" stream-viewer-bot:latest

echo [5/5] Deploying Rumble pre-stream monitor (Min 4 viewers)...
docker run -d --name rumble-prestream -e USERNAME="timmaythetoolman" -e PLATFORM="rumble" -e PRE_STREAM_MODE="true" -e MIN_VIEWERS="4" -e MAX_VIEWERS="15" -e ENABLE_CHAT="true" -e CHAT_ENGAGEMENT_PERCENT="35" -e HEADLESS="true" -e PROXY_LIST_PATH="/app/proxies.txt" -v "%cd%\proxies.txt:/app/proxies.txt" -v "%cd%\logs\rumble:/app/logs" stream-viewer-bot:latest

echo.
echo ═══════════════════════════════════════════════════════════
echo ✅ ALL PLATFORMS DEPLOYED IN PRE-STREAM MODE!
echo ═══════════════════════════════════════════════════════════
echo.
echo Monitors are now running and checking every 3 minutes.
echo When streams go live, viewers will auto-deploy automatically.
echo.
echo View status: docker ps
echo View logs: docker logs -f trovo-prestream
echo.
echo Stop all: .\STOP_PRESTREAM.bat
echo.
pause

