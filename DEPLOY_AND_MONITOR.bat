@echo off
REM ========================================================================
REM JARVIS 3.0 - COMPLETE DEPLOYMENT AND MONITORING
REM ========================================================================

echo.
echo ╔══════════════════════════════════════════════════════════════════╗
echo ║          JARVIS 3.0 - DEPLOYING ENHANCED BOT SYSTEM             ║
echo ╚══════════════════════════════════════════════════════════════════╝
echo.

REM Step 1: Final Validation
echo [1/5] Running pre-deployment validation...
powershell -ExecutionPolicy Bypass -File .\VALIDATE_DEPLOYMENT.ps1
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Validation failed! Please review errors above.
    pause
    exit /b 1
)
echo ✓ Validation passed
echo.

REM Step 2: Build Docker Images
echo [2/5] Building Docker images...
docker-compose -f docker-compose.fortified.yml build
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Docker build failed!
    pause
    exit /b 1
)
echo ✓ Docker images built successfully
echo.

REM Step 3: Deploy Containers
echo [3/5] Deploying containers...
docker-compose -f docker-compose.fortified.yml up -d
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Deployment failed!
    pause
    exit /b 1
)
echo ✓ Containers deployed successfully
echo.

REM Step 4: Wait for startup
echo [4/5] Waiting for services to initialize...
timeout /t 10 /nobreak > nul
echo.

REM Step 5: Verify Deployment
echo [5/5] Verifying deployment status...
echo.
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.

echo ╔══════════════════════════════════════════════════════════════════╗
echo ║                  DEPLOYMENT COMPLETE                             ║
echo ╚══════════════════════════════════════════════════════════════════╝
echo.
echo Services Running:
echo   • BotCore (Twitch): http://localhost:5000
echo   • BotCore (YouTube): http://localhost:5001
echo   • Prometheus: http://localhost:9090
echo   • Grafana: http://localhost:3000
echo.
echo Monitoring:
echo   • View logs: docker logs -f botcore-twitch
echo   • View all logs: docker-compose -f docker-compose.fortified.yml logs -f
echo   • Health check: curl http://localhost:8080/health
echo   • Metrics: curl http://localhost:9090/metrics
echo.
echo Starting live monitoring in 5 seconds...
timeout /t 5 /nobreak > nul

REM Launch monitoring
start powershell -ExecutionPolicy Bypass -File .\MONITOR_DEPLOYMENT.ps1 -ComposeFile docker-compose.fortified.yml -MonitorDurationMinutes 60

echo.
echo ✓ Deployment successful! Monitoring dashboard launched.
echo   Press Ctrl+C to exit this window (monitoring will continue)
echo.
pause

