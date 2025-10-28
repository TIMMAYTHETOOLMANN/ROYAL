@echo off
REM ========================================================================
REM MULTI-PLATFORM STAGED STANDBY DEPLOYMENT
REM Twitch + YouTube Pre-Stream Monitor Launcher
REM ========================================================================

echo ========================================================================
echo    JARVIS 2.0 - MULTI-PLATFORM STAGED STANDBY DEPLOYMENT
echo    Twitch + YouTube Pre-Stream Monitor
echo ========================================================================
echo.

PowerShell -ExecutionPolicy Bypass -File "%~dp0DEPLOY_MULTIPLATFORM.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Deployment failed with code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [SUCCESS] Multi-platform standby mode active.
echo Monitors are watching BOTH Twitch and YouTube for your streams.
echo Press any key to exit (monitors will continue running)...
pause > nul
# Prometheus Configuration for Multi-Platform BotCore Monitoring
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    monitor: 'botcore-multiplatform-monitor'

# Scrape configurations
scrape_configs:
  # BotCore Twitch Metrics
  - job_name: 'botcore-twitch'
    static_configs:
      - targets: ['botcore-twitch:5000']
        labels:
          service: 'botcore'
          platform: 'twitch'
          environment: 'production'
  
  # BotCore YouTube Metrics
  - job_name: 'botcore-youtube'
    static_configs:
      - targets: ['botcore-youtube:5000']
        labels:
          service: 'botcore'
          platform: 'youtube'
          environment: 'production'
    
  # Redis Metrics
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

