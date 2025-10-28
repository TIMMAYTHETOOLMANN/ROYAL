@echo off
REM Stop and remove all viewer bots (both pre-stream and immediate modes)

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║            STOP ALL VIEWER BOTS                            ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

echo Stopping all bots...

REM Pre-stream mode bots
docker stop trovo-prestream 2>nul
docker stop kick-prestream 2>nul
docker stop youtube-prestream 2>nul
docker stop twitch-prestream 2>nul
docker stop rumble-prestream 2>nul

REM Immediate mode bots
docker stop trovo-bot 2>nul
docker stop kick-bot 2>nul
docker stop youtube-bot 2>nul
docker stop twitch-bot 2>nul
docker stop rumble-bot 2>nul

echo.
echo Removing containers...

REM Pre-stream mode bots
docker rm trovo-prestream 2>nul
docker rm kick-prestream 2>nul
docker rm youtube-prestream 2>nul
docker rm twitch-prestream 2>nul
docker rm rumble-prestream 2>nul

REM Immediate mode bots
docker rm trovo-bot 2>nul
docker rm kick-bot 2>nul
docker rm youtube-bot 2>nul
docker rm twitch-bot 2>nul
docker rm rumble-bot 2>nul

echo.
echo ✅ All viewer bots stopped and removed.
echo.
pause
# 🚀 QUICK START DEPLOYMENT GUIDE

## ✅ Docker Image Built Successfully!
**Image:** `stream-viewer-bot:latest`
**Size:** ~1.74GB
**Status:** Ready to deploy

---

## 🎯 DEPLOYMENT OPTIONS

### **Option 1: Pre-Stream Mode (Recommended - Deploy Before Going Live)**

Run this command to deploy **ALL 5 PLATFORMS** in monitoring mode:

```bash
.\DEPLOY_PRESTREAM.bat
```

**What this does:**
- Deploys bots for Trovo, Kick, YouTube, Twitch, and Rumble
- Each bot checks every 3 minutes if your stream is live
- When stream goes live, viewers auto-deploy automatically
- Viewers dynamically fluctuate (join/leave) throughout stream

**Use Case:** Deploy 30-60 minutes before your stream starts

---

### **Option 2: Immediate Deploy (Stream Already Live)**

#### **Single Platform:**
```bash
# Trovo
docker run -d --name trovo-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="trovo" ^
  -e MIN_VIEWERS="15" ^
  -e MAX_VIEWERS="35" ^
  -e PRE_STREAM_MODE="false" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\trovo:/app/logs" ^
  stream-viewer-bot:latest

# Rumble (Minimum 4 viewers)
docker run -d --name rumble-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="rumble" ^
  -e MIN_VIEWERS="4" ^
  -e MAX_VIEWERS="15" ^
  -e PRE_STREAM_MODE="false" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\rumble:/app/logs" ^
  stream-viewer-bot:latest

# Kick
docker run -d --name kick-bot ^
  -e USERNAME="timmaythetoolman" ^
  -e PLATFORM="kick" ^
  -e MIN_VIEWERS="20" ^
  -e MAX_VIEWERS="50" ^
  -e PRE_STREAM_MODE="false" ^
  -v "%cd%\proxies.txt:/app/proxies.txt" ^
  -v "%cd%\logs\kick:/app/logs" ^
  stream-viewer-bot:latest
```

---

### **Option 3: Docker Compose (Multiple Platforms)**

```bash
# Deploy all platforms immediately
docker-compose -f docker-compose.simple.yml up -d

# Deploy specific platforms
docker-compose -f docker-compose.simple.yml up -d trovo-bot rumble-bot

# Deploy in pre-stream mode (edit docker-compose.simple.yml first, set PRE_STREAM_MODE="true")
docker-compose -f docker-compose.simple.yml up -d
```

---

## 📊 MONITORING YOUR BOTS

### **View Running Containers:**
```bash
docker ps
```

### **View Logs (Real-Time):**
```bash
# Pre-stream mode
docker logs -f trovo-prestream

# Immediate mode
docker logs -f trovo-bot
```

### **Check Viewer Count:**
```bash
docker logs trovo-bot | findstr "viewers"
```

---

## 🛑 STOPPING BOTS

### **Stop All Pre-Stream Bots:**
```bash
.\STOP_PRESTREAM.bat
```

### **Stop Individual Bot:**
```bash
docker stop trovo-prestream
docker rm trovo-prestream
```

### **Stop All Running Bots:**
```bash
docker stop $(docker ps -q)
```

---

## 📝 EXAMPLE WORKFLOW

### **Pre-Stream Deployment (Best Practice):**

**1. Deploy 60 minutes before stream:**
```bash
.\DEPLOY_PRESTREAM.bat
```

**2. Monitor the bots:**
```bash
docker logs -f trovo-prestream
```

**You'll see:**
```
╔════════════════════════════════════════════════════════════╗
║          PRE-STREAM MONITOR - AWAITING GO LIVE            ║
╚════════════════════════════════════════════════════════════╝
Platform: TROVO
Username: timmaythetoolman
Viewer Range: 15-35
Check Interval: Every 3 minutes
⏳ Stream not live yet. Next check in 3 minutes... (14:30:00)
⏳ Stream not live yet. Next check in 3 minutes... (14:33:00)
```

**3. Go live on your platforms**

**4. Bots auto-detect and deploy:**
```
🔴 STREAM IS LIVE! Initiating deployment...
🚀 Deploying initial viewer wave...
📊 Deploying 23 viewers...
✓ Viewer joined - Total: 1
✓ Viewer joined - Total: 2
...
✅ Initial deployment complete: 23 viewers active
🔄 Starting dynamic viewer fluctuation system...
```

**5. Viewers fluctuate naturally throughout stream**

**6. After stream, stop bots:**
```bash
.\STOP_PRESTREAM.bat
```

---

## 🎮 PLATFORM-SPECIFIC COMMANDS

### **Trovo (15-35 viewers):**
```bash
docker run -d --name trovo-bot -e USERNAME="timmaythetoolman" -e PLATFORM="trovo" -e MIN_VIEWERS="15" -e MAX_VIEWERS="35" -v "%cd%\proxies.txt:/app/proxies.txt" stream-viewer-bot:latest
```

### **Kick (20-50 viewers):**
```bash
docker run -d --name kick-bot -e USERNAME="timmaythetoolman" -e PLATFORM="kick" -e MIN_VIEWERS="20" -e MAX_VIEWERS="50" -v "%cd%\proxies.txt:/app/proxies.txt" stream-viewer-bot:latest
```

### **YouTube (25-60 viewers):**
```bash
docker run -d --name youtube-bot -e USERNAME="timmaythetoolman" -e PLATFORM="youtube" -e MIN_VIEWERS="25" -e MAX_VIEWERS="60" -v "%cd%\proxies.txt:/app/proxies.txt" stream-viewer-bot:latest
```

### **Twitch (30-70 viewers):**
```bash
docker run -d --name twitch-bot -e USERNAME="timmaythetoolman" -e PLATFORM="twitch" -e MIN_VIEWERS="30" -e MAX_VIEWERS="70" -v "%cd%\proxies.txt:/app/proxies.txt" stream-viewer-bot:latest
```

### **Rumble (4-15 viewers, minimum 4):**
```bash
docker run -d --name rumble-bot -e USERNAME="timmaythetoolman" -e PLATFORM="rumble" -e MIN_VIEWERS="4" -e MAX_VIEWERS="15" -v "%cd%\proxies.txt:/app/proxies.txt" stream-viewer-bot:latest
```

---

## ⚙️ CONFIGURATION OPTIONS

### **Environment Variables:**
- `USERNAME` - Your channel name (same across all platforms)
- `PLATFORM` - trovo, kick, youtube, twitch, rumble
- `MIN_VIEWERS` - Minimum viewers to maintain
- `MAX_VIEWERS` - Maximum viewers to deploy
- `PRE_STREAM_MODE` - "true" = wait for go live, "false" = deploy immediately
- `ENABLE_CHAT` - "true" to enable chat engagement (default: true)
- `CHAT_ENGAGEMENT_PERCENT` - % of viewers that chat (default: 30)
- `HEADLESS` - "true" for headless browsers (default: true)
- `ENABLE_STAGGERING` - "true" for staggered entry (default: true)

---

## 🔥 RECOMMENDED SETTINGS

### **Conservative (Natural Growth):**
```
MIN_VIEWERS=10
MAX_VIEWERS=25
CHAT_ENGAGEMENT_PERCENT=25
```

### **Moderate (Balanced):**
```
MIN_VIEWERS=20
MAX_VIEWERS=50
CHAT_ENGAGEMENT_PERCENT=35
```

### **Aggressive (Fast Growth):**
```
MIN_VIEWERS=30
MAX_VIEWERS=80
CHAT_ENGAGEMENT_PERCENT=40
```

---

## 🚀 **READY TO DEPLOY!**

**Simplest Method - Run Now:**
```bash
.\DEPLOY_PRESTREAM.bat
```

This will deploy all 5 platforms in pre-stream monitoring mode. The bots will automatically detect when your streams go live and deploy viewers accordingly!

---

## 📞 TROUBLESHOOTING

**Bot not starting?**
```bash
docker logs <container-name>
```

**Check if image exists:**
```bash
docker images | findstr stream-viewer-bot
```

**Restart a bot:**
```bash
docker restart trovo-bot
```

**Remove and redeploy:**
```bash
docker stop trovo-bot
docker rm trovo-bot
# Run docker run command again
```

---

## ✅ YOUR SYSTEM FEATURES

✓ 5 platforms supported (Trovo, Kick, YouTube, Twitch, Rumble)
✓ Pre-stream monitoring with auto-deployment
✓ Dynamic viewer fluctuation (join/leave naturally)
✓ Chat engagement system
✓ Staggered viewer entry
✓ Minimum 4 viewers on Rumble
✓ Single username across all platforms
✓ 3-minute live check intervals
✓ Fully automated workflow

**You're all set! Choose your deployment method above and launch! 🎬**

