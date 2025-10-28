# STREAM VIEWER BOT - COMPLETE RESTRUCTURE
**Date**: October 27, 2025  
**Status**: ✅ COMPLETE SYSTEM SEPARATION IMPLEMENTED

---

## 🎯 SYSTEM OVERVIEW

This project contains **TWO COMPLETELY SEPARATE SYSTEMS**:

### 1. TWITCH LIVE STREAM VIEWER + CHAT BOT
- **Purpose**: Connect to LIVE Twitch streams with real-time chat
- **Entry Point**: `DockerEntryPoint.cs`
- **Dockerfile**: `Dockerfile.twitch`
- **Compose**: `docker-compose.twitch.yml`
- **Deploy**: `./deploy-twitch.sh`
- **README**: `README-TWITCH.md`
- **Requires**: Active live stream + OAuth authentication

### 2. YOUTUBE WATCH TIME BOOSTER
- **Purpose**: Accumulate watch time on static YouTube videos/shorts
- **Entry Point**: `WatchTimeBoosterEntryPoint.cs`
- **Dockerfile**: `Dockerfile.youtube`
- **Compose**: `docker-compose.youtube.yml`
- **Deploy**: `./deploy-youtube.sh`
- **README**: `README-YOUTUBE.md`
- **Requires**: YouTube channel with content (no live stream needed)

**⚠️ HEADLESS MODE COMPLETELY PURGED** - HeadlessEntryPoint.cs is no longer used. Both systems use visual rendering via Xvfb (HEADLESS=false) to defeat anti-bot detection.

---

## 📁 NEW FILE STRUCTURE

```
Stream-Viewer-Chat-Bot/
├── TWITCH SYSTEM
│   ├── Dockerfile.twitch              # Builds with DockerEntryPoint
│   ├── docker-compose.twitch.yml      # Twitch-specific deployment
│   ├── deploy-twitch.sh               # Twitch deployment script
│   ├── README-TWITCH.md               # Twitch documentation
│   └── BotCore/appsettings.twitch.json  # OAuth configuration
│
├── YOUTUBE SYSTEM
│   ├── Dockerfile.youtube             # Builds with WatchTimeBoosterEntryPoint
│   ├── docker-compose.youtube.yml     # YouTube-specific deployment
│   ├── deploy-youtube.sh              # YouTube deployment script
│   ├── README-YOUTUBE.md              # YouTube documentation
│   └── BotCore/appsettings.youtube.json  # YouTube configuration
│
├── SHARED RESOURCES
│   ├── BotCore/                       # .NET application source
│   │   ├── DockerEntryPoint.cs        # Twitch live stream logic
│   │   ├── WatchTimeBoosterEntryPoint.cs  # YouTube watch time logic
│   │   ├── AdaptiveWatchTimeBooster.cs
│   │   └── ChatEngagementEngine.cs
│   ├── proxies.txt                    # Shared proxy list
│   └── logs/
│       ├── twitch/                    # Twitch logs
│       └── youtube/                   # YouTube logs
│
└── LEGACY FILES (DEPRECATED)
    ├── Dockerfile.fortified           # OLD - monolithic design
    ├── docker-compose.gitbash.yml     # OLD - mixed platforms
    └── HeadlessEntryPoint.cs          # PURGED - headless mode removed
```

---

## 🚀 QUICK START

### Deploy Twitch Live Stream Bot
```bash
cd ~/RiderProjects/Stream-Viewer-Chat-Bot
chmod +x deploy-twitch.sh
./deploy-twitch.sh
```

**Requirements**:
- ✅ You must be LIVE STREAMING on Twitch
- ✅ OAuth token configured in `BotCore/appsettings.twitch.json`
- ✅ 8GB+ RAM, 4+ CPU cores

### Deploy YouTube Watch Time Booster
```bash
cd ~/RiderProjects/Stream-Viewer-Chat-Bot
chmod +x deploy-youtube.sh
./deploy-youtube.sh
```

**Requirements**:
- ✅ YouTube channel with videos/shorts
- ✅ 8GB+ RAM, 4+ CPU cores
- ✅ NO live stream required

---

## 🔧 KEY DIFFERENCES

| Feature | Twitch Bot | YouTube Bot |
|---------|-----------|-------------|
| **Purpose** | Live stream viewers + chat | Static video watch time |
| **Entry Point** | DockerEntryPoint.cs | WatchTimeBoosterEntryPoint.cs |
| **Dockerfile** | Dockerfile.twitch | Dockerfile.youtube |
| **Docker Image** | botcore-twitch:latest | botcore-youtube:latest |
| **Container** | botcore-twitch | botcore-youtube |
| **Target URL** | twitch.tv/timmaythetoolman | youtube.com/@timmaythetoolman/videos |
| **Authentication** | OAuth required | Not required |
| **Chat** | Yes (25% engagement) | No |
| **Live Dependency** | Must be streaming | Works 24/7 |
| **Viewer Count** | 50 | 30 |
| **Ports** | 5000, 8080 | 5001, 8081 |
| **Monitoring** | 9090 (Prometheus), 3000 (Grafana) | 9091 (Prometheus), 3001 (Grafana) |

---

## 📊 DEPLOYMENT WORKFLOW

### Twitch Workflow
```
1. Create BotCore/appsettings.twitch.json with OAuth token
2. Run: ./deploy-twitch.sh
3. Start Twitch live stream
4. Watch viewer count increase by ~50
5. Chat messages appear every 60-240 seconds
6. Monitor: docker logs -f botcore-twitch
```

### YouTube Workflow
```
1. Ensure YouTube channel has videos/shorts
2. Run: ./deploy-youtube.sh
3. Bots navigate to /videos and /shorts
4. Watch time accumulates on random videos
5. No live stream required - works 24/7
6. Monitor: docker logs -f botcore-youtube
```

---

## ⚙️ CONFIGURATION

### Twitch: docker-compose.twitch.yml
```yaml
environment:
  - PLATFORM=twitch
  - CHANNEL_USERNAME=timmaythetoolman
  - VIEWER_COUNT=50
  - HEADLESS=false              # CRITICAL - must be false
  - LOW_CPU_RAM=false           # CRITICAL - must be false
  - ENABLE_CHAT=true
  - CHAT_ENGAGEMENT_PERCENT=25
  - MIN_CHAT_DELAY=60
  - MAX_CHAT_DELAY=240
  - DISPLAY=:99                 # Xvfb virtual display
```

### YouTube: docker-compose.youtube.yml
```yaml
environment:
  - CHANNEL_USERNAME=timmaythetoolman
  - VIEWER_COUNT=30
  - HEADLESS=false              # CRITICAL - must be false
  - LOW_CPU_RAM=false           # CRITICAL - must be false
  - DISPLAY=:99                 # Xvfb virtual display
```

**Note**: No PLATFORM or chat settings for YouTube - it's YouTube-specific with no chat capability.

---

## 🔍 MONITORING

### Twitch Monitoring
```bash
# Container status
docker ps --filter "name=twitch"

# Logs
docker logs -f botcore-twitch

# Health
curl http://localhost:8080/health

# Resource usage
docker stats --no-stream botcore-twitch

# Process count (should be 50+)
docker exec botcore-twitch sh -c "ps aux | grep chromium | wc -l"
```

### YouTube Monitoring
```bash
# Container status
docker ps --filter "name=youtube"

# Logs
docker logs -f botcore-youtube

# Health
curl http://localhost:8081/health

# Resource usage
docker stats --no-stream botcore-youtube

# Process count (should be 30+)
docker exec botcore-youtube sh -c "ps aux | grep chromium | wc -l"
```

---

## 🛑 STOPPING SYSTEMS

### Stop Twitch Bot
```bash
docker-compose -f docker-compose.twitch.yml down
```

### Stop YouTube Bot
```bash
docker-compose -f docker-compose.youtube.yml down
```

### Stop Both
```bash
docker-compose -f docker-compose.twitch.yml down
docker-compose -f docker-compose.youtube.yml down
```

---

## 🐛 TROUBLESHOOTING

### Twitch: No Viewers Appearing
1. **Verify live stream is active** - Bot fails if you're not streaming
2. **Check OAuth token** - Invalid token = no chat
3. **Review logs**: `docker logs botcore-twitch 2>&1 | grep -i error`

### YouTube: No Watch Time Increase
1. **Verify channel has content** - /videos page must have videos
2. **Check browser launch**: `docker exec botcore-youtube sh -c "ps aux | grep chromium"`
3. **Review logs**: `docker logs botcore-youtube 2>&1 | grep -i error`

### General: Container Restarting
1. **Check BotCore.dll exists**: `docker exec <container> ls -la /app/BotCore.dll`
2. **Verify Xvfb running**: `docker exec <container> sh -c "ps aux | grep Xvfb"`
3. **Check resource usage**: `docker stats --no-stream` - should show high CPU/RAM

---

## ⚠️ CRITICAL NOTES

### Why HEADLESS=false is Mandatory
Both Twitch and YouTube have sophisticated anti-bot detection that immediately flags pure headless browsers. This system uses **Xvfb (X Virtual Framebuffer)** to:
- Render full browser UI with GPU acceleration
- Execute JavaScript exactly like real browsers
- Display video frames to virtual display :99
- Defeat anti-bot fingerprinting

**Setting HEADLESS=true will make the system completely ineffective.**

### Why HeadlessEntryPoint Was Removed
The HeadlessEntryPoint.cs file attempted to run browsers in pure headless mode, which:
- Gets detected and blocked immediately by platforms
- Defeats the entire purpose of visual rendering
- Provides zero practical value

**It has been completely purged from the deployment system.**

---

## 📖 DETAILED DOCUMENTATION

For complete details on each system, see:
- **Twitch**: `README-TWITCH.md` - OAuth setup, live stream requirements, chat configuration
- **YouTube**: `README-YOUTUBE.md` - Watch time strategy, video rotation, no-auth operation

---

## 🔄 MIGRATION FROM OLD SYSTEM

If you have the old monolithic deployment running:

```bash
# Stop old system
docker-compose -f docker-compose.gitbash.yml down

# Remove old image
docker rmi botcore-base:latest

# Deploy new separated systems
./deploy-twitch.sh
./deploy-youtube.sh
```

**Key Changes**:
- ✅ Separate Docker images for each platform
- ✅ Separate entry points (no more confusion)
- ✅ Separate deployment scripts
- ✅ Separate log directories
- ✅ Separate monitoring ports
- ✅ HeadlessEntryPoint.cs completely removed

---

## 📊 EXPECTED RESULTS

### Twitch Bot (When Working)
- **Viewer count increases by ~50** during live stream
- **Chat messages appear every 60-240 seconds** (12-13 total)
- **CPU usage: 200-400%** (2-4 cores)
- **RAM usage: 4-8 GB**
- **50+ chromium processes running**

### YouTube Bot (When Working)
- **Watch time accumulates** on videos and shorts
- **Views increase** by ~30
- **CPU usage: 150-300%** (1.5-3 cores)
- **RAM usage: 3-6 GB**
- **30+ chromium processes running**

---

## 🎯 SYSTEM STATUS

**RESTRUCTURE COMPLETE**: ✅  
**Twitch System**: Ready for deployment  
**YouTube System**: Ready for deployment  
**HeadlessEntryPoint**: Purged  
**Platform Separation**: Implemented  

**Next Steps**:
1. Create `BotCore/appsettings.twitch.json` with OAuth token
2. Deploy Twitch bot: `./deploy-twitch.sh`
3. Deploy YouTube bot: `./deploy-youtube.sh`
4. Monitor logs to verify operation
5. Confirm viewers/watch time increase

---

**Restructure Date**: October 27, 2025  
**System Version**: 2.0 - Complete Platform Separation

