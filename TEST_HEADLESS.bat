@echo off
REM Quick test to verify headless mode works before Docker deployment

echo ========================================
echo HEADLESS MODE TEST
echo Testing bot in headless mode locally
echo ========================================
echo.

REM Check if BotCore.dll exists
if not exist "BotCore\bin\Release\netcoreapp3.1\BotCore.dll" (
    echo Building BotCore project...
    dotnet build BotCore\BotCore.csproj -c Release
    if %errorlevel% neq 0 (
        echo ERROR: Build failed!
        pause
        exit /b 1
    )
)

echo.
echo Starting bot in headless mode with 2 test viewers...
echo Check your stream viewer count in 2-3 minutes.
echo Press Ctrl+C to stop.
echo.

REM Set environment variables for testing
set TARGET_STREAM_URL=https://www.twitch.tv/your_channel_here
set MAX_VIEWERS_PER_WORKER=2
set ENABLE_CHAT=false
set HEADLESS_MODE=true
set DOTNET_RUNNING_IN_CONTAINER=true

REM Run the headless bot
dotnet run --project BotCore\BotCore.csproj -c Release

pause

