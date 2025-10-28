#!/bin/bash
# ========================================================================
# JARVIS 3.0 - DEPLOYMENT SCRIPT (Git Bash Compatible)
# ========================================================================

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║          JARVIS 3.0 - ENHANCED BOT DEPLOYMENT                   ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

# Step 1: Verify Docker is running
echo "[1/5] Checking Docker status..."
if ! docker info > /dev/null 2>&1; then
    echo "✗ Docker is not running or not responding"
    echo "  Please start Docker Desktop and run this script again"
    exit 1
fi
echo "✓ Docker is running"
echo ""

# Step 2: Clean Docker cache
echo "[2/5] Cleaning Docker build cache..."
docker system prune -f
echo "✓ Cache cleaned"
echo ""

# Step 3: Build images (Git Bash optimized)
echo "[3/5] Building Docker images..."
echo "  → Disabling BuildKit for Git Bash compatibility..."
export DOCKER_BUILDKIT=0
export COMPOSE_DOCKER_CLI_BUILD=0

echo "  → Building base image with legacy builder..."
docker build -t botcore-base:latest -f Dockerfile.fortified .
if [ $? -ne 0 ]; then
    echo "✗ Docker build failed!"
    echo ""
    echo "Troubleshooting:"
    echo "  1. Restart Docker Desktop"
    echo "  2. Ensure you have at least 8GB RAM allocated to Docker"
    echo "  3. Run: docker system prune -a -f"
    echo "  4. Try again"
    exit 1
fi
echo "✓ Base image built successfully"
echo ""

# Step 4: Deploy containers
echo "[4/5] Deploying containers..."
docker-compose -f docker-compose.gitbash.yml up -d
if [ $? -ne 0 ]; then
    echo "✗ Deployment failed!"
    exit 1
fi
echo "✓ Containers deployed"
echo ""

# Step 5: Verify deployment
echo "[5/5] Verifying deployment..."
sleep 5
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║                  DEPLOYMENT COMPLETE                             ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Services Available:"
echo "  • Twitch Bot: http://localhost:5000"
echo "  • YouTube Bot: http://localhost:5001"
echo "  • Prometheus: http://localhost:9090"
echo "  • Grafana: http://localhost:3000"
echo ""
echo "Monitor logs:"
echo "  docker logs -f botcore-twitch"
echo "  docker logs -f botcore-youtube"
echo ""

