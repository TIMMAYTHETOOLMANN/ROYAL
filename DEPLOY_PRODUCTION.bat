@echo off
REM ============================================================
REM PRODUCTION DEPLOYMENT SCRIPT
REM Stream Viewer Bot - Automated Docker Deployment
REM ============================================================

echo.
echo ========================================
echo  JARVIS 2.0 - DEPLOYMENT INITIATED
echo ========================================
echo.

REM Stop any existing containers
echo [1/5] Terminating existing containers...
docker-compose down 2>nul
docker stop bot-orchestrator bot-worker 2>nul
docker rm bot-orchestrator bot-worker 2>nul

REM Verify Docker image exists
echo.
echo [2/5] Verifying Docker image...
docker images streamviewerbot:latest
if %errorlevel% neq 0 (
    echo ERROR: Docker image not found. Building now...
    docker build -t streamviewerbot:latest .
)

REM Create necessary directories
echo.
echo [3/5] Preparing environment...
if not exist "logs" mkdir logs
if not exist "logs\kick" mkdir logs\kick
if not exist "logs\rumble" mkdir logs\rumble
if not exist "logs\trovo" mkdir logs\trovo
if not exist "config" mkdir config

REM Ensure proxy and chat config files exist
if not exist "proxies.txt" echo # Add proxies in format IP:PORT:USERNAME:PASSWORD > proxies.txt
if not exist "chat-config.txt" echo Hello!;Great stream!;Awesome content!;Love this! > chat-config.txt

REM Deploy using Docker Compose
echo.
echo [4/5] Deploying bot infrastructure...
docker-compose -f docker-compose.prod.yml up -d

REM Verify deployment
echo.
echo [5/5] Verifying deployment...
timeout /t 5 /nobreak >nul
docker ps --filter "name=bot-"

echo.
echo ========================================
echo  DEPLOYMENT COMPLETE
echo ========================================
echo.
echo Bot orchestrator: http://localhost:5000
echo View logs: docker logs -f bot-orchestrator
echo Stop all: docker-compose -f docker-compose.prod.yml down
echo.
echo System operational. Standing by.
echo.

pause

