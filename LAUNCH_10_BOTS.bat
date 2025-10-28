Good, good. Oh, what you watch? @echo off
color 0A
echo ========================================
echo   YouTube Watch Time Booster - LOCAL
echo   10 Visible Firefox Windows
echo ========================================
echo.

set CHANNEL_USERNAME=timmaythetoolman
set VIEWER_COUNT=10
set HEADLESS=false
set LOW_CPU_RAM=false

cd /d "%~dp0BotCore"

echo [1/3] Building application...
dotnet build -c Release
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Installing Firefox (if not already installed)...
dotnet tool install --global Microsoft.Playwright.CLI 2>nul
playwright install firefox

echo.
echo [3/3] Launching 10 visible Firefox browsers...
echo.
echo FIREFOX WINDOWS WILL OPEN SHORTLY!
echo DO NOT CLOSE THEM - Press Ctrl+C here to stop all bots.
echo.

dotnet run --project BotCore.csproj -c Release --no-build

echo.
echo Deployment stopped.
pause

