#!/bin/bash
set -e

echo "🔍 ENTERPRISE DEPLOYMENT VALIDATION - TWITCH & YOUTUBE"

validate_environment() {
    echo "--- Environment Validation ---"
    echo "✅ Docker Desktop: $(docker --version)"
    echo "✅ Docker running: $(docker info >/dev/null 2>&1 && echo 'YES' || echo 'NO')"
    echo "✅ Available memory: $(free -h 2>/dev/null | grep Mem | awk '{print $2}' || echo 'N/A (Windows)')"
    echo "✅ Available CPUs: $(nproc 2>/dev/null || echo 'N/A')"
}

validate_container() {
    local container=$1
    local platform=$2
    
    echo "--- Validating $platform Container ---"
    
    # Check if container exists and is running
    if ! docker ps | grep -q $container; then
        echo "❌ Container not running: $container"
        return 1
    fi
    
    echo "✅ Container running: $container"
    
    # Check container status
    local status=$(docker inspect $container | grep '"Status":' | head -1)
    if echo "$status" | grep -q "healthy"; then
        echo "✅ Container healthy: $container"
    else
        echo "⚠️  Container status: $status"
    fi
    
    # Check logs for platform-specific initialization
    local logs=$(docker logs $container --tail 50)
    if echo "$logs" | grep -q "$platform"; then
        echo "✅ $platform initialization detected"
    else
        echo "❌ $platform initialization not found in logs"
        return 1
    fi
    
    # Check for auth bypass messages
    if echo "$logs" | grep -q "AUTH BYPASS"; then
        echo "✅ Authentication bypass active"
    else
        echo "⚠️  Auth bypass not confirmed"
    fi
    
    return 0
}

validate_visual_browser() {
    local container=$1
    
    echo "--- Visual Browser Validation ---"
    
    # Check if browser process is running
    if docker exec $container ps aux | grep -q "[c]hromium"; then
        echo "✅ Visual browser process active"
    else
        echo "❌ Visual browser not running"
        return 1
    fi
    
    # Check X11 display server
    if docker exec $container ps aux | grep -q "[X]vfb"; then
        echo "✅ X11 display server running"
    else
        echo "❌ X11 display server not found"
        return 1
    fi
    
    return 0
}

# Main validation
validate_environment

# Validate specific platform containers
PLATFORMS=("youtube" "twitch")
for platform in "${PLATFORMS[@]}"; do
    container="botcore-enterprise-$platform"
    if docker ps | grep -q $container; then
        validate_container $container $platform
        validate_visual_browser $container
        echo "✅ $platform validation successful"
    else
        echo "⚠️  $platform container not deployed"
    fi
    echo ""
done

echo "🎉 ENTERPRISE VALIDATION COMPLETE!"
echo "🚀 System ready for sophisticated viewer emulation"
echo "📊 Quick commands:"
echo "   docker logs -f botcore-enterprise-youtube"
echo "   docker logs -f botcore-enterprise-twitch"
echo "   docker stats botcore-enterprise-*"

