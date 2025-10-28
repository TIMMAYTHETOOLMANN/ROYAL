I. @echo off
REM ========================================================================
REM SOPHISTICATED DOCKER DEPLOYMENT - Windows Batch Launcher
REM ========================================================================

echo ========================================================================
echo    JARVIS 2.0 - SOPHISTICATED DOCKER DEPLOYMENT
echo ========================================================================
echo.

PowerShell -ExecutionPolicy Bypass -File "%~dp0DEPLOY_SOPHISTICATED.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Deployment failed with code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [SUCCESS] Press any key to exit...
pause > nul
# Prometheus Configuration for BotCore Monitoring
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    monitor: 'botcore-monitor'

# Scrape configurations
scrape_configs:
  # BotCore Application Metrics
  - job_name: 'botcore'
    static_configs:
      - targets: ['botcore:5000']
        labels:
          service: 'botcore'
          environment: 'production'
    
  # Redis Metrics (if redis_exporter is added)
  - job_name: 'redis'
    static_configs:
      - targets: ['redis:6379']
        labels:
          service: 'redis'
    
  # Prometheus Self-Monitoring
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

# Alerting rules (optional)
alerting:
  alertmanagers:
    - static_configs:
        - targets: []

# Rule files
rule_files:
  # - "alerts/*.yml"

