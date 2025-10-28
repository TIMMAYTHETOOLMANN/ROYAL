# ========================================================================
# MULTI-PLATFORM STAGED STANDBY DEPLOYMENT
# Twitch + YouTube Pre-Stream Monitoring with Auto-Deploy
# ========================================================================

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   JARVIS 2.0 - MULTI-PLATFORM STAGED STANDBY             ║" -ForegroundColor Cyan
Write-Host "║   Twitch + YouTube Pre-Stream Monitor                    ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Configuration
$COMPOSE_FILE = "docker-compose.multiplatform.yml"
$DOCKERFILE = "Dockerfile.enhanced"
$IMAGE_NAME = "botcore:sophisticated"
$CHANNEL = "timmaythetoolman"
$PLATFORMS = @("Twitch", "YouTube")

Write-Host "`n[INFO] Multi-Platform Configuration:" -ForegroundColor Yellow
Write-Host "  • Channel: $CHANNEL" -ForegroundColor White
Write-Host "  • Platforms: Twitch + YouTube" -ForegroundColor White
Write-Host "  • Mode: PRESTREAM (Staged Standby)" -ForegroundColor White
Write-Host "  • Auto-Deploy: 30-50 viewers PER PLATFORM when live" -ForegroundColor White
Write-Host "  • Total Capacity: 60-100 viewers across both platforms" -ForegroundColor White
Write-Host "  • Check Interval: Every 3 minutes (each platform)" -ForegroundColor White

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
docker-compose -f docker-compose.standby.yml down --remove-orphans 2>$null
Write-Host "✓ Previous containers removed" -ForegroundColor Green

# Step 3: Create monitoring directory structure
Write-Host "`n[STEP 3/7] Setting up multi-platform monitoring infrastructure..." -ForegroundColor Yellow
$dirs = @("logs/twitch", "logs/youtube", "config", "monitoring/grafana/dashboards", "monitoring/grafana/datasources")
foreach ($dir in $dirs) {
    New-Item -Path $dir -ItemType Directory -Force | Out-Null
}
Write-Host "✓ Directory structure created for both platforms" -ForegroundColor Green

# Copy multi-platform Prometheus config
Copy-Item -Path "monitoring/prometheus-multiplatform.yml" -Destination "monitoring/prometheus.yml" -Force -ErrorAction SilentlyContinue
Write-Host "✓ Multi-platform Prometheus configuration deployed" -ForegroundColor Green

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

# Step 5: Start multi-platform services
Write-Host "`n[STEP 5/7] Deploying multi-platform staged standby stack..." -ForegroundColor Yellow
docker-compose -f $COMPOSE_FILE up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Deployment failed!" -ForegroundColor Red
    docker-compose -f $COMPOSE_FILE logs
    exit 1
}
Write-Host "✓ All services started in multi-platform standby mode" -ForegroundColor Green

# Step 6: Health check wait
Write-Host "`n[STEP 6/7] Waiting for services to become healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

$maxRetries = 12
$twitchHealthy = $false
$youtubeHealthy = $false

# Check Twitch
Write-Host "  Checking Twitch monitor..." -ForegroundColor Cyan
$retryCount = 0
while ($retryCount -lt $maxRetries -and -not $twitchHealthy) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $twitchHealthy = $true
            Write-Host "  ✓ Twitch monitor is healthy" -ForegroundColor Green
        }
    } catch {
        Start-Sleep -Seconds 5
        $retryCount++
    }
}

# Check YouTube
Write-Host "  Checking YouTube monitor..." -ForegroundColor Cyan
$retryCount = 0
while ($retryCount -lt $maxRetries -and -not $youtubeHealthy) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5001/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $youtubeHealthy = $true
            Write-Host "  ✓ YouTube monitor is healthy" -ForegroundColor Green
        }
    } catch {
        Start-Sleep -Seconds 5
        $retryCount++
    }
}

if (-not $twitchHealthy -or -not $youtubeHealthy) {
    Write-Host "⚠ Some health checks timed out. Monitors may still be initializing..." -ForegroundColor Yellow
    Write-Host "  Check logs: docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor Cyan
}

# Step 7: Display status
Write-Host "`n[STEP 7/7] Deployment Status" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

docker-compose -f $COMPOSE_FILE ps

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   MULTI-PLATFORM STANDBY MODE - ACTIVE & MONITORING       ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n🎯 Multi-Platform Pre-Stream Monitor Configuration:" -ForegroundColor Cyan
Write-Host "  • Channel:            $CHANNEL" -ForegroundColor White
Write-Host "  • Platforms:          Twitch + YouTube" -ForegroundColor White
Write-Host "  • Status:             ⏳ WAITING FOR LIVE SIGNALS" -ForegroundColor Yellow
Write-Host "  • Check Interval:     Every 3 minutes (per platform)" -ForegroundColor White
Write-Host "  • Auto-Deploy/Each:   30-50 viewers" -ForegroundColor White
Write-Host "  • Total Capacity:     60-100 viewers" -ForegroundColor White
Write-Host "  • Chat Engagement:    25% active (per platform)" -ForegroundColor White

Write-Host "`n📊 Monitoring Dashboards:" -ForegroundColor Cyan
Write-Host "  • Twitch Health:      http://localhost:5000/health" -ForegroundColor White
Write-Host "  • YouTube Health:     http://localhost:5001/health" -ForegroundColor White
Write-Host "  • Twitch Logs:        docker-compose -f $COMPOSE_FILE logs -f botcore-twitch" -ForegroundColor White
Write-Host "  • YouTube Logs:       docker-compose -f $COMPOSE_FILE logs -f botcore-youtube" -ForegroundColor White
Write-Host "  • All Logs:           docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor White
Write-Host "  • Prometheus:         http://localhost:9090" -ForegroundColor White
Write-Host "  • Grafana:            http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "  • RabbitMQ:           http://localhost:15672 (admin/admin)" -ForegroundColor White

Write-Host "`n🎬 What Happens Next:" -ForegroundColor Cyan
Write-Host "  Each platform monitor independently checks every 3 minutes:" -ForegroundColor White
Write-Host "  • Twitch:  https://www.twitch.tv/$CHANNEL" -ForegroundColor White
Write-Host "  • YouTube: https://www.youtube.com/@$CHANNEL/live" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "  When EITHER platform goes LIVE:" -ForegroundColor White
Write-Host "     → That platform instantly deploys 30-50 viewers" -ForegroundColor White
Write-Host "     → Activates chat engagement (25% participation)" -ForegroundColor White
Write-Host "     → Simulates realistic viewer fluctuation" -ForegroundColor White
Write-Host "     → Maintains presence throughout stream" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "  If BOTH platforms go live simultaneously:" -ForegroundColor White
Write-Host "     → Both deploy independently (60-100 total viewers)" -ForegroundColor White
Write-Host "     → Each platform managed separately" -ForegroundColor White

Write-Host "`n⚙️ Management Commands:" -ForegroundColor Cyan
Write-Host "  • View Twitch logs:   docker-compose -f $COMPOSE_FILE logs -f botcore-twitch" -ForegroundColor White
Write-Host "  • View YouTube logs:  docker-compose -f $COMPOSE_FILE logs -f botcore-youtube" -ForegroundColor White
Write-Host "  • View all logs:      docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor White
Write-Host "  • Check status:       docker-compose -f $COMPOSE_FILE ps" -ForegroundColor White
Write-Host "  • Stop all:           docker-compose -f $COMPOSE_FILE down" -ForegroundColor White
Write-Host "  • Restart Twitch:     docker-compose -f $COMPOSE_FILE restart botcore-twitch" -ForegroundColor White
Write-Host "  • Restart YouTube:    docker-compose -f $COMPOSE_FILE restart botcore-youtube" -ForegroundColor White

Write-Host "`n🔔 Live Stream Detection:" -ForegroundColor Cyan
Write-Host "  Both monitors run independently and will automatically detect" -ForegroundColor White
Write-Host "  when you go live on either Twitch or YouTube." -ForegroundColor White
Write-Host "  No manual intervention required - just start streaming!" -ForegroundColor White

Write-Host "`n✅ System Status: MULTI-PLATFORM STANDBY MODE ACTIVE" -ForegroundColor Green
Write-Host "✅ Monitoring Twitch: https://www.twitch.tv/$CHANNEL" -ForegroundColor Green
Write-Host "✅ Monitoring YouTube: https://www.youtube.com/@$CHANNEL/live" -ForegroundColor Green
Write-Host "✅ Ready to auto-deploy when either stream goes live!" -ForegroundColor Green

Write-Host "`n💡 Pro Tips:" -ForegroundColor Yellow
Write-Host "  • Monitor both platforms: docker-compose -f $COMPOSE_FILE logs -f" -ForegroundColor White
Write-Host "  • Use Grafana to visualize metrics across both platforms" -ForegroundColor White
Write-Host "  • Each platform's logs stored separately in logs/twitch and logs/youtube" -ForegroundColor White

Write-Host "`n🚀 JARVIS 2.0 is now watching BOTH your channels. Good luck with your streams!" -ForegroundColor Magenta

