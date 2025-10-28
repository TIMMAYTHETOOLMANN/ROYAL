# 🎯 DEPLOYMENT FLOWCHART

## 📋 Complete Deployment Process

```
┌─────────────────────────────────────────────────────────────┐
│                    START DEPLOYMENT                         │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 1: CONFIGURATION                                      │
│  • Edit config/appsettings.twitch.json                      │
│  • Edit config/appsettings.youtube.json                     │
│  • Set channel names and preferences                        │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 2: VALIDATION                                         │
│  Run: ./deploy-validate.sh                                  │
│  Checks: Docker, Configs, Dockerfiles                       │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ├─────────── FAILED ──────────┐
                  │                             │
                  ▼                             ▼
              SUCCESS                    Fix Issues & Retry
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 3: CHOOSE DEPLOYMENT METHOD                           │
└─────────────────┬───────────────────────────────────────────┘
                  │
        ┌─────────┴─────────┬─────────────────┐
        │                   │                 │
        ▼                   ▼                 ▼
    Individual          Complete          Docker Compose
        │                   │                 │
        │                   │                 │
        ▼                   ▼                 ▼
┌─────────────┐  ┌──────────────────┐  ┌──────────────┐
│  Twitch     │  │  DEPLOY_ALL.bat  │  │  Existing    │
│  deploy-    │  │  or              │  │  docker-     │
│  twitch-    │  │  DEPLOY_ALL.sh   │  │  compose.yml │
│  optimized  │  │                  │  │              │
└──────┬──────┘  └────────┬─────────┘  └──────┬───────┘
       │                  │                   │
       ▼                  │                   │
┌─────────────┐          │                   │
│  YouTube    │          │                   │
│  deploy-    │          │                   │
│  youtube-   │          │                   │
│  optimized  │          │                   │
└──────┬──────┘          │                   │
       │                  │                   │
       └──────────────────┴───────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 4: CONTAINERS BUILDING                                │
│  • Building Twitch image (Chromium + .NET 8)                │
│  • Building YouTube image (Firefox + .NET 8)                │
│  • Estimated time: 2-3 minutes                              │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 5: CONTAINERS STARTING                                │
│  • botcore-twitch  → Port 8080 (512MB RAM, 1 CPU)           │
│  • botcore-youtube → Port 8081 (1GB RAM, 2 CPU)             │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 6: MONITORING DEPLOYMENT                              │
│  Run: deploy-monitoring.bat                                 │
│  Services:                                                  │
│  • Prometheus  → Port 9090                                  │
│  • Grafana     → Port 3000                                  │
│  • Node Exp.   → Port 9100                                  │
│  • cAdvisor    → Port 8082                                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 7: HEALTH CHECK                                       │
│  curl http://localhost:8080/health  (Twitch)                │
│  curl http://localhost:8081/health  (YouTube)               │
└─────────────────┬───────────────────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
    HEALTHY             UNHEALTHY
        │                   │
        │                   ▼
        │           Check Logs & Restart
        │                   │
        └───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  STEP 8: MONITORING & VERIFICATION                          │
│  • Open Grafana: http://localhost:3000                      │
│  • View logs: docker logs -f botcore-twitch                 │
│  • Check stats: docker stats                                │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                  ✅ DEPLOYMENT COMPLETE                      │
│                  System is operational!                     │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Continuous Operation Flow

```
┌────────────────────────────────────────────────────────────┐
│  RUNNING SYSTEM                                            │
└────────┬───────────────────────────────────────────────────┘
         │
         ├──────────────────────────────────────────┐
         │                                          │
         ▼                                          ▼
┌─────────────────┐                      ┌──────────────────┐
│  Twitch Bot     │                      │  YouTube Bot     │
│  • Watch stream │                      │  • Watch videos  │
│  • Send chat    │                      │  • Skip ads      │
│  • Health: 8080 │                      │  • Health: 8081  │
└────────┬────────┘                      └────────┬─────────┘
         │                                        │
         │    ┌─────────────────────────┐         │
         └────▶  Auto Monitoring         ◀────────┘
              │  Every 5 minutes:       │
              │  • Memory cleanup       │
              │  • Browser refresh      │
              │  • Health check         │
              │  • Resource stats       │
              └──────────┬──────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Prometheus          │
              │  Scraping metrics    │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Grafana             │
              │  Visualizing data    │
              └──────────────────────┘
```

---

## 🚨 Error Recovery Flow

```
┌────────────────────────────────────────────────────────────┐
│  ERROR DETECTED                                            │
└────────┬───────────────────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────────────────────────┐
│  ResilientStreamWatcher Activated                          │
│  • Attempt 1: Wait 30s → Retry                             │
│  • Attempt 2: Wait 60s → Retry                             │
│  • Attempt 3: Wait 120s → Retry                            │
└────────┬───────────────────────────────────────────────────┘
         │
         ├──────── SUCCESS ────────┐
         │                         │
         ▼                         ▼
     FAILED                   RECOVERED
         │                         │
         ▼                         │
┌────────────────┐                 │
│  Log Error     │                 │
│  Alert Admin   │                 │
│  Wait & Retry  │                 │
└────────┬───────┘                 │
         │                         │
         └─────────────────────────┘
                   │
                   ▼
         ┌──────────────────┐
         │  Continue Normal │
         │  Operation       │
         └──────────────────┘
```

---

## 📊 Resource Management Flow

```
┌────────────────────────────────────────────────────────────┐
│  PerformanceOptimizedBotService                            │
└────────┬───────────────────────────────────────────────────┘
         │
         ▼
    Every 5 minutes:
         │
         ├──────────────────────────┬─────────────────────────┐
         │                          │                         │
         ▼                          ▼                         ▼
┌────────────────┐       ┌──────────────────┐    ┌──────────────────┐
│  Check Memory  │       │  Browser Cleanup │    │  Cache Cleanup   │
│  Usage         │       │  Close stale (>  │    │  Remove expired  │
│  If > 400MB:   │       │  30min inactive) │    │  entries         │
│  Trigger GC    │       │                  │    │                  │
└────────────────┘       └──────────────────┘    └──────────────────┘
         │                          │                         │
         └──────────────────────────┴─────────────────────────┘
                                    │
                                    ▼
                    ┌──────────────────────────┐
                    │  Log Cleanup Results     │
                    │  • Memory freed          │
                    │  • Browsers closed       │
                    │  • Cache items removed   │
                    └──────────────────────────┘
```

---

## 🎯 Quick Command Reference

| Action | Windows | Linux |
|--------|---------|-------|
| **Deploy All** | `DEPLOY_ALL.bat` | `./DEPLOY_ALL.sh` |
| **Deploy Twitch** | `deploy-twitch-optimized.bat` | `./deploy-twitch-optimized.sh` |
| **Deploy YouTube** | `deploy-youtube-optimized.bat` | `./deploy-youtube-optimized.sh` |
| **Start Monitor** | `deploy-monitoring.bat` | `docker-compose -f docker-compose.monitoring.yml up -d` |
| **Validate** | `bash deploy-validate.sh` | `./deploy-validate.sh` |
| **View Logs** | `docker logs -f botcore-twitch` | Same |
| **Check Health** | `curl localhost:8080/health` | Same |
| **Stop All** | `docker stop botcore-twitch botcore-youtube` | Same |

---

**Follow the flow → Deploy with confidence! 🚀**
@echo off
REM =====================================================
REM  COMPLETE DEPLOYMENT - ALL PLATFORMS + MONITORING
REM =====================================================

echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║  STREAMING BOT - COMPLETE DEPLOYMENT SUITE            ║
echo ║  Performance Enhanced - Production Ready              ║
echo ╚════════════════════════════════════════════════════════╝
echo.

REM Check Docker
echo [1/5] Validating Docker environment...
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running! Please start Docker Desktop.
    pause
    exit /b 1
)
echo ✅ Docker is running
echo.

REM Create directories
echo [2/5] Creating directory structure...
if not exist "logs\twitch" mkdir logs\twitch
if not exist "logs\youtube" mkdir logs\youtube
if not exist "data\twitch" mkdir data\twitch
if not exist "data\youtube" mkdir data\youtube
if not exist "cache\youtube" mkdir cache\youtube
echo ✅ Directories created
echo.

REM Deploy Twitch Bot
echo [3/5] Deploying Twitch Bot...
echo ----------------------------------------
docker stop botcore-twitch 2>nul
docker rm botcore-twitch 2>nul
docker build -f Dockerfile.twitch -t botcore-twitch:latest . --no-cache || goto :error

docker run -d ^
    --name botcore-twitch ^
    --restart unless-stopped ^
    --memory=512m ^
    --cpus=1.0 ^
    -p 5000:5000 ^
    -p 8080:8080 ^
    -v "%cd%/config/appsettings.twitch.json:/app/appsettings.json:ro" ^
    -v "%cd%/logs/twitch:/app/logs" ^
    -v "%cd%/data/twitch:/app/data" ^
    -e PLATFORM=Twitch ^
    botcore-twitch:latest || goto :error

echo ✅ Twitch Bot deployed
echo.

REM Deploy YouTube Bot
echo [4/5] Deploying YouTube Bot...
echo ----------------------------------------
docker stop botcore-youtube 2>nul
docker rm botcore-youtube 2>nul
docker build -f Dockerfile.youtube -t botcore-youtube:latest . --no-cache || goto :error

docker run -d ^
    --name botcore-youtube ^
    --restart unless-stopped ^
    --memory=1g ^
    --cpus=2.0 ^
    -p 5001:5001 ^
    -p 8081:8081 ^
    -v "%cd%/config/appsettings.youtube.json:/app/appsettings.json:ro" ^
    -v "%cd%/logs/youtube:/app/logs" ^
    -v "%cd%/data/youtube:/app/data" ^
    -v "%cd%/cache/youtube:/app/cache" ^
    -e PLATFORM=YouTube ^
    botcore-youtube:latest || goto :error

echo ✅ YouTube Bot deployed
echo.

REM Start Monitoring
echo [5/5] Starting Monitoring Stack...
echo ----------------------------------------
docker-compose -f docker-compose.monitoring.yml up -d || goto :error
echo ✅ Monitoring stack started
echo.

REM Wait for services
echo Waiting for services to initialize...
timeout /t 10 /nobreak >nul

REM Display status
echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║            DEPLOYMENT SUCCESSFUL! 🚀                   ║
echo ╚════════════════════════════════════════════════════════╝
echo.
echo 📊 RUNNING SERVICES:
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | findstr "botcore\|grafana\|prometheus"
echo.
echo 🌐 ACCESS POINTS:
echo   • Grafana Dashboard: http://localhost:3000 (admin/admin)
echo   • Prometheus:        http://localhost:9090
echo   • Twitch Health:     http://localhost:8080/health
echo   • YouTube Health:    http://localhost:8081/health
echo.
echo 📋 MONITORING COMMANDS:
echo   • Twitch Logs:   docker logs -f botcore-twitch
echo   • YouTube Logs:  docker logs -f botcore-youtube
echo   • Resources:     docker stats
echo.
echo 💡 TIP: Open Grafana to view real-time metrics!
echo.
pause
exit /b 0

:error
echo.
echo ❌ DEPLOYMENT FAILED!
echo Check the error messages above.
echo.
echo Troubleshooting:
echo   1. Check Docker is running: docker info
echo   2. Check disk space: docker system df
echo   3. View logs: docker logs botcore-twitch
echo.
pause
exit /b 1

