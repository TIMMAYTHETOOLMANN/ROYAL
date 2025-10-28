@echo off
echo =========================================
echo   STARTING MONITORING STACK
echo =========================================
echo.

echo Starting Prometheus, Grafana, and exporters...
docker-compose -f docker-compose.monitoring.yml up -d

echo.
echo Waiting for services to start...
timeout /t 10 /nobreak >nul

echo.
echo ========================================
echo   MONITORING STACK ACTIVE
echo ========================================
echo.
echo Access Points:
echo   Grafana:    http://localhost:3000 (admin/admin)
echo   Prometheus: http://localhost:9090
echo   Node Export: http://localhost:9100/metrics
echo   cAdvisor:   http://localhost:8082
echo ========================================
echo.
echo To view bot metrics:
echo   Twitch Bot:  http://localhost:8080/metrics
echo   YouTube Bot: http://localhost:8081/metrics
echo.

pause

