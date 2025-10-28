#!/bin/bash
# ========================================================================
# MONITORING SCRIPT - REAL-TIME CONTAINER STATUS
# ========================================================================

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║            STREAM VIEWER BOT - SYSTEM MONITOR                    ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

# Container Status
echo "=== CONTAINER STATUS ==="
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "(NAMES|botcore|prometheus|grafana)"
echo ""

# Resource Usage
echo "=== RESOURCE USAGE ==="
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}" | grep -E "(NAME|botcore|prometheus|grafana)"
echo ""

# Health Check Status
echo "=== HEALTH CHECK RESULTS ==="
echo "Twitch Bot:"
curl -s -o /dev/null -w "  Status: %{http_code}\n" http://localhost:8080/health 2>/dev/null || echo "  Status: UNREACHABLE"

echo "YouTube Bot:"
curl -s -o /dev/null -w "  Status: %{http_code}\n" http://localhost:8081/health 2>/dev/null || echo "  Status: UNREACHABLE"
echo ""

# Recent Logs
echo "=== RECENT TWITCH BOT LOGS (Last 10 lines) ==="
docker logs botcore-twitch --tail 10 2>/dev/null || echo "Container not running"
echo ""

echo "=== RECENT YOUTUBE BOT LOGS (Last 10 lines) ==="
docker logs botcore-youtube --tail 10 2>/dev/null || echo "Container not running"
echo ""

# Check for common errors
echo "=== ERROR DETECTION ==="
TWITCH_ERRORS=$(docker logs botcore-twitch 2>&1 | grep -i "error\|exception\|fatal" | wc -l)
YOUTUBE_ERRORS=$(docker logs botcore-youtube 2>&1 | grep -i "error\|exception\|fatal" | wc -l)

echo "Twitch Bot Errors: $TWITCH_ERRORS"
echo "YouTube Bot Errors: $YOUTUBE_ERRORS"
echo ""

if [ "$TWITCH_ERRORS" -gt 0 ] || [ "$YOUTUBE_ERRORS" -gt 0 ]; then
    echo "⚠️  ERRORS DETECTED - Review logs with:"
    echo "  docker logs botcore-twitch --tail 50"
    echo "  docker logs botcore-youtube --tail 50"
fi

echo ""
echo "Continuous monitoring: watch -n 5 ./monitor.sh"

