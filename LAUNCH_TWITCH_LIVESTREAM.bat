@echo off
color 0C
echo ============================================
echo   TWITCH LIVE VIEWERS BOOSTER - LOCAL
echo   10 Visible Firefox Windows
echo   Target: Twitch Live Streams
echo ============================================
echo.

set CHANNEL_USERNAME=timmaythetoolman
set VIEWER_COUNT=10
set HEADLESS=false
set LOW_CPU_RAM=false

cd /d "%~dp0BotCore"

echo [1/3] Building Twitch Live Viewer Bot...
dotnet build -c Release -p:StartupObject=BotCore.DockerEntryPoint
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Installing Firefox (Twitch-optimized)...
dotnet tool install --global Microsoft.Playwright.CLI 2>nul
playwright install firefox

echo.
echo [3/3] Launching 10 Firefox windows for TWITCH live viewers...
echo.
echo TARGET: Twitch.tv live stream
echo MODE: Live Viewer Count Boost
echo.
echo FIREFOX WINDOWS WILL OPEN SHORTLY!
echo All windows will watch your LIVE TWITCH STREAM
echo DO NOT CLOSE THEM - Press Ctrl+C here to stop all bots.
echo.

rem Run with DockerEntryPoint in LIVESTREAM mode for Twitch
set MODE=LIVESTREAM
dotnet run --project BotCore.csproj -c Release --no-build

echo.
echo Twitch Live Viewer Bot stopped.
pause

