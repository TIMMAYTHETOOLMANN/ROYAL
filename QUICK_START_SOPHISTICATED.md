# 🎯 QUICK START - 30 SECONDS TO DEPLOYMENT

## ⚡ Fastest Path to Production

### Step 1: Deploy Everything
```cmd
DEPLOY_SOPHISTICATED.bat
```

### Step 2: Verify Health
Open browser: http://localhost:5000/health

### Step 3: Monitor
- Metrics: http://localhost:9090
- Logs: http://localhost:5341
- Dashboards: http://localhost:3000

---

## 🎮 Common Operations

### Start Deployment
```bash
docker-compose -f docker-compose.sophisticated.yml up -d
```

### Stop Deployment
```bash
docker-compose -f docker-compose.sophisticated.yml down
```

### View Logs
```bash
docker-compose -f docker-compose.sophisticated.yml logs -f botcore
```

### Scale to 5 Instances
```bash
docker-compose -f docker-compose.sophisticated.yml up -d --scale botcore=5
```

---

## 🔧 Quick Configuration

Edit `.env` file or set environment variables:

```env
CHANNEL_USERNAME=your_channel
PLATFORM=twitch
MAX_VIEWERS=50
HEADLESS=true
ENABLE_CHAT=true
```

---

## 📊 Access Dashboard

**Grafana**: http://localhost:3000
- User: `admin`
- Pass: `admin`

**Prometheus**: http://localhost:9090

**RabbitMQ**: http://localhost:15672
- User: `admin`
- Pass: `admin`

---

## ✅ Health Check

```bash
curl http://localhost:5000/health
```

Expected response: `200 OK`

---

## 🚀 Deploy Different Modes

### Live Stream Mode (Default)
```bash
docker-compose -f docker-compose.sophisticated.yml up -d
```

### Watch Time Booster Mode
```bash
docker-compose -f docker-compose.sophisticated.yml up -d \
  -e MODE=WATCHTIME \
  -e VIEWER_COUNT=50
```

### Multi-Platform Deployment
```bash
# Twitch
docker run -d -e PLATFORM=twitch botcore:sophisticated

# YouTube
docker run -d -e PLATFORM=youtube botcore:sophisticated

# Kick
docker run -d -e PLATFORM=kick botcore:sophisticated
```

---

## 🎯 That's It!

**You're now running an enterprise-grade browser automation system.**

For advanced features, see: `SOPHISTICATED_DEPLOYMENT_GUIDE.md`
# 🚀 SOPHISTICATED DOCKER DEPLOYMENT - COMPLETE GUIDE

## 🎯 Overview

This deployment system provides **enterprise-grade browser automation** with:

- ✅ .NET 8.0 with latest Playwright
- ✅ Health checks & self-healing
- ✅ Prometheus metrics & Grafana dashboards
- ✅ Distributed caching with Redis
- ✅ Message queue with RabbitMQ
- ✅ Structured logging with Seq
- ✅ Resilience patterns (retry, circuit breaker)
- ✅ Non-root security hardening
- ✅ Horizontal scaling ready

---

## 📦 What's Been Enhanced

### 1. **BotCore.csproj** → Upgraded to .NET 8.0
- Single-file deployment
- Latest Playwright 1.42.0
- Full dependency injection
- Health checks
- Polly resilience
- Prometheus metrics
- Redis caching
- RabbitMQ support

### 2. **DockerEntryPoint.Enhanced.cs** → Enterprise Features
- Structured logging with Serilog
- Browser pool management
- Health check implementations
- Metrics service
- Graceful shutdown
- Auto-retry with exponential backoff
- Circuit breaker patterns

### 3. **Dockerfile.enhanced** → Production-Ready
- Multi-stage build optimization
- Security hardening (non-root user)
- Playwright dependencies pre-installed
- Health checks built-in
- Resource limits
- Xvfb for non-headless mode

### 4. **docker-compose.sophisticated.yml** → Full Stack
- BotCore with health checks
- Redis for distributed caching
- Prometheus for metrics
- Grafana for visualization
- RabbitMQ for messaging
- Seq for structured logs
- Network isolation

---

## 🚀 Quick Start

### Option 1: One-Click Deployment (Windows)
```cmd
DEPLOY_SOPHISTICATED.bat
```

### Option 2: PowerShell
```powershell
.\DEPLOY_SOPHISTICATED.ps1
```

### Option 3: Manual Docker Compose
```bash
docker-compose -f docker-compose.sophisticated.yml up -d
```

---

## 📊 Access Points

Once deployed, access these services:

| Service | URL | Credentials |
|---------|-----|-------------|
| **BotCore Health** | http://localhost:5000/health | - |
| **Prometheus** | http://localhost:9090 | - |
| **Grafana** | http://localhost:3000 | admin/admin |
| **RabbitMQ** | http://localhost:15672 | admin/admin |
| **Seq Logs** | http://localhost:5341 | - |
| **Redis** | localhost:6379 | - |

---

## ⚙️ Configuration

### Environment Variables

```yaml
# Application Mode
MODE: LIVESTREAM              # or WATCHTIME
ASPNETCORE_ENVIRONMENT: Production

# Browser Settings
HEADLESS: true
MAX_VIEWERS: 50
ENABLE_CHAT: true

# Stream Settings
CHANNEL_USERNAME: timmaythetoolman
PLATFORM: twitch             # twitch, youtube, kick

# Resource Management
LOW_CPU_RAM: true
MAX_CONCURRENT_BROWSERS: 50

# Monitoring
ENABLE_METRICS: true
ENABLE_HEALTH_CHECKS: true

# Redis (optional)
ConnectionStrings__Redis: redis:6379

# RabbitMQ (optional)
RabbitMQ__Host: rabbitmq
```

### Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Production.json` - Production overrides
- `appsettings.Development.json` - Development settings
- `monitoring/prometheus.yml` - Metrics collection

---

## 🔧 Management Commands

### View Logs
```bash
docker-compose -f docker-compose.sophisticated.yml logs -f botcore
```

### Scale Horizontally
```bash
# Scale to 3 instances
docker-compose -f docker-compose.sophisticated.yml up -d --scale botcore=3

# Scale to 10 instances
docker-compose -f docker-compose.sophisticated.yml up -d --scale botcore=10
```

### Restart Service
```bash
docker-compose -f docker-compose.sophisticated.yml restart botcore
```

### Stop All Services
```bash
docker-compose -f docker-compose.sophisticated.yml down
```

### View Resource Usage
```bash
docker stats
```

### Enter Container
```bash
docker exec -it botcore-primary /bin/bash
```

---

## 🏥 Health Checks

The system includes three health checks:

1. **Browser Health** - Validates Playwright browser connectivity
2. **Memory Health** - Monitors memory usage (<1GB = healthy)
3. **System Resources** - Tracks threads, handles, CPU

Access health endpoint:
```bash
curl http://localhost:5000/health
```

---

## 📈 Monitoring & Metrics

### Prometheus Metrics
- Request rates
- Error rates
- Browser pool status
- Memory usage
- GC collections
- Thread counts

### Grafana Dashboards
1. Navigate to http://localhost:3000
2. Login: admin/admin
3. Import dashboard from Prometheus datasource

### Custom Metrics
Add custom metrics in code:
```csharp
private static readonly Counter BrowserLaunches = Metrics.CreateCounter(
    "botcore_browser_launches_total", 
    "Total browser launches");

BrowserLaunches.Inc();
```

---

## 🔐 Security Features

1. **Non-root user** - Runs as `botuser` (UID 1001)
2. **Network isolation** - Dedicated bridge network
3. **Resource limits** - CPU/memory constraints
4. **Health checks** - Auto-restart on failure
5. **Secrets management** - Environment variables

---

## 🚨 Troubleshooting

### Container Won't Start
```bash
# Check logs
docker-compose -f docker-compose.sophisticated.yml logs botcore

# Verify health
docker-compose -f docker-compose.sophisticated.yml ps
```

### Browser Initialization Fails
```bash
# Rebuild with no cache
docker-compose -f docker-compose.sophisticated.yml build --no-cache

# Check Playwright installation
docker exec -it botcore-primary playwright install chromium
```

### High Memory Usage
```bash
# Check memory
docker stats botcore-primary

# Reduce MAX_VIEWERS or scale horizontally
```

### Connection Issues
```bash
# Check network
docker network inspect stream-viewer-chat-bot_botcore-network

# Verify all services are up
docker-compose -f docker-compose.sophisticated.yml ps
```

---

## 🎯 Performance Tuning

### For High Volume (100+ viewers)
```yaml
deploy:
  resources:
    limits:
      cpus: '8.0'
      memory: 16G
```

### For Low Resource Mode
```yaml
environment:
  - MAX_VIEWERS=10
  - LOW_CPU_RAM=true
  - HEADLESS=true
```

### Horizontal Scaling
```bash
# Deploy 5 instances with load balancing
docker-compose -f docker-compose.sophisticated.yml up -d --scale botcore=5
```

---

## 📚 Architecture

```
┌─────────────────────────────────────────────┐
│           Load Balancer (Optional)          │
└─────────────────┬───────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
┌───────▼──────┐    ┌───────▼──────┐
│  BotCore #1  │    │  BotCore #2  │
│  (Port 5000) │    │  (Port 5001) │
└───────┬──────┘    └───────┬──────┘
        │                   │
        └─────────┬─────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
┌───▼───┐   ┌─────▼─────┐  ┌───▼──────┐
│ Redis │   │Prometheus │  │ RabbitMQ │
└───────┘   └─────┬─────┘  └──────────┘
                  │
            ┌─────▼─────┐
            │  Grafana  │
            └───────────┘
```

---

## 🎖️ Best Practices

1. **Always use health checks** - Enable monitoring
2. **Scale horizontally** - Don't max out single instance
3. **Monitor metrics** - Watch Prometheus dashboards
4. **Use Redis** - For session sharing across instances
5. **Log to Seq** - Structured logging for debugging
6. **Backup volumes** - Redis/RabbitMQ data persistence

---

## 🔄 Updates & Maintenance

### Update to Latest Version
```bash
docker-compose -f docker-compose.sophisticated.yml pull
docker-compose -f docker-compose.sophisticated.yml up -d
```

### Clean Up Old Images
```bash
docker system prune -a
```

### Backup Redis Data
```bash
docker exec botcore-redis redis-cli BGSAVE
```

---

## 📞 Support

For issues or enhancements, check:
- Container logs: `docker-compose logs -f`
- Health endpoint: `http://localhost:5000/health`
- Prometheus metrics: `http://localhost:9090`

---

## ✨ JARVIS 2.0 Features

This deployment includes:
- ♾️ Infinite retry on failure
- 🔄 Auto-self-heal
- 📊 Real-time monitoring
- 🚀 Zero-downtime deployment
- 🔐 Security hardening
- 📈 Auto-scaling ready
- 🎯 Multi-platform support

**Status**: ✅ WAR READY - OPERATIONAL

