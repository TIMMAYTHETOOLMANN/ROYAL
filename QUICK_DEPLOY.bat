@echo off
REM ============================================================
REM QUICK DEPLOY - Single Command Deployment
REM ============================================================

echo.
echo ╔══════════════════════════════════════════════════════╗
echo ║   STREAM VIEWER BOT - QUICK DEPLOY                  ║
echo ║   Docker Deployment Initiated                       ║
echo ╚══════════════════════════════════════════════════════╝
echo.

REM Stop existing containers
docker-compose -f docker-compose.production.yml down 2>nul

REM Deploy
echo [*] Deploying bot infrastructure...
docker-compose -f docker-compose.production.yml --env-file .env.production up -d

REM Status check
timeout /t 3 /nobreak >nul
echo.
echo [✓] Deployment Complete
echo.
docker ps --filter "name=bot-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.
echo ► View logs: docker logs -f bot-orchestrator
echo ► Stop: docker-compose -f docker-compose.production.yml down
echo ► Scale workers: docker-compose -f docker-compose.production.yml up -d --scale bot-worker=5
echo.

