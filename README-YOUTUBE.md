﻿# YOUTUBE WATCH TIME BOOSTER

## Purpose
Deploy 30 concurrent Chromium browsers that:
- Navigate to your YouTube channel's `/videos` and `/shorts` pages
- Play random videos to accumulate watch time
- Use visual rendering (Xvfb) to defeat anti-bot detection
- **NO LIVE STREAM REQUIRED** - Works with static recorded content

## Entry Point
**WatchTimeBoosterEntryPoint.cs** - YouTube static video watch time accumulation

## Requirements

### 1. YouTube Channel with Content
Target URL:
```
https://youtube.com/@timmaythetoolman/videos
https://youtube.com/@timmaythetoolman/shorts
```

**No authentication required** - Bots play public videos as anonymous viewers.

### 2. System Resources
- **CPU**: 4+ cores
- **RAM**: 8+ GB
- **Docker**: Desktop 4.0+ with legacy builder support

## Deployment

### Quick Start
```bash
cd ~/RiderProjects/Stream-Viewer-Chat-Bot
chmod +x deploy-youtube.sh
./deploy-youtube.sh
```

### Manual Deployment
```bash
# Build image
DOCKER_BUILDKIT=0 docker build -t botcore-youtube:latest -f Dockerfile.youtube .

# Deploy
docker-compose -f docker-compose.youtube.yml up -d

# Monitor
docker logs -f botcore-youtube
```

## Configuration

Edit `docker-compose.youtube.yml` environment variables:

```yaml
environment:
  - CHANNEL_USERNAME=timmaythetoolman      # Your YouTube channel
  - VIEWER_COUNT=30                         # Concurrent viewers
  - HEADLESS=false                          # MUST be false
  - LOW_CPU_RAM=false                       # MUST be false
  - DISPLAY=:99                             # Xvfb display (DO NOT CHANGE)
```

**Note**: No PLATFORM or chat settings - this is YouTube-specific with no chat.

## Monitoring

### Check Status
```bash
docker ps --filter "name=youtube"
```

### View Logs
```bash
docker logs -f botcore-youtube
```

### Health Check
```bash
curl http://localhost:8081/health
```

### Resource Usage
```bash
docker stats --no-stream botcore-youtube
```

**Expected Usage**:
- CPU: 150-300% (1.5-3 cores)
- RAM: 3-6 GB
- Chromium processes: 30+

## Expected Behavior

### Startup (First 60 seconds)
```
[INFO] Starting Adaptive Watch Time Booster
[INFO] Channel: timmaythetoolman
[INFO] Xvfb display server started on :99
[INFO] Launching 30 viewer instances
[INFO] Browser 1/30 launched
[INFO] Browser 2/30 launched
...
```

### Runtime
```
[INFO] Navigating to https://youtube.com/@timmaythetoolman/videos
[INFO] Playing video: "Your Video Title Here"
[INFO] Watch time accumulated: 5 minutes
[INFO] Rotating to next video
[INFO] Playing short: "Your Short Title"
```

### On YouTube Analytics
- **Watch time increases** on your videos and shorts
- **Views increase** (30 concurrent viewers)
- **Retention metrics improve** (bots watch full videos)

## How It Works

1. **Navigates to `/videos` page** → Finds list of uploaded videos
2. **Selects random video** → Clicks and plays
3. **Watches to completion** → Accumulates watch time
4. **Rotates to `/shorts`** → Plays random shorts
5. **Repeats indefinitely** → Continuous watch time accumulation

**Key Difference from Twitch**: 
- Does NOT connect to live streams
- Does NOT require you to be streaming
- Works 24/7 on static content

## Troubleshooting

### No Watch Time Increase
1. **Check logs**: `docker logs botcore-youtube --tail 100`
2. **Verify channel URL**: Ensure /videos page has content
3. **Check browser launch**: `docker exec botcore-youtube sh -c "ps aux | grep chromium"`

### Container Restarting
1. **Check BotCore.dll exists**: `docker exec botcore-youtube ls -la /app/BotCore.dll`
2. **Verify Xvfb**: `docker exec botcore-youtube sh -c "ps aux | grep Xvfb"`
3. **Check errors**: `docker logs botcore-youtube 2>&1 | grep -i error`

### Low CPU/RAM Usage
- If CPU <100% or RAM <2GB, browsers may not be launching
- Check for errors in logs
- Verify Dockerfile.youtube was built correctly

## Stop/Remove

```bash
# Stop containers
docker-compose -f docker-compose.youtube.yml down

# Remove image
docker rmi botcore-youtube:latest

# Clean logs
rm -rf logs/youtube/*
```

## Architecture

```
Dockerfile.youtube
  ↓
Builds with WatchTimeBoosterEntryPoint.cs
  ↓
Creates botcore-youtube:latest image
  ↓
Deploys via docker-compose.youtube.yml
  ↓
Container: botcore-youtube
  - 30 Chromium browsers (visible mode via Xvfb)
  - Navigates to /videos and /shorts pages
  - Plays random content continuously
  - No authentication required
  - No live stream dependency
```

## Ports

- **5001**: Internal app endpoint (not for user access)
- **8081**: Health check endpoint
- **9091**: Prometheus metrics
- **3001**: Grafana dashboard (admin/admin)

## Important Notes

⚠️ **HEADLESS=false is CRITICAL** - YouTube detects pure headless browsers immediately. This system uses Xvfb (virtual framebuffer) to render full browser UI without physical display, defeating anti-bot detection.

⚠️ **No Live Stream Required** - Unlike Twitch bot, this works 24/7 on static videos.

⚠️ **Resource Intensive** - 30 browsers with full rendering uses significant CPU/RAM.

⚠️ **Watch Time != Views** - YouTube may filter suspicious views, but watch time accumulation is harder to detect.

## Comparison: Twitch vs YouTube Bot

| Feature | Twitch Bot | YouTube Bot |
|---------|-----------|-------------|
| **Entry Point** | DockerEntryPoint.cs | WatchTimeBoosterEntryPoint.cs |
| **Target** | LIVE streams | Static videos/shorts |
| **Authentication** | OAuth required | Not required |
| **Chat** | Yes (25% engagement) | No |
| **Live Dependency** | Must be streaming | Works anytime |
| **URL** | /timmaythetoolman | /@timmaythetoolman/videos |
| **Viewers** | 50 | 30 |

## Support

For issues, check:
1. Container logs: `docker logs botcore-youtube`
2. Health endpoint: `curl http://localhost:8081/health`
3. Process count: `docker exec botcore-youtube sh -c "ps aux | grep chromium | wc -l"`

Expected: 30+ chromium processes if working correctly.
# TWITCH LIVE STREAM VIEWER + CHAT BOT

## Purpose
Deploy 50 concurrent Chromium browsers that:
- Connect to your **LIVE Twitch stream** as viewers
- Send chat messages with 25% engagement rate (12-13 messages)
- Use visual rendering (Xvfb) to defeat anti-bot detection
- Require OAuth authentication for chat participation

## Entry Point
**DockerEntryPoint.cs** - Multi-platform live stream viewer

## Requirements

### 1. Active Live Stream
**YOU MUST BE LIVE STREAMING ON TWITCH** when running this bot. It connects to:
```
https://twitch.tv/timmaythetoolman
```

### 2. OAuth Authentication (REQUIRED for Chat)
Create `BotCore/appsettings.twitch.json`:
```json
{
  "Twitch": {
    "Username": "your_bot_account_username",
    "OAuthToken": "oauth:your_token_here",
    "Channel": "timmaythetoolman"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Get OAuth Token**:

⚠️ **IMPORTANT:** Twitchapps TMI has been discontinued. Use modern alternatives:

**Option 1 (Recommended for End Users):**
1. Visit: https://twitchtokengenerator.com/
2. Click "Bot Chat Token"
3. Authorize with your bot account
4. Copy the access token
5. Paste into `config/appsettings.twitch.json`

**Option 2 (For Developers):**
- See `TWITCH_OAUTH_GUIDE.md` for official Twitch OAuth setup
- Use Twitch Developer Console: https://dev.twitch.tv/console/apps

**Required Scopes:** `chat:read chat:edit channel:moderate`

### 3. System Resources
- **CPU**: 4+ cores
- **RAM**: 8+ GB
- **Docker**: Desktop 4.0+ with legacy builder support

## Deployment

### Quick Start
```bash
cd ~/RiderProjects/Stream-Viewer-Chat-Bot
chmod +x deploy-twitch.sh
./deploy-twitch.sh
```

### Manual Deployment
```bash
# Build image
DOCKER_BUILDKIT=0 docker build -t botcore-twitch:latest -f Dockerfile.twitch .

# Deploy
docker-compose -f docker-compose.twitch.yml up -d

# Monitor
docker logs -f botcore-twitch
```

## Configuration

Edit `docker-compose.twitch.yml` environment variables:

```yaml
environment:
  - PLATFORM=twitch
  - CHANNEL_USERNAME=timmaythetoolman      # Your Twitch channel
  - VIEWER_COUNT=50                         # Concurrent viewers
  - HEADLESS=false                          # MUST be false
  - LOW_CPU_RAM=false                       # MUST be false
  - ENABLE_CHAT=true                        # Enable chat
  - CHAT_ENGAGEMENT_PERCENT=25              # 25% of viewers chat
  - MIN_CHAT_DELAY=60                       # Min seconds between messages
  - MAX_CHAT_DELAY=240                      # Max seconds between messages
  - DISPLAY=:99                             # Xvfb display (DO NOT CHANGE)
```

## Monitoring

### Check Status
```bash
docker ps | grep twitch
```

### View Logs
```bash
docker logs -f botcore-twitch
```

### Health Check
```bash
curl http://localhost:8080/health
```

### Resource Usage
```bash
docker stats botcore-twitch
```

**Expected Usage**:
- CPU: 200-400% (2-4 cores)
- RAM: 4-8 GB
- Chromium processes: 50+

## Expected Behavior

### Startup (First 60 seconds)
```
[INFO] Xvfb display server started on :99
[INFO] Launching 50 viewer instances for twitch.tv/timmaythetoolman
[INFO] Browser 1/50 launched
[INFO] Browser 2/50 launched
...
[INFO] All browsers connected to stream
```

### Runtime
```
[INFO] Chat engagement enabled (25%)
[INFO] Sending chat message: "Great stream!"
[INFO] Viewer 23 sent message: "Love this content!"
```

### On Your Twitch Stream
- **Viewer count increases by ~50**
- **Chat messages appear every 60-240 seconds**
- **Viewers persist for duration of stream**

## Troubleshooting

### No Viewers Appearing
1. **Verify you are LIVE**: Bot only works when stream is active
2. **Check logs**: `docker logs botcore-twitch --tail 100`
3. **Verify OAuth**: Ensure appsettings.twitch.json has valid token

### No Chat Messages
1. **Check OAuth token**: Invalid token = no chat capability
2. **Verify ENABLE_CHAT=true**
3. **Check engagement percent**: 25% = ~12 chatters from 50 viewers

### Container Restarting
1. **Check BotCore.dll exists**: `docker exec botcore-twitch ls -la /app/BotCore.dll`
2. **Verify Xvfb**: `docker exec botcore-twitch ps aux | grep Xvfb`
3. **Check errors**: `docker logs botcore-twitch | grep -i error`

## Stop/Remove

```bash
# Stop containers
docker-compose -f docker-compose.twitch.yml down

# Remove image
docker rmi botcore-twitch:latest

# Clean logs
rm -rf logs/twitch/*
```

## Architecture

```
Dockerfile.twitch
  ↓
Builds with DockerEntryPoint.cs
  ↓
Creates botcore-twitch:latest image
  ↓
Deploys via docker-compose.twitch.yml
  ↓
Container: botcore-twitch
  - 50 Chromium browsers (visible mode via Xvfb)
  - WebSocket connections for real-time chat
  - OAuth authenticated chat participation
  - Connects to LIVE Twitch stream only
```

## Ports

- **5000**: Internal app endpoint (not for user access)
- **8080**: Health check endpoint
- **9090**: Prometheus metrics
- **3000**: Grafana dashboard (admin/admin)

## Important Notes

⚠️ **HEADLESS=false is CRITICAL** - Twitch detects pure headless browsers immediately. This system uses Xvfb (virtual framebuffer) to render full browser UI without physical display, defeating anti-bot detection.

⚠️ **OAuth Token Required** - Without valid OAuth, bot can view but cannot chat.

⚠️ **Live Stream Required** - Bot connects to `/timmaythetoolman` - if you're not live, it will fail or hang.

⚠️ **Resource Intensive** - 50 browsers with full rendering uses significant CPU/RAM.

## Support

For issues, check:
1. Container logs: `docker logs botcore-twitch`
2. Health endpoint: `curl http://localhost:8080/health`
3. Process count: `docker exec botcore-twitch ps aux | grep chromium | wc -l`

Expected: 50+ chromium processes if working correctly.

