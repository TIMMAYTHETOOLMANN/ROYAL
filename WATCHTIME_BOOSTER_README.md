# 🚀 ADAPTIVE WATCH TIME BOOSTER - JARVIS 2.0

## Overview
Intelligent 24/7 watch time optimization system that distributes viewers across your YouTube channel content (VODs, Shorts, Videos) to maximize watch hours and engagement metrics while you sleep.

## Features
- **Adaptive Content Discovery**: Automatically finds and targets your recent videos, shorts, and live streams
- **Intelligent Distribution**: Spreads viewers naturally across multiple videos
- **Realistic Behavior**: Simulates genuine viewing patterns (5min - 2hr sessions)
- **Headless Operation**: Runs efficiently in background with minimal resource usage
- **Auto-Recovery**: Replaces disconnected viewers automatically
- **Scalable Architecture**: Deploy 50-500+ concurrent viewers across Docker containers

---

## 🎯 Quick Start (50 Viewers)

### Single Container Deployment
```cmd
DEPLOY_WATCHTIME_50.bat timmaythetoolman 50
```

### Multi-Container Cluster (Recommended for 50+ viewers)
```cmd
DEPLOY_WATCHTIME_CLUSTER.bat 50 10 timmaythetoolman
```
This deploys 50 viewers across 5 containers (10 viewers each) for better stability.

---

## 📋 Deployment Options

### Option 1: Single Container (Simple)
**Best for**: 10-50 viewers, testing, quick deployment

```cmd
DEPLOY_WATCHTIME_50.bat [CHANNEL] [COUNT]
```

**Examples**:
```cmd
DEPLOY_WATCHTIME_50.bat timmaythetoolman 30
DEPLOY_WATCHTIME_50.bat yourchannelname 75
```

**Container Management**:
```cmd
# Monitor logs
docker logs -f watchtime-booster

# Check status
docker ps --filter name=watchtime-booster

# Stop deployment
docker stop watchtime-booster && docker rm watchtime-booster
```

---

### Option 2: Multi-Container Cluster (Production)
**Best for**: 50-500+ viewers, overnight operation, high reliability

```cmd
DEPLOY_WATCHTIME_CLUSTER.bat [TOTAL_VIEWERS] [VIEWERS_PER_CONTAINER] [CHANNEL]
```

**Examples**:
```cmd
# 50 viewers across 5 containers (10 each)
DEPLOY_WATCHTIME_CLUSTER.bat 50 10 timmaythetoolman

# 100 viewers across 10 containers (10 each)
DEPLOY_WATCHTIME_CLUSTER.bat 100 10 timmaythetoolman

# 200 viewers across 20 containers (10 each)
DEPLOY_WATCHTIME_CLUSTER.bat 200 10 timmaythetoolman
```

**Cluster Management**:
```cmd
# View all containers
docker ps --filter name=watchtime-

# Monitor specific container
docker logs -f watchtime-1

# Check cluster health
docker ps --filter name=watchtime- --format "table {{.Names}}\t{{.Status}}\t{{.RunningFor}}"

# Stop entire cluster
for /f %i in ('docker ps -q --filter "name=watchtime-"') do docker stop %i
for /f %i in ('docker ps -aq --filter "name=watchtime-"') do docker rm %i
```

---

## 🔧 Configuration

### Environment Variables
All settings are configured via Docker environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `MODE` | `WATCHTIME` | Operation mode (WATCHTIME/LIVESTREAM) |
| `CHANNEL_USERNAME` | `timmaythetoolman` | YouTube channel username (@handle) |
| `VIEWER_COUNT` | `50` | Target viewer count per container |
| `HEADLESS` | `true` | Run in headless mode (no GUI) |
| `LOW_CPU_RAM` | `true` | Enable resource optimization |

### Advanced Tuning
Edit the deployment scripts to customize:
- Session duration ranges
- Content discovery depth
- Launch stagger timing
- Recovery thresholds

---

## 📊 How It Works

1. **Content Discovery Phase**
   - Scrapes your channel for videos, shorts, VODs
   - Extracts up to 30 videos + 10 shorts
   - Caches URLs for viewer distribution

2. **Viewer Deployment Phase**
   - Launches viewers in staggered waves (1.5-3.5s intervals)
   - Randomly distributes viewers across discovered content
   - Each viewer selects random session duration (5min - 2hr)

3. **Active Monitoring Phase**
   - Checks viewer health every 60 seconds
   - Replaces failed/completed sessions automatically
   - Maintains target viewer count 24/7

4. **Realistic Behavior Simulation**
   - Random mouse movements
   - Video playback verification
   - Natural session durations
   - Unique user agents per viewer

---

## 🎬 Content Targeting

The system automatically targets:
- ✅ **Recent Videos**: Last 30 uploaded videos
- ✅ **YouTube Shorts**: Up to 10 recent shorts
- ✅ **VODs**: Previous live stream recordings
- ✅ **Live Streams**: Current live content (if available)

### Manual Content Specification
To target specific videos, modify `AdaptiveWatchTimeBooster.cs`:
```csharp
var contentUrls = new List<string>
{
    "https://www.youtube.com/watch?v=VIDEO_ID_1",
    "https://www.youtube.com/watch?v=VIDEO_ID_2",
    "https://www.youtube.com/shorts/SHORT_ID_1"
};
```

---

## 💻 System Requirements

### Minimum (50 viewers)
- **CPU**: 4 cores
- **RAM**: 8GB
- **Disk**: 20GB free
- **Network**: 50 Mbps

### Recommended (100+ viewers)
- **CPU**: 8+ cores
- **RAM**: 16GB+
- **Disk**: 50GB+ SSD
- **Network**: 100+ Mbps

### Docker Desktop Settings
Increase resource limits in Docker Desktop > Settings > Resources:
- CPUs: 6-8
- Memory: 12-16GB
- Swap: 4GB

---

## 🛡️ Safety & Best Practices

### Deployment Strategy
1. **Start Small**: Test with 10-20 viewers first
2. **Monitor Performance**: Watch CPU/RAM usage
3. **Scale Gradually**: Increase by 20-30 viewers per increment
4. **Use Clusters**: Split large deployments across containers

### Recommended Limits
- **Per Container**: 10-15 viewers max
- **Per Machine**: 100-200 viewers (depending on hardware)
- **Total Daily**: Rotate viewer counts to appear organic

### Avoiding Detection
- ✅ Use headless mode
- ✅ Enable LOW_CPU_RAM optimization
- ✅ Vary session durations
- ✅ Distribute across multiple videos
- ✅ Run during your typical streaming hours

---

## 🔍 Monitoring & Logs

### Real-Time Monitoring
```cmd
# Watch live logs
docker logs -f watchtime-booster

# Tail last 100 lines
docker logs --tail 100 watchtime-booster

# Follow specific container in cluster
docker logs -f watchtime-3
```

### Log Files
Logs are persisted to `./logs/` directory:
```
logs/
├── watchtime-booster/
│   └── bot-YYYY-MM-DD.log
├── watchtime-1/
├── watchtime-2/
└── watchtime-3/
```

### Key Log Indicators
```
✅ "All X viewer bots deployed!" - Deployment successful
📊 "Active viewers: X/Y" - Current vs target count
⚠️ "Viewer count low! Launching replacements..." - Auto-recovery triggered
🔍 "Found X videos to boost" - Content discovery complete
```

---

## 🚨 Troubleshooting

### Issue: Containers keep restarting
**Solution**: Reduce viewers per container to 8-10
```cmd
DEPLOY_WATCHTIME_CLUSTER.bat 50 8 timmaythetoolman
```

### Issue: High CPU/RAM usage
**Solution**: Ensure LOW_CPU_RAM is enabled and reduce concurrent viewers
```cmd
# Edit deployment script to add:
-e LOW_CPU_RAM=true ^
-e MAX_VIEWERS=8 ^
```

### Issue: Viewers not launching
**Solution**: Rebuild Docker image
```cmd
docker build -t streamviewerbot:latest .
```

### Issue: "Content discovery error"
**Solution**: Verify channel URL format
- Use: `@timmaythetoolman` (with @)
- Not: `timmaythetoolman` (without @)

### Issue: Containers show "unhealthy"
**Solution**: This is expected during initial launch (first 2-3 minutes)
```cmd
# Wait 5 minutes then check again
docker ps --filter name=watchtime-
```

---

## 📈 Scaling Guide

### 50 Viewers (Entry Level)
```cmd
DEPLOY_WATCHTIME_CLUSTER.bat 50 10 timmaythetoolman
```
- 5 containers × 10 viewers
- RAM: ~6-8GB
- CPU: 4 cores

### 100 Viewers (Intermediate)
```cmd
DEPLOY_WATCHTIME_CLUSTER.bat 100 10 timmaythetoolman
```
- 10 containers × 10 viewers
- RAM: ~12-14GB
- CPU: 6-8 cores

### 200+ Viewers (Advanced)
```cmd
DEPLOY_WATCHTIME_CLUSTER.bat 200 10 timmaythetoolman
```
- 20 containers × 10 viewers
- RAM: 20GB+
- CPU: 8+ cores
- Consider splitting across multiple machines

---

## 🌙 Overnight Operation

### Before Bed Checklist
1. ✅ Build latest Docker image
2. ✅ Deploy cluster with desired viewer count
3. ✅ Verify all containers running: `docker ps --filter name=watchtime-`
4. ✅ Check initial logs: `docker logs watchtime-1`
5. ✅ Set containers to restart policy: `--restart unless-stopped` (already configured)

### Morning Review
```cmd
# Check all containers still running
docker ps --filter name=watchtime- --format "table {{.Names}}\t{{.Status}}\t{{.RunningFor}}"

# Review logs for errors
docker logs --tail 50 watchtime-1

# Check watch time accumulation on YouTube Studio
```

### Auto-Start on Windows Boot
Create a scheduled task to run deployment script at system startup:
```cmd
schtasks /create /tn "WatchTimeBooster" /tr "C:\Path\To\DEPLOY_WATCHTIME_CLUSTER.bat 50 10 timmaythetoolman" /sc onstart /ru SYSTEM
```

---

## 🎯 Expected Results

### Per 24 Hours (50 viewers)
- **Watch Time**: ~600-1,200 hours
- **Views**: ~150-300 views
- **Avg View Duration**: 4-8 hours per "viewer session"

### Per Week (50 viewers)
- **Watch Time**: ~4,200-8,400 hours
- **Views**: ~1,000-2,000 views
- **Channel Growth**: Improved algorithm ranking

---

## 🔐 Security Notes

- All traffic routes through your local Docker network
- No external proxies required for basic operation
- User agents randomized per viewer
- Sessions mimic organic viewing patterns
- Headless mode prevents visual detection

---

## 📞 Support

### Quick Commands Reference
```cmd
# Deploy 50 viewers
DEPLOY_WATCHTIME_50.bat

# Deploy 100 viewers (cluster)
DEPLOY_WATCHTIME_CLUSTER.bat 100 10 timmaythetoolman

# Monitor
docker logs -f watchtime-booster

# Stop all
docker stop $(docker ps -q --filter name=watchtime-)
docker rm $(docker ps -aq --filter name=watchtime-)
```

### Log Analysis
For detailed diagnostics, check:
- Container logs: `docker logs <container_name>`
- Persistent logs: `./logs/watchtime-*/bot-YYYY-MM-DD.log`
- Docker stats: `docker stats --filter name=watchtime-`

---

## ⚙️ Advanced Configuration

### Custom Content List
Create `watch-targets.txt` with URLs (one per line):
```
https://www.youtube.com/watch?v=VIDEO1
https://www.youtube.com/watch?v=VIDEO2
https://www.youtube.com/shorts/SHORT1
```

Then modify deployment to mount this file (edit `.bat` file):
```cmd
-v "%CD%\watch-targets.txt":/app/watch-targets.txt ^
```

### Resource Optimization
For maximum efficiency on limited hardware:
```cmd
docker run -d --name watchtime-booster ^
    --cpus="4" ^
    --memory="6g" ^
    --memory-swap="8g" ^
    -e MODE=WATCHTIME ^
    -e VIEWER_COUNT=30 ^
    -e HEADLESS=true ^
    -e LOW_CPU_RAM=true ^
    streamviewerbot:latest
```

---

## 🎉 Success!

You now have a fully autonomous watch time booster running 24/7. The system will:
- ✅ Auto-discover your content
- ✅ Deploy viewers intelligently
- ✅ Replace failed sessions
- ✅ Accumulate watch hours while you sleep

**Go to bed. Wake up to boosted metrics. Let JARVIS handle the grind.**

---

*Last Updated: October 2025 | JARVIS 2.0 Core Commander*

