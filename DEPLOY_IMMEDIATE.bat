@echo off
REM ============================================
REM INSTANT DEPLOY - All Platforms Immediately
REM (Use when streams are already live)
REM ============================================

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║   INSTANT DEPLOY - ALL PLATFORMS (IMMEDIATE MODE)          ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo This will deploy viewers immediately on ALL 5 platforms.
echo Use this when your streams are ALREADY LIVE.
echo.
echo For pre-stream mode (recommended), use: DEPLOY_PRESTREAM.bat
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
echo [1/5] Deploying Trovo bot (15-35 viewers)...
docker run -d --name trovo-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="trovo" ^
  -e MIN_VIEWERS="15" ^
  -e MAX_VIEWERS="35" ^
  -e PRE_STREAM_MODE="false" ^
  -e ENABLE_CHAT="true" ^
  -e CHAT_ENGAGEMENT_PERCENT="35" ^
  -e HEADLESS="true" ^
  -e ENABLE_STAGGERING="true" ^
  -e STAGGER_DELAY_MIN="3000" ^
  -e STAGGER_DELAY_MAX="8000" ^
  -e ENABLE_DYNAMIC_ENTRY="true" ^
  -e PROXY_LIST_PATH="/app/proxies.txt" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\trovo:/app/logs" ^
  stream-viewer-bot:latest

echo [2/5] Deploying Kick bot (20-50 viewers)...
docker run -d --name kick-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="kick" ^
  -e MIN_VIEWERS="20" ^
  -e MAX_VIEWERS="50" ^
  -e PRE_STREAM_MODE="false" ^
  -e ENABLE_CHAT="true" ^
  -e CHAT_ENGAGEMENT_PERCENT="40" ^
  -e HEADLESS="true" ^
  -e ENABLE_STAGGERING="true" ^
  -e STAGGER_DELAY_MIN="4000" ^
  -e STAGGER_DELAY_MAX="10000" ^
  -e ENABLE_DYNAMIC_ENTRY="true" ^
  -e PROXY_LIST_PATH="/app/proxies.txt" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\kick:/app/logs" ^
  stream-viewer-bot:latest

echo [3/5] Deploying YouTube bot (25-60 viewers)...
docker run -d --name youtube-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="youtube" ^
  -e MIN_VIEWERS="25" ^
  -e MAX_VIEWERS="60" ^
  -e PRE_STREAM_MODE="false" ^
  -e ENABLE_CHAT="true" ^
  -e CHAT_ENGAGEMENT_PERCENT="25" ^
  -e HEADLESS="true" ^
  -e ENABLE_STAGGERING="true" ^
  -e STAGGER_DELAY_MIN="5000" ^
  -e STAGGER_DELAY_MAX="12000" ^
  -e ENABLE_DYNAMIC_ENTRY="true" ^
  -e PROXY_LIST_PATH="/app/proxies.txt" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\youtube:/app/logs" ^
  stream-viewer-bot:latest

echo [4/5] Deploying Twitch bot (30-70 viewers)...
docker run -d --name twitch-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="twitch" ^
  -e MIN_VIEWERS="30" ^
  -e MAX_VIEWERS="70" ^
  -e PRE_STREAM_MODE="false" ^
  -e ENABLE_CHAT="true" ^
  -e CHAT_ENGAGEMENT_PERCENT="30" ^
  -e HEADLESS="true" ^
  -e ENABLE_STAGGERING="true" ^
  -e STAGGER_DELAY_MIN="3500" ^
  -e STAGGER_DELAY_MAX="9000" ^
  -e ENABLE_DYNAMIC_ENTRY="true" ^
  -e PROXY_LIST_PATH="/app/proxies.txt" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\twitch:/app/logs" ^
  stream-viewer-bot:latest

echo [5/5] Deploying Rumble bot (4-15 viewers, minimum 4)...
docker run -d --name rumble-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="rumble" ^
  -e MIN_VIEWERS="4" ^
  -e MAX_VIEWERS="15" ^
  -e PRE_STREAM_MODE="false" ^
  -e ENABLE_CHAT="true" ^
  -e CHAT_ENGAGEMENT_PERCENT="35" ^
  -e HEADLESS="true" ^
  -e ENABLE_STAGGERING="true" ^
  -e STAGGER_DELAY_MIN="4000" ^
  -e STAGGER_DELAY_MAX="9000" ^
  -e ENABLE_DYNAMIC_ENTRY="true" ^
  -e PROXY_LIST_PATH="/app/proxies.txt" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\rumble:/app/logs" ^
  stream-viewer-bot:latest

echo.
echo ═══════════════════════════════════════════════════════════
echo ✅ ALL PLATFORMS DEPLOYED - IMMEDIATE MODE!
echo ═══════════════════════════════════════════════════════════
echo.
echo Viewers are deploying now with staggered entry.
echo Dynamic fluctuation will keep viewers coming and going naturally.
echo.
echo View status: docker ps
echo View logs: docker logs -f [container-name]
echo.
echo Available containers:
echo   - trovo-bot
echo   - kick-bot
echo   - youtube-bot
echo   - twitch-bot
echo   - rumble-bot
echo.
echo Stop all: docker stop trovo-bot kick-bot youtube-bot twitch-bot rumble-bot
echo Remove all: docker rm trovo-bot kick-bot youtube-bot twitch-bot rumble-bot
echo.
pause

