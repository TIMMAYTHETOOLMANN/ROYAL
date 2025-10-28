@echo off
REM ========================================================================
REM STAGED STANDBY DEPLOYMENT - Pre-Stream Monitor Launcher
REM Watches for live signal and auto-deploys viewers
REM ========================================================================

echo ========================================================================
echo    JARVIS 2.0 - STAGED STANDBY DEPLOYMENT
echo    Pre-Stream Monitor - Awaiting Live Signal
echo ========================================================================
echo.

PowerShell -ExecutionPolicy Bypass -File "%~dp0DEPLOY_STANDBY.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Deployment failed with code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [SUCCESS] Standby mode active. Monitor is watching for your stream.
echo Press any key to exit (monitor will continue running)...
pause > nul
version: '3.8'

services:
  # ========================================================================
  # STAGED STANDBY MODE - Pre-Stream Monitor
  # Watches for live signal and auto-deploys sophisticated stack
  # ========================================================================
  botcore-standby:
    build:
      context: .
      dockerfile: Dockerfile.enhanced
    image: botcore:sophisticated
    container_name: botcore-standby
    restart: unless-stopped
    environment:
      # Staged Standby Mode
      - MODE=PRESTREAM
      - ASPNETCORE_ENVIRONMENT=Production
      
      # Your Channel Configuration
      - CHANNEL_USERNAME=timmaythetoolman
      - PLATFORM=twitch
      
      # Auto-Deploy Settings
      - MIN_VIEWERS=30
      - MAX_VIEWERS=50
      - ENABLE_VIEWER_FLUCTUATION=true
      - CHECK_INTERVAL=180
      
      # Browser Configuration
      - HEADLESS=true
      - DISPLAY=:99
      
      # Chat Engagement (activates when live)
      - ENABLE_CHAT=true
      - CHAT_ENGAGEMENT_PERCENT=25
      - MIN_CHAT_DELAY=60
      - MAX_CHAT_DELAY=240
      
      # Resource Management
      - LOW_CPU_RAM=true
      - MAX_CONCURRENT_BROWSERS=50
      
      # Monitoring & Health
      - ENABLE_METRICS=true
      - ENABLE_HEALTH_CHECKS=true
      
      # Infrastructure
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
    
    ports:
      - "5000:5000"  # Metrics & Health checks
    
    volumes:
      - ./logs:/app/logs
      - ./config:/app/config
      - ./proxies.txt:/app/proxies.txt:ro
    
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
    
    deploy:
      resources:
        limits:
          cpus: '4.0'
          memory: 8G
        reservations:
          cpus: '1.0'
          memory: 2G
    
    networks:
      - botcore-network
    
    depends_on:
      redis:
        condition: service_healthy
      prometheus:
        condition: service_started

  # ========================================================================
  # Redis - Distributed Caching
  # ========================================================================
  redis:
    image: redis:7-alpine
    container_name: botcore-redis-standby
    restart: unless-stopped
    command: redis-server --appendonly yes --maxmemory 512mb --maxmemory-policy allkeys-lru
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 3
    networks:
      - botcore-network

  # ========================================================================
  # Prometheus - Metrics Collection
  # ========================================================================
  prometheus:
    image: prom/prometheus:latest
    container_name: botcore-prometheus-standby
    restart: unless-stopped
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--storage.tsdb.retention.time=30d'
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus-data:/prometheus
    networks:
      - botcore-network

  # ========================================================================
  # Grafana - Real-Time Dashboard
  # ========================================================================
  grafana:
    image: grafana/grafana:latest
    container_name: botcore-grafana-standby
    restart: unless-stopped
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_USERS_ALLOW_SIGN_UP=false
      - GF_DASHBOARDS_DEFAULT_HOME_DASHBOARD_PATH=/etc/grafana/provisioning/dashboards/prestream.json
    ports:
      - "3000:3000"
    volumes:
      - grafana-data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards:ro
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources:ro
    networks:
      - botcore-network
    depends_on:
      - prometheus

  # ========================================================================
  # RabbitMQ - Message Queue
  # ========================================================================
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: botcore-rabbitmq-standby
    restart: unless-stopped
    environment:
      - RABBITMQ_DEFAULT_USER=admin
      - RABBITMQ_DEFAULT_PASS=admin
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - botcore-network

volumes:
  redis-data:
    driver: local
  prometheus-data:
    driver: local
  grafana-data:
    driver: local
  rabbitmq-data:
    driver: local

networks:
  botcore-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.26.0.0/16

