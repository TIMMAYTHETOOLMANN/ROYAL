@echo off
echo =========================================
echo   DEPLOYING ENHANCED YOUTUBE BOT
echo =========================================
echo.

REM Cleanup previous deployment
echo Cleaning previous deployment...
docker stop botcore-youtube 2>nul
docker rm botcore-youtube 2>nul

REM Build optimized YouTube image
echo Building optimized YouTube image...
docker build -f Dockerfile.youtube -t botcore-youtube:latest . --no-cache

REM Create directories
if not exist "logs\youtube" mkdir logs\youtube
if not exist "data\youtube" mkdir data\youtube
if not exist "cache\youtube" mkdir cache\youtube

REM Deploy with enhanced resource allocation
echo Deploying YouTube bot with performance optimizations...
docker run -d ^
    --name botcore-youtube ^
    --restart unless-stopped ^
    --memory=1g ^
    --memory-reservation=512m ^
    --cpus=2.0 ^
    --cpu-shares=2048 ^
    -p 5001:5001 ^
    -p 8081:8081 ^
    -v "%cd%/config/appsettings.youtube.json:/app/appsettings.json:ro" ^
    -v "%cd%/logs/youtube:/app/logs" ^
    -v "%cd%/data/youtube:/app/data" ^
    -v "%cd%/cache/youtube:/app/cache" ^
    -e PLATFORM=YouTube ^
    -e DOTNET_gcServer=1 ^
    -e DOTNET_GCHeapCount=4 ^
    -e MOZ_HEADLESS=1 ^
    botcore-youtube:latest

echo.
echo ========================================
echo   YOUTUBE BOT DEPLOYED SUCCESSFULLY!
echo ========================================
echo.
echo Monitoring Commands:
echo   Logs:    docker logs -f botcore-youtube
echo   Stats:   docker stats botcore-youtube
echo   Health:  curl http://localhost:8081/health
echo ========================================
echo.

timeout /t 5 /nobreak >nul

REM Check if container is running
docker ps | findstr botcore-youtube >nul
if %errorlevel% equ 0 (
    echo Container is running successfully!
    docker logs --tail 20 botcore-youtube
) else (
    echo ERROR: Container failed to start
    docker logs botcore-youtube
    exit /b 1
)

