@echo off
REM ========================================================================
REM FORTIFIED DEPLOYMENT LAUNCHER - JARVIS 3.0
REM Quick Launch Script for PowerShell Deployment
REM ========================================================================

echo ========================================================================
echo   JARVIS 3.0 - FORTIFIED MULTI-PLATFORM DEPLOYMENT
echo   Enterprise-Grade Reliability with Resource Management
echo ========================================================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [WARNING] Not running as Administrator. Some features may be limited.
    echo.
)

REM Execute PowerShell deployment script
powershell.exe -ExecutionPolicy Bypass -File ".\deploy-multiplatform.ps1" -Channel "timmaythetoolman" -MaxViewersPerPlatform 50 -MinViewersPerPlatform 30 -CheckIntervalMinutes 3 -Environment "Production"

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Deployment failed with error code: %ERRORLEVEL%
    echo Check logs directory for detailed error information.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================================
echo   DEPLOYMENT COMPLETE - System is now monitoring and ready!
echo ========================================================================
pause
version: '3.8'

services:
  # Twitch Platform Service
  botcore-twitch:
    image: botcore:fortified-v3
    container_name: botcore-twitch
    profiles: ["twitch"]
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 1GB
        reservations:
          cpus: '0.5'
          memory: 512m
    restart: on-failure:3
    environment:
      - PLATFORM=Twitch
      - CHANNEL=${CHANNEL:-timmaythetoolman}
      - MAX_VIEWERS=50
      - MIN_VIEWERS=30
      - CHECK_INTERVAL_MINUTES=3
      - ASPNETCORE_ENVIRONMENT=Production
      - HEADLESS=true
      - MODE=PRESTREAM
      - ENABLE_CHAT=true
      - CHAT_ENGAGEMENT_PERCENT=25
      - MIN_CHAT_DELAY=60
      - MAX_CHAT_DELAY=240
      - LOW_CPU_RAM=true
      - ENABLE_METRICS=true
      - ENABLE_HEALTH_CHECKS=true
    ports:
      - "5000:5000"
    volumes:
      - ./logs/twitch:/app/logs
      - ./config/platforms/twitch:/app/config
      - ./proxies.txt:/app/proxies.txt:ro
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
    networks:
      - botcore-network

  # YouTube Platform Service
  botcore-youtube:
    image: botcore:fortified-v3
    container_name: botcore-youtube
    profiles: ["youtube"]
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 1GB
        reservations:
          cpus: '0.5'
          memory: 512m
    restart: on-failure:3
    environment:
      - PLATFORM=YouTube
      - CHANNEL=${CHANNEL:-timmaythetoolman}
      - MAX_VIEWERS=50
      - MIN_VIEWERS=30
      - CHECK_INTERVAL_MINUTES=3
      - ASPNETCORE_ENVIRONMENT=Production
      - HEADLESS=true
      - MODE=PRESTREAM
      - ENABLE_CHAT=true
      - CHAT_ENGAGEMENT_PERCENT=25
      - MIN_CHAT_DELAY=60
      - MAX_CHAT_DELAY=240
      - LOW_CPU_RAM=true
      - ENABLE_METRICS=true
      - ENABLE_HEALTH_CHECKS=true
    ports:
      - "5001:5000"
    volumes:
      - ./logs/youtube:/app/logs
      - ./config/platforms/youtube:/app/config
      - ./proxies.txt:/app/proxies.txt:ro
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
    networks:
      - botcore-network

  # Monitoring Stack
  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus-multiplatform
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'
    networks:
      - botcore-network

  grafana:
    image: grafana/grafana:latest
    container_name: grafana-multiplatform
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_SECURITY_ADMIN_USER=admin
    volumes:
      - grafana_data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
    depends_on:
      - prometheus
    networks:
      - botcore-network

  alertmanager:
    image: prom/alertmanager:latest
    container_name: alertmanager
    ports:
      - "9093:9093"
    volumes:
      - ./monitoring/alertmanager.yml:/etc/alertmanager/alertmanager.yml
      - alertmanager_data:/alertmanager
    command:
      - '--config.file=/etc/alertmanager/alertmanager.yml'
      - '--storage.path=/alertmanager'
    networks:
      - botcore-network

  redis:
    image: redis:alpine
    container_name: redis-botcore
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - botcore-network

volumes:
  prometheus_data:
  grafana_data:
  alertmanager_data:
  redis_data:

networks:
  botcore-network:
    driver: bridge

