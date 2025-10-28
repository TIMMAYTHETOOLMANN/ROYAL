# ========================================================================
# SOPHISTICATED DOCKER DEPLOYMENT SCRIPT
# Enterprise-grade deployment with health checks and monitoring
# ========================================================================

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   JARVIS 2.0 - SOPHISTICATED DOCKER DEPLOYMENT            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Configuration
$COMPOSE_FILE = "docker-compose.sophisticated.yml"
$DOCKERFILE = "Dockerfile.enhanced"
$IMAGE_NAME = "botcore:sophisticated"

# Step 1: Pre-flight checks
Write-Host "`n[STEP 1/6] Running pre-flight checks..." -ForegroundColor Yellow

# Check Docker
if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Docker not found. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker installed" -ForegroundColor Green

# Check Docker Compose
if (!(Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Docker Compose not found." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker Compose installed" -ForegroundColor Green

# Step 2: Clean previous deployment
Write-Host "`n[STEP 2/6] Cleaning previous deployment..." -ForegroundColor Yellow
docker-compose -f $COMPOSE_FILE down --remove-orphans 2>$null
Write-Host "✓ Previous containers removed" -ForegroundColor Green

# Step 3: Build enhanced image
Write-Host "`n[STEP 3/6] Building sophisticated Docker image..." -ForegroundColor Yellow
Write-Host "This may take 5-10 minutes on first run..." -ForegroundColor Cyan

docker build -f $DOCKERFILE -t $IMAGE_NAME . 

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Image built successfully" -ForegroundColor Green

# Step 4: Start services
Write-Host "`n[STEP 4/6] Starting sophisticated deployment..." -ForegroundColor Yellow
docker-compose -f $COMPOSE_FILE up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Deployment failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ All services started" -ForegroundColor Green

# Step 5: Health check wait
Write-Host "`n[STEP 5/6] Waiting for services to become healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

$maxRetries = 12
$retryCount = 0
$healthy = $false

while ($retryCount -lt $maxRetries -and -not $healthy) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            Write-Host "✓ BotCore is healthy" -ForegroundColor Green
        }
    } catch {
        Write-Host "  Waiting for health check... ($retryCount/$maxRetries)" -ForegroundColor Cyan
        Start-Sleep -Seconds 5
        $retryCount++
    }
}

if (-not $healthy) {
    Write-Host "✗ Health check timeout. Check logs: docker-compose -f $COMPOSE_FILE logs" -ForegroundColor Red
}

# Step 6: Display status
Write-Host "`n[STEP 6/6] Deployment Status" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan

docker-compose -f $COMPOSE_FILE ps

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   DEPLOYMENT COMPLETE - ENTERPRISE MODE ACTIVE            ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`nAccess Points:" -ForegroundColor Cyan
Write-Host "  • BotCore Health:     http://localhost:5000/health" -ForegroundColor White
Write-Host "  • Prometheus:         http://localhost:9090" -ForegroundColor White
Write-Host "  • Grafana:            http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "  • RabbitMQ:           http://localhost:15672 (admin/admin)" -ForegroundColor White
Write-Host "  • Seq Logs:           http://localhost:5341" -ForegroundColor White
Write-Host "  • Redis:              localhost:6379" -ForegroundColor White

Write-Host "`nManagement Commands:" -ForegroundColor Cyan
Write-Host "  • View logs:          docker-compose -f $COMPOSE_FILE logs -f botcore" -ForegroundColor White
Write-Host "  • Scale bots:         docker-compose -f $COMPOSE_FILE up -d --scale botcore=3" -ForegroundColor White
Write-Host "  • Stop deployment:    docker-compose -f $COMPOSE_FILE down" -ForegroundColor White
Write-Host "  • Restart service:    docker-compose -f $COMPOSE_FILE restart botcore" -ForegroundColor White

Write-Host "`n✓ All systems operational. JARVIS 2.0 ready for action." -ForegroundColor Green

