#!/bin/bash
set -e

echo "========================================="
echo "🚀 DEPLOYING ENHANCED YOUTUBE BOT"
echo "========================================="

# Cleanup previous deployment
echo "🧹 Cleaning previous deployment..."
docker stop botcore-youtube 2>/dev/null || true
docker rm botcore-youtube 2>/dev/null || true

# Build optimized YouTube image
echo "🔨 Building optimized YouTube image..."
docker build -f Dockerfile.youtube -t botcore-youtube:latest . --no-cache

# Create logs directory
mkdir -p logs/youtube data/youtube cache/youtube

# Deploy with enhanced resource allocation for video processing
echo "🚢 Deploying YouTube bot with performance optimizations..."
docker run -d \
    --name botcore-youtube \
    --restart unless-stopped \
    --memory=1g \
    --memory-reservation=512m \
    --cpus=2.0 \
    --cpu-shares=2048 \
    -p 5001:5001 \
    -p 8081:8081 \
    -v "$(pwd)/config/appsettings.youtube.json:/app/appsettings.json:ro" \
    -v "$(pwd)/logs/youtube:/app/logs" \
    -v "$(pwd)/data/youtube:/app/data" \
    -v "$(pwd)/cache/youtube:/app/cache" \
    -e PLATFORM=YouTube \
    -e DOTNET_gcServer=1 \
    -e DOTNET_GCHeapCount=4 \
    -e MOZ_HEADLESS=1 \
    botcore-youtube:latest

echo ""
echo "✅ YouTube Bot deployed successfully!"
echo "========================================="
echo "📊 Monitoring Commands:"
echo "   Logs:    docker logs -f botcore-youtube"
echo "   Stats:   docker stats botcore-youtube"
echo "   Health:  curl http://localhost:8081/health"
echo "========================================="
echo ""

# Wait for startup
sleep 5

# Check if container is running
if docker ps | grep -q botcore-youtube; then
    echo "✅ Container is running"
    docker logs --tail 20 botcore-youtube
else
    echo "❌ Container failed to start"
    docker logs botcore-youtube
    exit 1
fi

