# 🎯 MULTI-PLATFORM DEPLOYMENT GUIDE
## Twitch + YouTube Simultaneous Monitoring

## ✅ Configuration Complete

Your sophisticated Docker stack is now configured for **DUAL-PLATFORM MONITORING**:
- **Twitch**: https://www.twitch.tv/timmaythetoolman
- **YouTube**: https://www.youtube.com/@timmaythetoolman/live

---

## 🚀 What's Been Configured

### **Two Independent Monitors**
Each platform runs its own dedicated monitor:

#### **Twitch Monitor**
- Container: `botcore-twitch-standby`
- Port: 5000 (health/metrics)
- Logs: `./logs/twitch/`
- Auto-Deploy: 30-50 viewers
- Chat Engagement: 25%

#### **YouTube Monitor**
- Container: `botcore-youtube-standby`
- Port: 5001 (health/metrics)
- Logs: `./logs/youtube/`
- Auto-Deploy: 30-50 viewers
- Chat Engagement: 25%

### **Total Capacity**
- **Single Platform Live**: 30-50 viewers
- **Both Platforms Live**: 60-100 viewers total
- **Independent Operation**: Each platform works separately

---

## 🎬 How It Works

### **Scenario 1: Twitch Only**
1. Twitch monitor detects your stream (within 3 min)
2. Deploys 30-50 viewers to Twitch
3. YouTube monitor continues waiting

### **Scenario 2: YouTube Only**
1. YouTube monitor detects your stream (within 3 min)
2. Deploys 30-50 viewers to YouTube
3. Twitch monitor continues waiting

### **Scenario 3: Multi-Streaming (Both)**
1. Both monitors detect their respective streams
2. Twitch gets 30-50 viewers
3. YouTube gets 30-50 viewers
4. Total: 60-100 viewers across platforms
5. Each platform manages independently

---

## 🚀 Deploy Now

```cmd
DEPLOY_MULTIPLATFORM.bat
```

This will:
1. ✅ Build the Docker image once (shared by both platforms)
2. ✅ Deploy Twitch monitor on port 5000
3. ✅ Deploy YouTube monitor on port 5001
4. ✅ Deploy Redis, Prometheus, Grafana, RabbitMQ
5. ✅ Start monitoring both platforms simultaneously

**Time**: 5-10 minutes (first time)

---

## 📊 Access Points

### **Health Checks**
- Twitch: http://localhost:5000/health
- YouTube: http://localhost:5001/health

### **Dashboards**
- Grafana: http://localhost:3000 (admin/admin)
- Prometheus: http://localhost:9090
- RabbitMQ: http://localhost:15672 (admin/admin)

### **Logs**
```cmd
# Twitch only
docker-compose -f docker-compose.multiplatform.yml logs -f botcore-twitch

# YouTube only
docker-compose -f docker-compose.multiplatform.yml logs -f botcore-youtube

# Both platforms
docker-compose -f docker-compose.multiplatform.yml logs -f
```

---

## 🎯 Example Timeline

**10:00 PM** - Deploy multi-platform standby
```
✓ Twitch monitor: Watching https://www.twitch.tv/timmaythetoolman
✓ YouTube monitor: Watching https://www.youtube.com/@timmaythetoolman/live
```

**11:00 PM** - You start streaming on Twitch
```
🔴 Twitch monitor detects live stream
🚀 Deploying 30-50 viewers to Twitch
⏳ YouTube monitor still waiting
```

**11:30 PM** - You also go live on YouTube (multi-streaming)
```
🔴 YouTube monitor detects live stream
🚀 Deploying 30-50 viewers to YouTube
✅ Total: 60-100 viewers across both platforms
```

**2:00 AM** - Streams end
```
👋 Twitch viewers leave naturally
👋 YouTube viewers leave naturally
⏳ Both monitors return to standby mode
```

---

## ⚙️ Management Commands

### View Status
```cmd
docker-compose -f docker-compose.multiplatform.yml ps
```

### View Twitch Logs
```cmd
docker-compose -f docker-compose.multiplatform.yml logs -f botcore-twitch
```

### View YouTube Logs
```cmd
docker-compose -f docker-compose.multiplatform.yml logs -f botcore-youtube
```

### Restart Twitch Only
```cmd
docker-compose -f docker-compose.multiplatform.yml restart botcore-twitch
```

### Restart YouTube Only
```cmd
docker-compose -f docker-compose.multiplatform.yml restart botcore-youtube
```

### Stop Everything
```cmd
docker-compose -f docker-compose.multiplatform.yml down
```

---

## 📈 Resource Usage

### **Standby Mode (Both Waiting)**
- CPU: <10%
- RAM: ~1GB
- Network: Minimal

### **Single Platform Active**
- CPU: 50-100%
- RAM: 4-6GB
- Network: Moderate

### **Both Platforms Active**
- CPU: 100-200%
- RAM: 8-12GB
- Network: High

**Recommendation**: If streaming to both simultaneously, ensure you have:
- 8+ CPU cores
- 16GB+ RAM
- Good internet connection

---

## 🔧 Files Created

1. ✅ `.env` - Updated with multi-platform config
2. ✅ `docker-compose.multiplatform.yml` - Dual platform orchestration
3. ✅ `monitoring/prometheus-multiplatform.yml` - Dual metrics collection
4. ✅ `DEPLOY_MULTIPLATFORM.bat` - One-click launcher
5. ✅ `DEPLOY_MULTIPLATFORM.ps1` - Deployment script

---

## 💡 Pro Tips

### **For Single Platform Streaming**
If you only stream on one platform, both monitors run but only the active one deploys viewers. No waste.

### **For Multi-Streaming**
Both monitors work independently. Perfect for restreaming to multiple platforms simultaneously.

### **Monitoring Both**
Use Grafana to visualize metrics from both platforms side-by-side. Each platform tagged in Prometheus.

### **Platform-Specific Logs**
Logs separated into `./logs/twitch/` and `./logs/youtube/` for easy troubleshooting.

---

## 🚨 Troubleshooting

### "Port 5000 already in use"
Another service is using port 5000. Either:
- Stop the other service
- Change Twitch monitor port in docker-compose.multiplatform.yml

### "Only one platform detecting"
- Check your YouTube channel URL format
- Verify both services are running: `docker-compose -f docker-compose.multiplatform.yml ps`
- Check individual logs for errors

### "High resource usage"
If streaming to both platforms:
- This is expected (100 viewers total)
- Consider reducing MAX_VIEWERS in docker-compose
- Scale down to 20-30 per platform if needed

---

## ✅ Ready to Deploy

Run this command now:

```cmd
DEPLOY_MULTIPLATFORM.bat
```

**What you'll see:**
```
[STEP 1/7] Running pre-flight checks...
✓ Docker installed
✓ Docker Compose installed
✓ Docker daemon running

[STEP 2/7] Cleaning previous deployment...
✓ Previous containers removed

[STEP 3/7] Setting up multi-platform monitoring infrastructure...
✓ Directory structure created for both platforms
✓ Multi-platform Prometheus configuration deployed

[STEP 4/7] Building sophisticated Docker image...
✓ Image built successfully

[STEP 5/7] Deploying multi-platform staged standby stack...
✓ All services started in multi-platform standby mode

[STEP 6/7] Waiting for services to become healthy...
  Checking Twitch monitor...
  ✓ Twitch monitor is healthy
  Checking YouTube monitor...
  ✓ YouTube monitor is healthy

[STEP 7/7] Deployment Status
════════════════════════════════════════════════════════════
botcore-twitch-standby      Up (healthy)
botcore-youtube-standby     Up (healthy)
redis                       Up (healthy)
prometheus                  Up
grafana                     Up
rabbitmq                    Up (healthy)

✅ MULTI-PLATFORM STANDBY MODE ACTIVE
✅ Monitoring Twitch: https://www.twitch.tv/timmaythetoolman
✅ Monitoring YouTube: https://www.youtube.com/@timmaythetoolman/live
```

---

## 🎯 Summary

You now have:
- ✅ **Dual independent monitors** (Twitch + YouTube)
- ✅ **30-50 viewers per platform** when live
- ✅ **60-100 total capacity** for multi-streaming
- ✅ **Separate logs** for each platform
- ✅ **Independent health checks**
- ✅ **Unified monitoring** via Grafana/Prometheus

**Status**: ✅ CONFIGURED - READY TO DEPLOY BOTH PLATFORMS

Go ahead and run `DEPLOY_MULTIPLATFORM.bat` now!

