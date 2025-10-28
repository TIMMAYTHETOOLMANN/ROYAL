# ========================================================================
# STAGED STANDBY DEPLOYMENT - Pre-Stream Monitor
# Watches for your stream to go live, then auto-deploys viewers
# ========================================================================

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   JARVIS 2.0 - STAGED STANDBY DEPLOYMENT                 ║" -ForegroundColor Cyan
Write-Host "║   Pre-Stream Monitor Mode - Awaiting Live Signal         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Configuration
$COMPOSE_FILE = "docker-compose.standby.yml"
$DOCKERFILE = "Dockerfile.enhanced"
$IMAGE_NAME = "botcore:sophisticated"
$CHANNEL = "timmaythetoolman"
$PLATFORM = "twitch"

Write-Host "`n[INFO] Configuration:" -ForegroundColor Yellow
Write-Host "  • Channel: $CHANNEL" -ForegroundColor White
Write-Host "  • Platform: $PLATFORM" -ForegroundColor White
Write-Host "  • Mode: PRESTREAM (Staged Standby)" -ForegroundColor White
Write-Host "  • Auto-Deploy: 30-50 viewers when live" -ForegroundColor White
Write-Host "  • Check Interval: Every 3 minutes" -ForegroundColor White

# Step 1: Pre-flight checks
Write-Host "`n[STEP 1/7] Running pre-flight checks..." -ForegroundColor Yellow

if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Docker not found. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker installed" -ForegroundColor Green

if (!(Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Docker Compose not found." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker Compose installed" -ForegroundColor Green

# Check if Docker is running
try {
    docker ps > $null 2>&1
    Write-Host "✓ Docker daemon running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker daemon not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Step 2: Clean previous deployment
Write-Host "`n[STEP 2/7] Cleaning previous deployment..." -ForegroundColor Yellow
docker-compose -f $COMPOSE_FILE down --remove-orphans 2>$null
Write-Host "✓ Previous containers removed" -ForegroundColor Green

# Step 3: Create monitoring directory structure
Write-Host "`n[STEP 3/7] Setting up monitoring infrastructure..." -ForegroundColor Yellow
$dirs = @("logs", "config", "monitoring/grafana/dashboards", "monitoring/grafana/datasources")
foreach ($dir in $dirs) {
    New-Item -Path $dir -ItemType Directory -Force | Out-Null
}
Write-Host "✓ Directory structure created" -ForegroundColor Green

# Step 4: Build enhanced image
Write-Host "`n[STEP 4/7] Building sophisticated Docker image..." -ForegroundColor Yellow
Write-Host "This may take 5-10 minutes on first run (downloads .NET 8.0 + Playwright)..." -ForegroundColor Cyan

docker build -f $DOCKERFILE -t $IMAGE_NAME . 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed! Running with verbose output..." -ForegroundColor Red
    docker build -f $DOCKERFILE -t $IMAGE_NAME .
    exit 1
}
Write-Host "✓ Image built successfully" -ForegroundColor Green

# Step 5: Start services in standby mode
Write-Host "`n[STEP 5/7] Deploying staged standby stack..." -ForegroundColor Yellow
docker-compose -f $COMPOSE_FILE up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Deployment failed!" -ForegroundColor Red
    docker-compose -f $COMPOSE_FILE logs
    exit 1
}
Write-Host "✓ All services started in standby mode" -ForegroundColor Green

# Step 6: Health check wait
Write-Host "`n[STEP 6/7] Waiting for services to become healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

$maxRetries = 12
$retryCount = 0
$healthy = $false

while ($retryCount -lt $maxRetries -and -not $healthy) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            Write-Host "✓ BotCore standby monitor is healthy" -ForegroundColor Green
        }
    } catch {
        Write-Host "  Waiting for health check... ($retryCount/$maxRetries)" -ForegroundColor Cyan
        Start-Sleep -Seconds 5
        $retryCount++
    }
}

if (-not $healthy) {
    Write-Host "⚠ Health check timeout. Monitor may still be initializing..." -ForegroundColor Yellow
    Write-Host "  Check logs: docker-compose -f $COMPOSE_FILE logs -f botcore-standby" -ForegroundColor Cyan
}

# Step 7: Display status
Write-Host "`n[STEP 7/7] Deployment Status" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

docker-compose -f $COMPOSE_FILE ps

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   STAGED STANDBY MODE - ACTIVE & MONITORING               ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n🎯 Pre-Stream Monitor Configuration:" -ForegroundColor Cyan
Write-Host "  • Channel:            $CHANNEL" -ForegroundColor White
Write-Host "  • Platform:           $PLATFORM" -ForegroundColor White
Write-Host "  • Status:             ⏳ WAITING FOR LIVE SIGNAL" -ForegroundColor Yellow
Write-Host "  • Check Interval:     Every 3 minutes" -ForegroundColor White
Write-Host "  • Auto-Deploy:        30-50 viewers (with fluctuation)" -ForegroundColor White
Write-Host "  • Chat Engagement:    25% active (when live)" -ForegroundColor White

Write-Host "`n📊 Monitoring Dashboards:" -ForegroundColor Cyan
Write-Host "  • BotCore Health:     http://localhost:5000/health" -ForegroundColor White
Write-Host "  • Live Logs:          docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor White
Write-Host "  • Prometheus:         http://localhost:9090" -ForegroundColor White
Write-Host "  • Grafana:            http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "  • RabbitMQ:           http://localhost:15672 (admin/admin)" -ForegroundColor White
Write-Host "  • Redis:              localhost:6379" -ForegroundColor White

Write-Host "`n🎬 What Happens Next:" -ForegroundColor Cyan
Write-Host "  1. Monitor checks https://www.twitch.tv/$CHANNEL every 3 minutes" -ForegroundColor White
Write-Host "  2. When stream goes LIVE:" -ForegroundColor White
Write-Host "     → Instantly deploys 30-50 viewers" -ForegroundColor White
Write-Host "     → Activates chat engagement (25% participation)" -ForegroundColor White
Write-Host "     → Simulates realistic viewer fluctuation" -ForegroundColor White
Write-Host "     → Maintains presence throughout stream" -ForegroundColor White

Write-Host "`n⚙️ Management Commands:" -ForegroundColor Cyan
Write-Host "  • View live logs:     docker-compose -f $COMPOSE_FILE logs -f botcore-standby" -ForegroundColor White
Write-Host "  • Check status:       docker-compose -f $COMPOSE_FILE ps" -ForegroundColor White
Write-Host "  • Stop standby:       docker-compose -f $COMPOSE_FILE down" -ForegroundColor White
Write-Host "  • Restart monitor:    docker-compose -f $COMPOSE_FILE restart botcore-standby" -ForegroundColor White

Write-Host "`n🔔 Live Stream Detection:" -ForegroundColor Cyan
Write-Host "  The monitor will automatically detect when you go live on Twitch." -ForegroundColor White
Write-Host "  No manual intervention required - just start streaming!" -ForegroundColor White

Write-Host "`n✅ System Status: STANDBY MODE ACTIVE" -ForegroundColor Green
Write-Host "✅ Monitoring: https://www.twitch.tv/$CHANNEL" -ForegroundColor Green
Write-Host "✅ Ready to auto-deploy when stream goes live!" -ForegroundColor Green

Write-Host "`n💡 Pro Tip:" -ForegroundColor Yellow
Write-Host "  Open a terminal and run: docker-compose -f $COMPOSE_FILE logs -f botcore-standby" -ForegroundColor White
Write-Host "  This will show you real-time monitoring updates." -ForegroundColor White

Write-Host "`n🚀 JARVIS 2.0 is now watching your channel. Good luck with your stream!" -ForegroundColor Magenta

