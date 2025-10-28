# DEPLOY_OVERNIGHT.ps1
# PowerShell deployment script for Watch Time Booster
# Usage: .\DEPLOY_OVERNIGHT.ps1

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   OVERNIGHT WATCH TIME BOOSTER - JARVIS 2.0               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Configuration
$CHANNEL = "timmaythetoolman"
$TOTAL_VIEWERS = 50
$VIEWERS_PER_CONTAINER = 5  # Reduced to prevent resource spikes

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Channel: $CHANNEL"
Write-Host "  Total Viewers: $TOTAL_VIEWERS"
Write-Host "  Distribution: $VIEWERS_PER_CONTAINER viewers per container"
Write-Host "  Mode: Non-Headless (Xvfb virtual display)"
Write-Host "  Auto-Recovery: Enabled"
Write-Host "  Duration: Continuous (until stopped)"
Write-Host ""

# Check Docker
try {
    docker info | Out-Null
    Write-Host "[✓] Docker is running" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Docker Desktop is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again."
    pause
    exit 1
}
Write-Host ""

# Cleanup old containers
Write-Host "[CLEANUP] Removing old watchtime containers..." -ForegroundColor Yellow
$oldContainers = docker ps -aq --filter "name=watchtime-"
if ($oldContainers) {
    $oldContainers | ForEach-Object { docker rm -f $_ 2>&1 | Out-Null }
}
Write-Host "[✓] Cleanup complete" -ForegroundColor Green
Write-Host ""

# Calculate number of containers
$NUM = [Math]::Ceiling($TOTAL_VIEWERS / $VIEWERS_PER_CONTAINER)

Write-Host "[DEPLOY] Launching $NUM containers with $TOTAL_VIEWERS total viewers..." -ForegroundColor Cyan
Write-Host ""

# Deploy containers with staggered launch and resource limits
for ($i = 1; $i -le $NUM; $i++) {
    $START_INDEX = ($i - 1) * $VIEWERS_PER_CONTAINER
    $REMAIN = $TOTAL_VIEWERS - $START_INDEX
    
    if ($REMAIN -lt $VIEWERS_PER_CONTAINER) {
        $VIEWERS = $REMAIN
    } else {
        $VIEWERS = $VIEWERS_PER_CONTAINER
    }
    
    $CONTAINER_NAME = "watchtime-$i"
    Write-Host "[$i/$NUM] Launching $CONTAINER_NAME with $VIEWERS viewers..." -ForegroundColor White
    
    # Create logs directory
    $logDir = Join-Path $PSScriptRoot "logs\$CONTAINER_NAME"
    if (!(Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    # Run container with strict resource limits (CPU: 1 core, RAM: 1.5GB per container)
    # Using --cpus with decimal values for better WSL2 compatibility
    $result = docker run -d --name $CONTAINER_NAME --restart unless-stopped `
        --cpus="1.0" `
        --memory="1536m" `
        --memory-swap="1536m" `
        --memory-reservation="1024m" `
        --cpu-shares=512 `
        --pids-limit=200 `
        -e MODE=WATCHTIME `
        -e CHANNEL_USERNAME=$CHANNEL `
        -e VIEWER_COUNT=$VIEWERS `
        -e HEADLESS=false `
        -e LOW_CPU_RAM=true `
        -v "${logDir}:/app/logs" `
        streamviewerbot:latest 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [✓] $CONTAINER_NAME launched (ID: $($result.Substring(0, 12)))" -ForegroundColor Green
        
        # Wait for container to stabilize before launching next one
        Write-Host "   ⏳ Waiting 15 seconds for container to stabilize..." -ForegroundColor Gray
        Start-Sleep -Seconds 15
        
        # Verify container is still running
        $status = docker ps --filter "name=$CONTAINER_NAME" --format "{{.Status}}" 2>$null
        if ($status) {
            Write-Host "   [✓] $CONTAINER_NAME is healthy: $status" -ForegroundColor Green
        } else {
            Write-Host "   [⚠] $CONTAINER_NAME may have crashed - check logs" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   [ERROR] Failed to launch $CONTAINER_NAME" -ForegroundColor Red
        Write-Host "   $result" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   DEPLOYMENT COMPLETE" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Containers deployed: $NUM" -ForegroundColor White
Write-Host "Total viewers: $TOTAL_VIEWERS" -ForegroundColor White
Write-Host "Channel: $CHANNEL" -ForegroundColor White
Write-Host ""
Write-Host "The system will now run continuously and automatically:" -ForegroundColor Yellow
Write-Host "  • Discover your videos, shorts, and VODs"
Write-Host "  • Distribute viewers across content"
Write-Host "  • Replace failed sessions"
Write-Host "  • Accumulate watch time 24/7"
Write-Host ""
Write-Host "MONITORING:" -ForegroundColor Cyan
Write-Host '  • View all containers: docker ps --filter name=watchtime-'
Write-Host '  • Monitor logs: docker logs -f watchtime-1'
Write-Host '  • Check stats: docker stats --filter name=watchtime-'
Write-Host ""
Write-Host "SHUTDOWN:" -ForegroundColor Cyan
Write-Host '  • Run: .\STOP_WATCHTIME_ALL.bat'
Write-Host '  • Or PowerShell: docker ps -q --filter name=watchtime- | ForEach-Object { docker stop $_ }'
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   GO TO BED. WAKE UP TO BOOSTED METRICS. 🚀" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Show initial status
Write-Host "Waiting 10 seconds for containers to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "Container Status:" -ForegroundColor Cyan
docker ps --filter name=watchtime- --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host ""
Write-Host "Checking first container logs..." -ForegroundColor Cyan
docker logs watchtime-1 2>&1 | Select-Object -Last 20

pause

