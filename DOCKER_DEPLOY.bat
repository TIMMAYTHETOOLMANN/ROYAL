# =================================================================
# DOCKER DEPLOYMENT MANAGER - STREAM VIEWER BOT
# Automated scaling and orchestration for Docker Pro
# =================================================================

@echo off
setlocal enabledelayedexpansion

echo ========================================
echo  STREAM VIEWER BOT - DOCKER DEPLOYMENT
echo ========================================
echo.

:MENU
echo Select deployment mode:
echo.
echo [1] Build Docker Image
echo [2] Start Single Instance (Testing)
echo [3] Start Multi-Instance (5 workers)
echo [4] Start Swarm Mode (Massive Scale)
echo [5] Scale Workers Up/Down
echo [6] View Status
echo [7] View Logs
echo [8] Stop All
echo [9] Clean Everything
echo [0] Exit
echo.
set /p choice="Enter choice: "

if "%choice%"=="1" goto BUILD
if "%choice%"=="2" goto SINGLE
if "%choice%"=="3" goto MULTI
if "%choice%"=="4" goto SWARM
if "%choice%"=="5" goto SCALE
if "%choice%"=="6" goto STATUS
if "%choice%"=="7" goto LOGS
if "%choice%"=="8" goto STOP
if "%choice%"=="9" goto CLEAN
if "%choice%"=="0" goto END
goto MENU

:BUILD
echo.
echo Building Docker image...
docker build -t streamviewerbot:latest .
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    goto MENU
)
echo.
echo ✓ Image built successfully!
pause
goto MENU

:SINGLE
echo.
echo Starting single test instance...
docker-compose up -d bot-orchestrator redis
echo.
echo ✓ Bot orchestrator started on http://localhost:5000
echo View logs: docker logs -f bot-orchestrator
pause
goto MENU

:MULTI
echo.
echo Starting multi-instance deployment (5 workers)...
docker-compose up -d --scale bot-worker=5
echo.
echo ✓ Deployment started!
echo - 1x Orchestrator
echo - 5x Workers
echo - Redis cache
pause
goto MENU

:SWARM
echo.
echo Initializing Docker Swarm mode...
docker swarm init 2>nul
if %errorlevel% equ 0 (
    echo ✓ Swarm initialized
) else (
    echo ! Swarm already active
)
echo.
set /p replicas="Enter number of worker replicas (10-100): "
if "%replicas%"=="" set replicas=10

echo Deploying stack with %replicas% workers...
docker stack deploy -c docker-compose.swarm.yml botstack
timeout /t 2 >nul
docker service scale botstack_bot-worker=%replicas%
echo.
echo ✓ Swarm stack deployed!
echo.
echo Scale command: docker service scale botstack_bot-worker=N
pause
goto MENU

:SCALE
echo.
docker service ls 2>nul | find "botstack" >nul
if %errorlevel% neq 0 (
    echo ERROR: Swarm stack not running. Use option [4] first.
    pause
    goto MENU
)
echo Current services:
docker service ls
echo.
set /p newscale="Enter new worker count: "
docker service scale botstack_bot-worker=%newscale%
echo.
echo ✓ Scaling to %newscale% workers...
pause
goto MENU

:STATUS
echo.
echo === DOCKER STATUS ===
echo.
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.
echo === SWARM SERVICES ===
docker service ls 2>nul
if %errorlevel% neq 0 echo (Swarm mode not active)
echo.
pause
goto MENU

:LOGS
echo.
echo Select log source:
echo [1] Orchestrator
echo [2] Worker
echo [3] All containers
set /p logsrc="Choice: "
if "%logsrc%"=="1" docker logs -f --tail=100 bot-orchestrator
if "%logsrc%"=="2" docker-compose logs -f --tail=100 bot-worker
if "%logsrc%"=="3" docker-compose logs -f --tail=50
goto MENU

:STOP
echo.
echo Stopping all deployments...
docker-compose down
docker stack rm botstack 2>nul
echo ✓ All stopped
pause
goto MENU

:CLEAN
echo.
echo WARNING: This will remove all containers, images, and volumes!
set /p confirm="Type YES to confirm: "
if /i not "%confirm%"=="YES" goto MENU
echo.
echo Cleaning up...
docker-compose down -v
docker stack rm botstack 2>nul
docker system prune -af --volumes
echo ✓ Cleanup complete
pause
goto MENU

:END
echo.
echo Exiting...
exit /b 0

