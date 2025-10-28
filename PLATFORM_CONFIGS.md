# 🎯 PLATFORM-SPECIFIC CONFIGURATION TEMPLATES

## Trovo Configuration (Optimized)
```env
TARGET_STREAM_URL=https://trovo.live/s/timmaythetoolman
PLATFORM=trovo
MIN_VIEWERS=15
MAX_VIEWERS=35
HEADLESS=true
ENABLE_STAGGERING=true
STAGGER_DELAY_MIN=3000
STAGGER_DELAY_MAX=8000
ENABLE_DYNAMIC_ENTRY=true
ENABLE_CHAT=true
USERNAME=timmaythetoolman
MIN_CHAT_DELAY=45
# URL auto-constructed: https://kick.com/timmaythetoolman
MAX_CHAT_DELAY=180
AGGRESSIVE_CHAT=false
PROXY_LIST_PATH=/app/proxies.txt
```

## Kick Configuration (Optimized)
```env
TARGET_STREAM_URL=https://kick.com/your-channel
PLATFORM=kick
MIN_VIEWERS=20
MAX_VIEWERS=50
HEADLESS=true
ENABLE_STAGGERING=true
STAGGER_DELAY_MIN=4000
STAGGER_DELAY_MAX=10000
ENABLE_DYNAMIC_ENTRY=true
USERNAME=timmaythetoolman
CHAT_ENGAGEMENT_PERCENT=40
# URL auto-constructed: https://youtube.com/@timmaythetoolman/live
MIN_CHAT_DELAY=60
MAX_CHAT_DELAY=200
AGGRESSIVE_CHAT=false
PROXY_LIST_PATH=/app/proxies.txt
```

## YouTube Configuration (Optimized)
```env
TARGET_STREAM_URL=https://youtube.com/watch?v=YOUR_VIDEO_ID
PLATFORM=youtube
MIN_VIEWERS=25
MAX_VIEWERS=60
HEADLESS=true
ENABLE_STAGGERING=true
STAGGER_DELAY_MIN=5000
STAGGER_DELAY_MAX=12000
ENABLE_DYNAMIC_ENTRY=true
ENABLE_CHAT=true
CHAT_ENGAGEMENT_PERCENT=25
MIN_CHAT_DELAY=90
MAX_CHAT_DELAY=240
AGGRESSIVE_CHAT=false
PROXY_LIST_PATH=/app/proxies.txt
```

USERNAME=timmaythetoolman
```env
# URL auto-constructed: https://twitch.tv/timmaythetoolman
TARGET_STREAM_URL=https://twitch.tv/your-channel
PLATFORM=twitch
MIN_VIEWERS=30
MAX_VIEWERS=70
HEADLESS=true
ENABLE_STAGGERING=true
STAGGER_DELAY_MIN=3500
STAGGER_DELAY_MAX=9000
ENABLE_DYNAMIC_ENTRY=true
ENABLE_CHAT=true
CHAT_ENGAGEMENT_PERCENT=30
MIN_CHAT_DELAY=50
MAX_CHAT_DELAY=160
AGGRESSIVE_CHAT=false
PROXY_LIST_PATH=/app/proxies.txt
```

## Conservative Strategy (All Platforms)
```env
MIN_VIEWERS=10
MAX_VIEWERS=25
STAGGER_DELAY_MIN=5000
STAGGER_DELAY_MAX=12000
CHAT_ENGAGEMENT_PERCENT=25
AGGRESSIVE_CHAT=false
```

## Aggressive Strategy (All Platforms)
```env
MIN_VIEWERS=30
MAX_VIEWERS=100
STAGGER_DELAY_MIN=2000
STAGGER_DELAY_MAX=5000
CHAT_ENGAGEMENT_PERCENT=45
AGGRESSIVE_CHAT=true
```
@echo off
REM ============================================
REM Multi-Platform Stream Viewer Bot Launcher
REM ============================================

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║   MULTI-PLATFORM STREAM VIEWER BOT - QUICK DEPLOY         ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

:MENU
echo Select Platform:
echo [1] Trovo
echo [2] Kick
echo [3] YouTube
echo [4] Twitch
echo [5] All Platforms
echo [6] Stop All
echo [7] View Logs
echo [0] Exit
echo.

set /p choice="Enter choice: "

if "%choice%"=="1" goto TROVO
if "%choice%"=="2" goto KICK
if "%choice%"=="3" goto YOUTUBE
if "%choice%"=="4" goto TWITCH
if "%choice%"=="5" goto ALL
if "%choice%"=="6" goto STOP
if "%choice%"=="7" goto LOGS
if "%choice%"=="0" goto END

echo Invalid choice!
goto MENU

:TROVO
echo.
echo Starting Trovo bot...
docker-compose -f docker-compose.multi-platform.yml up -d trovo-bot
echo ✅ Trovo bot started!
echo View logs: docker logs -f trovo-viewer-bot
pause
goto MENU

:KICK
echo.
echo Starting Kick bot...
docker-compose -f docker-compose.multi-platform.yml up -d kick-bot
echo ✅ Kick bot started!
echo View logs: docker logs -f kick-viewer-bot
pause
goto MENU

:YOUTUBE
echo.
echo Starting YouTube bot...
docker-compose -f docker-compose.multi-platform.yml up -d youtube-bot
echo ✅ YouTube bot started!
echo View logs: docker logs -f youtube-viewer-bot
pause
goto MENU

:TWITCH
echo.
echo Starting Twitch bot...
docker-compose -f docker-compose.multi-platform.yml up -d twitch-bot
echo ✅ Twitch bot started!
echo View logs: docker logs -f twitch-viewer-bot
pause
goto MENU

:ALL
echo.
echo Starting ALL platform bots...
docker-compose -f docker-compose.multi-platform.yml up -d trovo-bot kick-bot youtube-bot twitch-bot
echo ✅ All bots started!
echo.
echo View status: docker-compose -f docker-compose.multi-platform.yml ps
pause
goto MENU

:STOP
echo.
echo Stopping all bots...
docker-compose -f docker-compose.multi-platform.yml down
echo ✅ All bots stopped!
pause
goto MENU

:LOGS
echo.
echo Select platform to view logs:
echo [1] Trovo
echo [2] Kick
echo [3] YouTube
echo [4] Twitch
echo [5] All
set /p logchoice="Enter choice: "

if "%logchoice%"=="1" docker logs -f trovo-viewer-bot
if "%logchoice%"=="2" docker logs -f kick-viewer-bot
if "%logchoice%"=="3" docker logs -f youtube-viewer-bot
if "%logchoice%"=="4" docker logs -f twitch-viewer-bot
if "%logchoice%"=="5" docker-compose -f docker-compose.multi-platform.yml logs -f
goto MENU

:END
echo.
echo Goodbye!
exit

