# 🚀 Multi-Platform Stream Viewer Bot - Deployment Guide

## ✅ Supported Platforms
- **Trovo** - trovo.live
- **Kick** - kick.com  
- **YouTube** - youtube.com
- **Twitch** - twitch.tv

---

## 📋 Features Implemented

### ✓ Multi-Platform Support
All four major streaming platforms with automatic detection

### ✓ Dynamic Viewer Amount
Random viewer count between MIN_VIEWERS and MAX_VIEWERS for natural appearance

### ✓ Staggered Entry System
Viewers join gradually with randomized delays to avoid detection

### ✓ Dynamic Entry Phasing
Smart 3-phase deployment system that adapts to viewer count

### ✓ Chat Engagement System
Configurable percentage of viewers participate in chat with realistic delays

### ✓ Headless/Visible Mode
Choose between resource-efficient headless or visible browser mode

---

## 🔧 Environment Variables

### **Platform Configuration**
```bash
TARGET_STREAM_URL     # Full URL to your stream (required)
PLATFORM              # trovo, kick, youtube, twitch, auto (default: auto)
```

### **Viewer Configuration**
```bash
MIN_VIEWERS           # Minimum viewers to deploy (default: 10)
MAX_VIEWERS           # Maximum viewers to deploy (default: 25)
HEADLESS              # true/false - headless mode (default: true)
```

### **Staggered Deployment**
```bash
ENABLE_STAGGERING     # Enable staggered entry (default: true)
STAGGER_DELAY_MIN     # Min delay between viewers in ms (default: 3000)
STAGGER_DELAY_MAX     # Max delay between viewers in ms (default: 8000)
ENABLE_DYNAMIC_ENTRY  # Random viewer count (default: true)
```

### **Chat Engagement**
```bash
ENABLE_CHAT                # Enable chat participation (default: true)
CHAT_ENGAGEMENT_PERCENT    # % of viewers that chat (default: 30)
MIN_CHAT_DELAY            # Min seconds between messages (default: 45)
MAX_CHAT_DELAY            # Max seconds between messages (default: 180)
AGGRESSIVE_CHAT           # More frequent messages (default: false)
```

### **Proxy Configuration**
```bash
PROXY_LIST_PATH       # Path to proxy file (default: /app/proxies.txt)
```

---

## 🎯 Quick Start Examples

### **Option 1: Single Platform (Trovo)**
```bash
docker run -d \
  -e TARGET_STREAM_URL="https://trovo.live/s/timmaythetoolman" \
  -e PLATFORM="trovo" \
  -e MIN_VIEWERS="15" \
  -e MAX_VIEWERS="35" \
  -e ENABLE_CHAT="true" \
  -e CHAT_ENGAGEMENT_PERCENT="35" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest
```

### **Option 2: Docker Compose - All Platforms**
```bash
# Start all platform bots
docker-compose -f docker-compose.multi-platform.yml up -d

# Start only Trovo bot
docker-compose -f docker-compose.multi-platform.yml up -d trovo-bot

# Start Trovo + Kick bots
docker-compose -f docker-compose.multi-platform.yml up -d trovo-bot kick-bot
```

### **Option 3: Auto-Detect Platform**
```bash
docker run -d \
  -e TARGET_STREAM_URL="https://kick.com/your-channel" \
  -e PLATFORM="auto" \
  -e MIN_VIEWERS="20" \
  -e MAX_VIEWERS="50" \
  -v ./proxies.txt:/app/proxies.txt \
  stream-viewer-bot:latest
```

---

## 📊 Deployment Strategies

### **Conservative (Natural Growth)**
```bash
MIN_VIEWERS=10
MAX_VIEWERS=25
STAGGER_DELAY_MIN=5000
STAGGER_DELAY_MAX=12000
CHAT_ENGAGEMENT_PERCENT=25
```

### **Moderate (Balanced)**
```bash
MIN_VIEWERS=20
MAX_VIEWERS=50
STAGGER_DELAY_MIN=3000
STAGGER_DELAY_MAX=8000
CHAT_ENGAGEMENT_PERCENT=35
```

### **Aggressive (Fast Growth)**
```bash
MIN_VIEWERS=30
MAX_VIEWERS=80
STAGGER_DELAY_MIN=2000
STAGGER_DELAY_MAX=5000
CHAT_ENGAGEMENT_PERCENT=40
AGGRESSIVE_CHAT=true
```

---

## 🔄 Phase Deployment System

The bot automatically calculates optimal deployment phases:

### **Small Deployment (≤5 viewers)**
- Single phase deployment

### **Medium Deployment (6-20 viewers)**
- **Phase 1**: 3 viewers (slower)
- **Phase 2**: 7 viewers (moderate)
- **Phase 3**: Remaining (faster)

### **Large Deployment (>20 viewers)**
- **Phase 1**: 5 viewers (7000ms intervals)
- **Phase 2**: 10 viewers (5000ms intervals)
- **Phase 3**: Remaining (3500ms intervals)

**Inter-phase breaks**: 10-20 seconds for natural appearance

---

## 📈 Monitoring & Logs

### **View Live Logs**
```bash
# All platforms
docker-compose -f docker-compose.multi-platform.yml logs -f

# Specific platform
docker logs -f trovo-viewer-bot
```

### **Log Files Location**
```
./logs/trovo/bot-<date>.log
./logs/kick/bot-<date>.log
./logs/youtube/bot-<date>.log
./logs/twitch/bot-<date>.log
```

### **Log Retention**
- Automatically rotates daily
- Keeps last 7 days of logs

---

## 🛠️ Advanced Configuration

### **Run Multiple Instances per Platform**
```bash
# Trovo Instance 1 (15-35 viewers)
docker run -d --name trovo-bot-1 \
  -e TARGET_STREAM_URL="https://trovo.live/s/timmaythetoolman" \
  -e MIN_VIEWERS="15" -e MAX_VIEWERS="35" \
  stream-viewer-bot:latest

# Trovo Instance 2 (20-40 viewers)
docker run -d --name trovo-bot-2 \
  -e TARGET_STREAM_URL="https://trovo.live/s/timmaythetoolman" \
  -e MIN_VIEWERS="20" -e MAX_VIEWERS="40" \
  stream-viewer-bot:latest
```

### **Visible Browser Mode (Debug)**
```bash
docker run -d \
  -e TARGET_STREAM_URL="https://trovo.live/s/timmaythetoolman" \
  -e HEADLESS="false" \
  -e DISPLAY=:0 \
  stream-viewer-bot:latest
```

---

## 🔥 Production Deployment

### **Build the Image**
```bash
docker build -t stream-viewer-bot:latest .
```

### **Deploy with Docker Compose**
```bash
# Create logs directory
mkdir -p logs/{trovo,kick,youtube,twitch}

# Start services
docker-compose -f docker-compose.multi-platform.yml up -d

# Check status
docker-compose -f docker-compose.multi-platform.yml ps

# Scale specific service
docker-compose -f docker-compose.multi-platform.yml up -d --scale trovo-bot=3
```

### **Stop Services**
```bash
# Stop all
docker-compose -f docker-compose.multi-platform.yml down

# Stop specific platform
docker-compose -f docker-compose.multi-platform.yml stop trovo-bot
```

---

## 🎮 Platform-Specific Notes

### **Trovo**
- Best chat engagement: 30-40%
- Recommended viewer range: 15-50
- Stagger delay: 3000-8000ms

### **Kick**
- Best chat engagement: 35-45%
- Recommended viewer range: 20-60
- Stagger delay: 4000-10000ms

### **YouTube**
- Best chat engagement: 20-30%
- Recommended viewer range: 25-80
- Stagger delay: 5000-12000ms

### **Twitch**
- Best chat engagement: 25-35%
- Recommended viewer range: 30-100
- Stagger delay: 3500-9000ms

---

## 🔒 Security Best Practices

1. **Use Quality Proxies**: Residential > Datacenter
2. **Rotate Proxies**: Change proxy list regularly
3. **Realistic Numbers**: Don't deploy 1000 viewers instantly
4. **Chat Variety**: Use diverse chat message templates
5. **Monitor Performance**: Watch for detection patterns

---

## 📞 Troubleshooting

### **Bot Not Starting**
```bash
# Check logs
docker logs <container-name>

# Verify environment variables
docker inspect <container-name> | grep -A 20 Env
```

### **No Viewers Joining**
- Check TARGET_STREAM_URL is correct
- Verify proxy file exists and has valid proxies
- Check platform is correctly detected/specified

### **Chat Not Working**
- Ensure ENABLE_CHAT="true"
- Verify chat engagement percentage > 0
- Check chat message templates exist

---

## 🚀 Ready to Deploy!

Your system now supports:
✅ All 4 major platforms (Trovo, Kick, YouTube, Twitch)
✅ Dynamic viewer amounts with min/max ranges
✅ Staggered entry with configurable delays
✅ 3-phase deployment system
✅ Advanced chat engagement
✅ Headless & visible modes
✅ Full Docker integration

**Start your first bot:**
```bash
docker-compose -f docker-compose.multi-platform.yml up -d trovo-bot
```
version: '3.8'

services:
  # ============================================
  # TROVO DEPLOYMENT
  # ============================================
  trovo-bot:
    image: stream-viewer-bot:latest
    container_name: trovo-viewer-bot
    environment:
      # Platform & Stream Configuration
      TARGET_STREAM_URL: "https://trovo.live/s/timmaythetoolman"
      PLATFORM: "trovo"                    # trovo, kick, youtube, twitch, auto
      
      # Dynamic Viewer Configuration
      MIN_VIEWERS: "15"                    # Minimum viewers to deploy
      MAX_VIEWERS: "35"                    # Maximum viewers to deploy
      
      # Deployment Strategy
      HEADLESS: "true"                     # true = headless, false = visible browsers
      ENABLE_STAGGERING: "true"            # Stagger viewer entry
      STAGGER_DELAY_MIN: "3000"            # Min delay between viewers (ms)
      STAGGER_DELAY_MAX: "8000"            # Max delay between viewers (ms)
      ENABLE_DYNAMIC_ENTRY: "true"         # Random viewer count within range
      
      # Chat Engagement System
      ENABLE_CHAT: "true"                  # Enable chat participation
      CHAT_ENGAGEMENT_PERCENT: "35"        # % of viewers that will chat
      MIN_CHAT_DELAY: "45"                 # Min seconds between messages
      MAX_CHAT_DELAY: "180"                # Max seconds between messages
      AGGRESSIVE_CHAT: "false"             # More frequent chatting
      
      # Proxy Configuration
      PROXY_LIST_PATH: "/app/proxies.txt"
    volumes:
      - ./proxies.txt:/app/proxies.txt:ro
      - ./logs/trovo:/app/logs
    restart: unless-stopped
    networks:
      - bot-network

  # ============================================
  # KICK DEPLOYMENT
  # ============================================
  kick-bot:
    image: stream-viewer-bot:latest
    container_name: kick-viewer-bot
    environment:
      TARGET_STREAM_URL: "https://kick.com/your-channel"
      PLATFORM: "kick"
      MIN_VIEWERS: "20"
      MAX_VIEWERS: "50"
      HEADLESS: "true"
      ENABLE_STAGGERING: "true"
      STAGGER_DELAY_MIN: "4000"
      STAGGER_DELAY_MAX: "10000"
      ENABLE_DYNAMIC_ENTRY: "true"
      ENABLE_CHAT: "true"
      CHAT_ENGAGEMENT_PERCENT: "40"
      MIN_CHAT_DELAY: "60"
      MAX_CHAT_DELAY: "200"
      AGGRESSIVE_CHAT: "false"
      PROXY_LIST_PATH: "/app/proxies.txt"
    volumes:
      - ./proxies.txt:/app/proxies.txt:ro
      - ./logs/kick:/app/logs
    restart: unless-stopped
    networks:
      - bot-network

  # ============================================
  # YOUTUBE DEPLOYMENT
  # ============================================
  youtube-bot:
    image: stream-viewer-bot:latest
    container_name: youtube-viewer-bot
    environment:
      TARGET_STREAM_URL: "https://youtube.com/watch?v=YOUR_VIDEO_ID"
      PLATFORM: "youtube"
      MIN_VIEWERS: "25"
      MAX_VIEWERS: "60"
      HEADLESS: "true"
      ENABLE_STAGGERING: "true"
      STAGGER_DELAY_MIN: "5000"
      STAGGER_DELAY_MAX: "12000"
      ENABLE_DYNAMIC_ENTRY: "true"
      ENABLE_CHAT: "true"
      CHAT_ENGAGEMENT_PERCENT: "25"
      MIN_CHAT_DELAY: "90"
      MAX_CHAT_DELAY: "240"
      AGGRESSIVE_CHAT: "false"
      PROXY_LIST_PATH: "/app/proxies.txt"
    volumes:
      - ./proxies.txt:/app/proxies.txt:ro
      - ./logs/youtube:/app/logs
    restart: unless-stopped
    networks:
      - bot-network

  # ============================================
  # TWITCH DEPLOYMENT
  # ============================================
  twitch-bot:
    image: stream-viewer-bot:latest
    container_name: twitch-viewer-bot
    environment:
      TARGET_STREAM_URL: "https://twitch.tv/your-channel"
      PLATFORM: "twitch"
      MIN_VIEWERS: "30"
      MAX_VIEWERS: "70"
      HEADLESS: "true"
      ENABLE_STAGGERING: "true"
      STAGGER_DELAY_MIN: "3500"
      STAGGER_DELAY_MAX: "9000"
      ENABLE_DYNAMIC_ENTRY: "true"
      ENABLE_CHAT: "true"
      CHAT_ENGAGEMENT_PERCENT: "30"
      MIN_CHAT_DELAY: "50"
      MAX_CHAT_DELAY: "160"
      AGGRESSIVE_CHAT: "false"
      PROXY_LIST_PATH: "/app/proxies.txt"
    volumes:
      - ./proxies.txt:/app/proxies.txt:ro
      - ./logs/twitch:/app/logs
    restart: unless-stopped
    networks:
      - bot-network

  # ============================================
  # MULTI-PLATFORM AUTO-DETECT
  # ============================================
  auto-bot:
    image: stream-viewer-bot:latest
    container_name: auto-viewer-bot
    environment:
      TARGET_STREAM_URL: "https://your-stream-url-here"
      PLATFORM: "auto"                     # Auto-detect platform from URL
      MIN_VIEWERS: "10"
      MAX_VIEWERS: "40"
      HEADLESS: "true"
      ENABLE_STAGGERING: "true"
      STAGGER_DELAY_MIN: "3000"
      STAGGER_DELAY_MAX: "8000"
      ENABLE_DYNAMIC_ENTRY: "true"
      ENABLE_CHAT: "true"
      CHAT_ENGAGEMENT_PERCENT: "30"
      MIN_CHAT_DELAY: "45"
      MAX_CHAT_DELAY: "180"
      AGGRESSIVE_CHAT: "false"
      PROXY_LIST_PATH: "/app/proxies.txt"
    volumes:
      - ./proxies.txt:/app/proxies.txt:ro
      - ./logs/auto:/app/logs
    restart: unless-stopped
    networks:
      - bot-network
    profiles:
      - manual  # Only start with: docker-compose --profile manual up

networks:
  bot-network:
    driver: bridge

