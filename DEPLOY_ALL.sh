#!/bin/bash
set -e

# =====================================================
#  COMPLETE DEPLOYMENT - ALL PLATFORMS + MONITORING
# =====================================================

echo ""
echo "╔════════════════════════════════════════════════════════╗"
echo "║  STREAMING BOT - COMPLETE DEPLOYMENT SUITE            ║"
echo "║  Performance Enhanced - Production Ready              ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check Docker
echo "[1/5] Validating Docker environment..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running! Please start Docker.${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker is running${NC}"
echo ""

# Create directories
echo "[2/5] Creating directory structure..."
mkdir -p logs/twitch logs/youtube data/twitch data/youtube cache/youtube monitoring/grafana/dashboards
echo -e "${GREEN}✅ Directories created${NC}"
echo ""

# Deploy Twitch Bot
echo "[3/5] Deploying Twitch Bot..."
echo "----------------------------------------"
docker stop botcore-twitch 2>/dev/null || true
docker rm botcore-twitch 2>/dev/null || true
docker build -f Dockerfile.twitch -t botcore-twitch:latest . --no-cache

docker run -d \
    --name botcore-twitch \
    --restart unless-stopped \
    --memory=512m \
    --cpus=1.0 \
    -p 5000:5000 \
    -p 8080:8080 \
    -v "$(pwd)/config/appsettings.twitch.json:/app/appsettings.json:ro" \
    -v "$(pwd)/logs/twitch:/app/logs" \
    -v "$(pwd)/data/twitch:/app/data" \
    -e PLATFORM=Twitch \
    botcore-twitch:latest

echo -e "${GREEN}✅ Twitch Bot deployed${NC}"
echo ""

# Deploy YouTube Bot
echo "[4/5] Deploying YouTube Bot..."
echo "----------------------------------------"
docker stop botcore-youtube 2>/dev/null || true
docker rm botcore-youtube 2>/dev/null || true
docker build -f Dockerfile.youtube -t botcore-youtube:latest . --no-cache

docker run -d \
    --name botcore-youtube \
    --restart unless-stopped \
    --memory=1g \
    --cpus=2.0 \
    -p 5001:5001 \
    -p 8081:8081 \
    -v "$(pwd)/config/appsettings.youtube.json:/app/appsettings.json:ro" \
    -v "$(pwd)/logs/youtube:/app/logs" \
    -v "$(pwd)/data/youtube:/app/data" \
    -v "$(pwd)/cache/youtube:/app/cache" \
    -e PLATFORM=YouTube \
    botcore-youtube:latest

echo -e "${GREEN}✅ YouTube Bot deployed${NC}"
echo ""

# Start Monitoring
echo "[5/5] Starting Monitoring Stack..."
echo "----------------------------------------"
docker-compose -f docker-compose.monitoring.yml up -d
echo -e "${GREEN}✅ Monitoring stack started${NC}"
echo ""

# Wait for services
echo "Waiting for services to initialize..."
sleep 10

# Display status
echo ""
echo "╔════════════════════════════════════════════════════════╗"
echo "║            DEPLOYMENT SUCCESSFUL! 🚀                   ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""
echo "📊 RUNNING SERVICES:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "botcore|grafana|prometheus"
echo ""
echo "🌐 ACCESS POINTS:"
echo "  • Grafana Dashboard: http://localhost:3000 (admin/admin)"
echo "  • Prometheus:        http://localhost:9090"
echo "  • Twitch Health:     http://localhost:8080/health"
echo "  • YouTube Health:    http://localhost:8081/health"
echo ""
echo "📋 MONITORING COMMANDS:"
echo "  • Twitch Logs:   docker logs -f botcore-twitch"
echo "  • YouTube Logs:  docker logs -f botcore-youtube"
echo "  • Resources:     docker stats"
echo ""
echo "💡 TIP: Open Grafana to view real-time metrics!"
echo ""

