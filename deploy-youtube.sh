#!/bin/bash
set -e

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║          YOUTUBE WATCH TIME BOOSTER DEPLOYMENT                  ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Purpose: Accumulate watch time on YouTube videos and shorts"
echo "Entry Point: WatchTimeBoosterEntryPoint.cs"
echo "Target: Static videos (/videos and /shorts pages)"
echo ""

# Verify Docker
echo "[1/4] Checking Docker status..."
if ! docker info > /dev/null 2>&1; then
    echo "✗ Docker is not running"
    exit 1
fi
echo "✓ Docker is running"
echo ""

# Build YouTube-specific image
echo "[2/4] Building YouTube watch time booster image..."
echo "  → Entry Point: WatchTimeBoosterEntryPoint (static videos)"
echo "  → Visual Rendering: ENABLED (Xvfb on :99)"
DOCKER_BUILDKIT=0 docker build -t botcore-youtube:latest -f Dockerfile.youtube . || {
    echo "✗ Build failed"
    exit 1
}
echo "✓ YouTube image built successfully"
echo ""

# Deploy
echo "[3/4] Deploying YouTube watch time booster..."
docker-compose -f docker-compose.youtube.yml up -d || {
    echo "✗ Deployment failed"
    exit 1
}
echo "✓ YouTube booster deployed"
echo ""

# Verify
echo "[4/4] Verifying deployment..."
sleep 5
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep youtube
echo ""

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║          YOUTUBE WATCH TIME BOOSTER ACTIVE                       ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Configuration:"
echo "  • Platform: YouTube Static Videos"
echo "  • Channel: timmaythetoolman"
echo "  • Viewers: 30 concurrent"
echo "  • Target: /videos and /shorts pages"
echo "  • Visual Mode: ENABLED (defeats anti-bot detection)"
echo ""
echo "NO LIVE STREAM REQUIRED - Plays recorded content"
echo ""
echo "Monitor:"
echo "  docker logs -f botcore-youtube"
echo ""
echo "Health:"
echo "  curl http://localhost:8081/health"
echo ""
echo "Stop:"
echo "  docker-compose -f docker-compose.youtube.yml down"
#!/bin/bash
set -e

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║        TWITCH LIVE STREAM VIEWER + CHAT DEPLOYMENT              ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Purpose: Deploy Twitch live stream viewer with real-time chat"
echo "Entry Point: DockerEntryPoint.cs"
echo "Requirements: Active Twitch stream + OAuth authentication"
echo ""

# Verify Docker
echo "[1/4] Checking Docker status..."
if ! docker info > /dev/null 2>&1; then
    echo "✗ Docker is not running"
    exit 1
fi
echo "✓ Docker is running"
echo ""

# Build Twitch-specific image
echo "[2/4] Building Twitch bot image..."
echo "  → Entry Point: DockerEntryPoint (live stream + chat)"
echo "  → Visual Rendering: ENABLED (Xvfb on :99)"
DOCKER_BUILDKIT=0 docker build -t botcore-twitch:latest -f Dockerfile.twitch . || {
    echo "✗ Build failed"
    exit 1
}
echo "✓ Twitch image built successfully"
echo ""

# Deploy
echo "[3/4] Deploying Twitch bot..."
docker-compose -f docker-compose.twitch.yml up -d || {
    echo "✗ Deployment failed"
    exit 1
}
echo "✓ Twitch bot deployed"
echo ""

# Verify
echo "[4/4] Verifying deployment..."
sleep 5
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep twitch
echo ""

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║              TWITCH DEPLOYMENT COMPLETE                          ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Configuration:"
echo "  • Platform: Twitch Live Stream"
echo "  • Channel: timmaythetoolman"
echo "  • Viewers: 50 concurrent"
echo "  • Chat: 25% engagement (12-13 messages)"
echo "  • Visual Mode: ENABLED (defeats anti-bot detection)"
echo ""
echo "IMPORTANT: Ensure you are LIVE STREAMING on Twitch!"
echo ""
echo "Monitor:"
echo "  docker logs -f botcore-twitch"
echo ""
echo "Health:"
echo "  curl http://localhost:8080/health"
echo ""
echo "Stop:"
echo "  docker-compose -f docker-compose.twitch.yml down"

