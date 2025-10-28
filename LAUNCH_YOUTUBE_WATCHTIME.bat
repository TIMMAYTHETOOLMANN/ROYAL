@echo off
color 0B
echo ============================================
echo   YOUTUBE WATCH TIME BOOSTER - LOCAL
echo   10 Visible Firefox Windows
echo   Target: YouTube Videos/VODs/Shorts
echo ============================================
echo.

set CHANNEL_USERNAME=timmaythetoolman
set VIEWER_COUNT=10
set HEADLESS=false
set LOW_CPU_RAM=false

cd /d "%~dp0BotCore"

echo [1/3] Building YouTube Watch Time Booster...
dotnet build -c Release -p:StartupObject=WatchTimeBoosterEntryPoint
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Installing Firefox (YouTube-optimized)...
dotnet tool install --global Microsoft.Playwright.CLI 2>nul
playwright install firefox

echo.
echo [3/3] Launching 10 Firefox windows for YOUTUBE watch time...
echo.
echo TARGET: YouTube videos, VODs, and Shorts
echo MODE: Watch Time Accumulation (NOT live viewers)
echo.
echo FIREFOX WINDOWS WILL OPEN SHORTLY!
echo Each window will watch a DIFFERENT YouTube video
echo DO NOT CLOSE THEM - Press Ctrl+C here to stop all bots.
echo.

rem Run with WatchTimeBoosterEntryPoint for YouTube
dotnet run --project BotCore.csproj -c Release --no-build

echo.
echo YouTube Watch Time Booster stopped.
pause

