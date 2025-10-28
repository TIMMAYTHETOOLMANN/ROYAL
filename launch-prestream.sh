#!/bin/bash
set -e

echo "🚀 PRE-STREAM QUICK LAUNCH - LIVE VIEWER + CHAT SYSTEM"
echo "======================================================="

# Configuration
VIEWER_COUNT=${1:-20}
CHAT_ENABLED=${2:-"true"}
CONTAINER_NAME="prestream-live-dynamic"
IMAGE_NAME="prestream-bot:latest"

echo "🎯 MODE: DYNAMIC (60% Twitch / 40% YouTube)"
echo "👥 TOTAL VIEWERS: $VIEWER_COUNT"
echo "💬 CHAT ENGAGEMENT: $CHAT_ENABLED"
echo "======================================================="

# Cleanup any existing deployment
echo "🧹 Cleaning up previous deployment..."
docker stop $CONTAINER_NAME 2>/dev/null || true
docker rm $CONTAINER_NAME 2>/dev/null || true

# Quick build
echo "🏗️ Building pre-stream image..."
docker build -f Dockerfile.prestream -t $IMAGE_NAME . || {
    echo "❌ Build failed! Check BotCore project structure."
    exit 1
}

# Deploy with optimized settings for 20 dynamic viewers
echo "🚀 Deploying dynamic multi-platform viewer system..."
docker run -d \
    --name $CONTAINER_NAME \
    --restart unless-stopped \
    --memory=6g \
    --cpus=4.0 \
    --shm-size=3g \
    -e PLATFORM=dynamic \
    -e VIEWER_COUNT=$VIEWER_COUNT \
    -e CHAT_ENABLED=$CHAT_ENABLED \
    -e DISPLAY=:99 \
    -v $(pwd)/logs:/app/logs \
    $IMAGE_NAME

echo ""
echo "✅ DYNAMIC DEPLOYMENT COMPLETE!"
echo "================================================"
echo "📊 Container: $CONTAINER_NAME"
echo "🌐 Platforms: Twitch + YouTube (60/40 split)"
echo "👥 Active Viewers: $VIEWER_COUNT"
TWITCH_COUNT=$((VIEWER_COUNT * 60 / 100))
YOUTUBE_COUNT=$((VIEWER_COUNT * 40 / 100))
echo "   🟣 Twitch: ~$TWITCH_COUNT viewers"
echo "   🔴 YouTube: ~$YOUTUBE_COUNT viewers"
echo "💬 Chat Engagement: $CHAT_ENABLED"
echo ""
echo "📝 Monitor: docker logs -f $CONTAINER_NAME"
echo "📊 Stats: docker stats $CONTAINER_NAME"
echo "🛑 Stop: docker stop $CONTAINER_NAME"
echo "================================================"

# Quick health check
sleep 5
if docker ps | grep -q $CONTAINER_NAME; then
    echo "🎉 SYSTEM LIVE - VIEWERS ACTIVE ON YOUR STREAM!"
    echo ""
    echo "💡 TIP: Your stream will show $VIEWER_COUNT live viewers"
    echo "💡 Chat engagement will occur every 1-3 minutes"
    echo ""
    docker logs --tail 20 $CONTAINER_NAME
else
    echo "❌ DEPLOYMENT FAILED - Checking logs..."
    docker logs $CONTAINER_NAME
    exit 1
fi

