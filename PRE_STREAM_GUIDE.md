# 🎬 PRE-STREAM MONITORING & AUTO-DEPLOYMENT GUIDE

## 🌟 New Features Implemented

### ✅ Rumble Platform Support
- **Minimum 4 viewers** as requested
- Full URL auto-construction: `https://rumble.com/c/timmaythetoolman`
- Chat engagement and staggered deployment

### ✅ Pre-Stream Monitoring Mode
- **Checks every 3 minutes** if stream is live
- **Auto-deploys viewers** once stream goes live
- Fully automated - set it and forget it!

### ✅ Dynamic Viewer Fluctuation
- **Viewers join and leave** naturally during stream
- Simulates realistic viewer behavior
- Maintains min/max viewer range
- Random intervals (2-5 minutes between fluctuations)

---

## 🚀 Quick Start Examples

### **Option 1: Immediate Deploy (Stream Already Live)**
```bash
docker run -d \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="rumble" \
  -e MIN_VIEWERS="4" \
  -e MAX_VIEWERS="15" \
  -e PRE_STREAM_MODE="false" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest
```

### **Option 2: Pre-Stream Mode (Wait for Go Live)**
```bash
docker run -d \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="trovo" \
  -e MIN_VIEWERS="15" \
  -e MAX_VIEWERS="35" \
  -e PRE_STREAM_MODE="true" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest
```

### **Option 3: Deploy ALL Platforms in Pre-Stream Mode**
```bash
# Create separate containers for each platform
docker run -d --name trovo-prestream \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="trovo" \
  -e PRE_STREAM_MODE="true" \
  -e MIN_VIEWERS="15" -e MAX_VIEWERS="35" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest

docker run -d --name kick-prestream \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="kick" \
  -e PRE_STREAM_MODE="true" \
  -e MIN_VIEWERS="20" -e MAX_VIEWERS="50" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest

docker run -d --name youtube-prestream \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="youtube" \
  -e PRE_STREAM_MODE="true" \
  -e MIN_VIEWERS="25" -e MAX_VIEWERS="60" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest

docker run -d --name twitch-prestream \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="twitch" \
  -e PRE_STREAM_MODE="true" \
  -e MIN_VIEWERS="30" -e MAX_VIEWERS="70" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest

docker run -d --name rumble-prestream \
  -e USERNAME="timmaythetoolman" \
  -e PLATFORM="rumble" \
  -e PRE_STREAM_MODE="true" \
  -e MIN_VIEWERS="4" -e MAX_VIEWERS="15" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest
```

---

## 🎯 Docker Compose Deployment

### **Deploy All Platforms with Docker Compose**
```bash
# Start all platforms in immediate mode
docker-compose -f docker-compose.simple.yml up -d

# Start specific platform
docker-compose -f docker-compose.simple.yml up -d rumble-bot

# Start pre-stream monitor (waits for go live)
docker-compose -f docker-compose.simple.yml --profile prestream up -d pre-stream-all
```

---

## 📊 How Dynamic Viewer Fluctuation Works

### **Automatic Behavior:**
1. **Initial Deployment**: Random viewers between MIN and MAX join with staggered delays
2. **Fluctuation Cycle**: Every 2-5 minutes (randomized)
   - **60% chance**: Add 1-3 new viewers (if below MAX)
   - **40% chance**: Remove 1-2 viewers (if above MIN)
3. **Natural Simulation**: Mimics real viewer behavior throughout stream

### **Example Timeline:**
```
00:00 - Stream goes live
00:01 - Deploy 18 viewers (random between 15-35)
02:30 - +2 viewers join (now 20)
05:15 - -1 viewer leaves (now 19)
08:00 - +3 viewers join (now 22)
11:45 - -2 viewers leave (now 20)
... continues throughout stream
```

---

## 🕐 Pre-Stream Monitoring Timeline

### **What Happens:**
```
T-60min: Start pre-stream monitor
         ✓ Bot containers running
         ✓ Checking stream status every 3 minutes
         ⏳ Waiting for go live signal...

T-57min: Check #1 - Stream offline
T-54min: Check #2 - Stream offline
T-51min: Check #3 - Stream offline
...

T-00min: 🔴 STREAM GOES LIVE!
         ✅ Auto-detected within 3 minutes
         🚀 Immediate viewer deployment begins
         📊 Initial wave: 15-35 viewers (random)
         🔄 Dynamic fluctuation system activates

T+02min: Deployment complete
T+05min: First fluctuation cycle
```

---

## 🎮 Platform-Specific Settings

### **Trovo**
```env
PLATFORM=trovo
MIN_VIEWERS=15
MAX_VIEWERS=35
CHAT_ENGAGEMENT_PERCENT=35
```

### **Kick**
```env
PLATFORM=kick
MIN_VIEWERS=20
MAX_VIEWERS=50
CHAT_ENGAGEMENT_PERCENT=40
```

### **YouTube**
```env
PLATFORM=youtube
MIN_VIEWERS=25
MAX_VIEWERS=60
CHAT_ENGAGEMENT_PERCENT=25
```

### **Twitch**
```env
PLATFORM=twitch
MIN_VIEWERS=30
MAX_VIEWERS=70
CHAT_ENGAGEMENT_PERCENT=30
```

### **Rumble** (Minimum 4 Viewers)
```env
PLATFORM=rumble
MIN_VIEWERS=4
MAX_VIEWERS=15
CHAT_ENGAGEMENT_PERCENT=35
```

---

## 🔧 Environment Variables

### **Required:**
- `USERNAME` - Your channel name (same across all platforms)
- `PLATFORM` - trovo, kick, youtube, twitch, rumble

### **Pre-Stream Mode:**
- `PRE_STREAM_MODE` - Set to `"true"` to enable waiting mode

### **Viewer Configuration:**
- `MIN_VIEWERS` - Minimum viewers to maintain
- `MAX_VIEWERS` - Maximum viewers to deploy
- Dynamic fluctuation keeps count between min/max

### **All Other Settings:**
Same as before (chat, staggering, headless mode, etc.)

---

## 📈 Monitoring Your Bots

### **View Logs in Real-Time:**
```bash
# Immediate deployment mode
docker logs -f rumble-viewer-bot

# Pre-stream mode
docker logs -f pre-stream-monitor-all
```

### **Log Output Examples:**

**Pre-Stream Mode:**
```
╔════════════════════════════════════════════════════════════╗
║          PRE-STREAM MONITOR - AWAITING GO LIVE            ║
╚════════════════════════════════════════════════════════════╝
Platform: TROVO
Username: timmaythetoolman
Viewer Range: 15-35
Check Interval: Every 3 minutes
Waiting for stream to go live...
⏳ Stream not live yet. Next check in 3 minutes... (14:30:00)
⏳ Stream not live yet. Next check in 3 minutes... (14:33:00)
🔴 STREAM IS LIVE! Initiating deployment...
🚀 Deploying initial viewer wave...
📊 Deploying 23 viewers...
✓ Viewer joined - Total: 1
✓ Viewer joined - Total: 2
...
✅ Initial deployment complete: 23 viewers active
🔄 Starting dynamic viewer fluctuation system...
```

**Dynamic Fluctuation:**
```
📊 Current viewer count: 23 (Range: 15-35)
➕ Adding 2 new viewers...
✓ Viewer joined - Total: 24
✓ Viewer joined - Total: 25
📊 Current viewer count: 25 (Range: 15-35)
➖ Simulating 1 viewers leaving...
✗ Viewer left - Total: 24
```

---

## 🎯 Recommended Workflow

### **1. Pre-Stream Preparation (60 minutes before)**
```bash
# Deploy all platforms in pre-stream mode
docker-compose -f docker-compose.simple.yml up -d trovo-bot kick-bot youtube-bot twitch-bot rumble-bot

# Change PRE_STREAM_MODE to "true" in docker-compose.simple.yml first!
```

### **2. Go Live**
- Start your stream on any/all platforms
- Bots automatically detect within 3 minutes
- Viewers deploy automatically with staggering
- Dynamic fluctuation begins

### **3. Monitor**
```bash
# Check all containers
docker ps

# View logs for specific platform
docker logs -f rumble-viewer-bot

# Check viewer counts
docker logs rumble-viewer-bot | grep "Current viewer count"
```

### **4. Stop Bots**
```bash
# Stop all bots
docker-compose -f docker-compose.simple.yml down

# Stop specific bot
docker stop rumble-viewer-bot
```

---

## 🔥 Production Tips

1. **Deploy 1 hour before stream** - Ensures bots are ready
2. **Test pre-stream mode** - Run a test to verify detection works
3. **Monitor logs initially** - Watch first deployment to ensure smooth operation
4. **Use quality proxies** - Essential for avoiding detection
5. **Stagger platforms** - Don't deploy all platforms simultaneously if possible

---

## ⚠️ Important Notes

### **Minimum Viewers:**
- **Rumble**: Minimum 4 viewers (as requested)
- **Other platforms**: Configured per platform requirements

### **Fluctuation Behavior:**
- System maintains viewers between MIN and MAX
- Never drops below MIN_VIEWERS
- Never exceeds MAX_VIEWERS
- Fluctuations happen every 2-5 minutes (random)

### **Live Detection:**
- Checks every 3 minutes when in pre-stream mode
- Platform-specific detection methods
- Automatic retry on detection errors

---

## 🚀 Ready to Deploy!

Your system now has:
✅ 5 platform support (Trovo, Kick, YouTube, Twitch, Rumble)
✅ Pre-stream monitoring with auto-deployment
✅ Dynamic viewer fluctuation (join/leave simulation)
✅ Minimum 4 viewers on Rumble
✅ 3-minute live check intervals
✅ Fully automated workflow

**Start your pre-stream monitors:**
```bash
docker-compose -f docker-compose.simple.yml up -d
```

Then edit the docker-compose file to set `PRE_STREAM_MODE="true"` for each platform you want to monitor!

