# CLEAN_AND_REDEPLOY.ps1
# Complete cleanup and fresh deployment after Docker restart

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   CLEAN & REDEPLOY - WATCHTIME BOOSTER                     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Wait for Docker to be ready
Write-Host "[1/5] Waiting for Docker Desktop to be ready..." -ForegroundColor Yellow
$retries = 0
$maxRetries = 60
while ($retries -lt $maxRetries) {
    try {
        $null = docker info 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   [✓] Docker is ready!" -ForegroundColor Green
            break
        }
    } catch { }
    
    if ($retries % 10 -eq 0) {
        Write-Host "   Waiting... ($retries seconds)" -ForegroundColor Gray
    }
    Start-Sleep -Seconds 1
    $retries++
}

if ($retries -eq $maxRetries) {
    Write-Host "   [✗] Docker failed to start after $maxRetries seconds" -ForegroundColor Red
    Write-Host "   Please start Docker Desktop manually and run this script again." -ForegroundColor Yellow
    pause
    exit 1
}
Write-Host ""

I. # AGGRESSIVE cleanup - force kill all Docker processes and containers
Write-Host "[2/5] AGGRESSIVE cleanup - force-killing all processes..." -ForegroundColor Yellow

# Step 1: Force stop ALL containers (not just watchtime)
Write-Host "   [1/3] Force stopping ALL Docker containers..." -ForegroundColor Gray
$allRunning = docker ps -q 2>$null
if ($allRunning) {
    Write-Host "       Found $($allRunning.Count) running containers - force stopping..." -ForegroundColor Gray
    docker stop $allRunning 2>&1 | Out-Null
    Start-Sleep -Seconds 2
}

# Step 2: Force remove ALL watchtime containers (even corrupted ones)
Write-Host "   [2/3] Force removing watchtime containers..." -ForegroundColor Gray
$allContainers = docker ps -aq --filter "name=watchtime-" 2>$null
if ($allContainers) {
    Write-Host "       Found $($allContainers.Count) watchtime containers - force removing..." -ForegroundColor Gray
    foreach ($container in $allContainers) {
        docker rm -f $container 2>&1 | Out-Null
    }
}

# Step 3: Nuclear option - prune EVERYTHING
Write-Host "   [3/3] Pruning all stopped containers and networks..." -ForegroundColor Gray
docker container prune -f 2>&1 | Out-Null
docker network prune -f 2>&1 | Out-Null

# Step 4: Kill any lingering Chrome/Chromium processes from previous runs
Write-Host "   [BONUS] Killing any lingering Chrome/Chromium processes..." -ForegroundColor Gray
$chromeProcesses = Get-Process | Where-Object { $_.ProcessName -like "*chrome*" -or $_.ProcessName -like "*chromium*" } -ErrorAction SilentlyContinue
if ($chromeProcesses) {
    Write-Host "       Found $($chromeProcesses.Count) Chrome processes - terminating..." -ForegroundColor Gray
    $chromeProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host "   [✓] AGGRESSIVE cleanup complete - system is clean" -ForegroundColor Green
Write-Host ""

# Check if image needs rebuild
Write-Host "[3/5] Checking Docker image..." -ForegroundColor Yellow
$imageExists = docker images streamviewerbot:latest -q 2>$null
if ($imageExists) {
    $imageDate = docker images streamviewerbot:latest --format "{{.CreatedAt}}" 2>$null
    Write-Host "   [✓] Image exists: $imageDate" -ForegroundColor Green
    Write-Host "   Checking if rebuild is needed..." -ForegroundColor Gray
    
    # Check if Dockerfile was modified more recently than image
    $dockerfilePath = Join-Path $PSScriptRoot "Dockerfile"
    if (Test-Path $dockerfilePath) {
        $dockerfileTime = (Get-Item $dockerfilePath).LastWriteTime
        Write-Host "   Dockerfile modified: $dockerfileTime" -ForegroundColor Gray
        
        # Always rebuild to ensure we have the fixed startup script
        Write-Host "   [!] Rebuilding to ensure latest fixes..." -ForegroundColor Yellow
        docker build -t streamviewerbot:latest . 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   [✓] Image rebuilt successfully!" -ForegroundColor Green
        } else {
            Write-Host "   [✗] Build failed! Check errors above." -ForegroundColor Red
            pause
            exit 1
        }
    }
} else {
    Write-Host "   [!] Image not found, building..." -ForegroundColor Yellow
    docker build -t streamviewerbot:latest . 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [✓] Image built successfully!" -ForegroundColor Green
    } else {
        Write-Host "   [✗] Build failed! Check errors above." -ForegroundColor Red
        pause
        exit 1
    }
}
Write-Host ""

# Deploy a single test container first
Write-Host "[4/5] Deploying test container (5 viewers)..." -ForegroundColor Yellow
$testResult = docker run -d --name watchtime-test `
    -e MODE=WATCHTIME `
    -e CHANNEL_USERNAME=timmaythetoolman `
    -e VIEWER_COUNT=5 `
    -e HEADLESS=false `
    -e LOW_CPU_RAM=true `
    streamviewerbot:latest 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   [✓] Test container launched: $testResult" -ForegroundColor Green
    Write-Host "   Waiting 15 seconds for startup..." -ForegroundColor Gray
    Start-Sleep -Seconds 15
    
    # Check if test container is running
    $testStatus = docker ps --filter "name=watchtime-test" --format "{{.Status}}" 2>$null
    if ($testStatus) {
        Write-Host "   [✓] Test container is running: $testStatus" -ForegroundColor Green
        
        # Check logs for success indicators
        $testLogs = docker logs watchtime-test 2>&1 | Out-String
        if ($testLogs -match "ADAPTIVE WATCH TIME BOOSTER" -or $testLogs -match "viewer" -or $testLogs -match "Discovering") {
            Write-Host "   [✓] Bot is starting up correctly!" -ForegroundColor Green
            Write-Host ""
            Write-Host "   Sample logs:" -ForegroundColor Gray
            $testLogs.Split("`n") | Select-Object -First 10 | ForEach-Object {
                Write-Host "     $_" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "   [⚠] Logs are empty or unexpected" -ForegroundColor Yellow
            Write-Host "   This might be normal during early startup" -ForegroundColor Gray
        }
    } else {
        Write-Host "   [✗] Test container is not running!" -ForegroundColor Red
        Write-Host "   Checking logs for errors..." -ForegroundColor Yellow
        docker logs watchtime-test 2>&1
        pause
        exit 1
    }
} else {
    Write-Host "   [✗] Failed to launch test container!" -ForegroundColor Red
    Write-Host "   Error: $testResult" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Ask user if they want to proceed with full deployment
Write-Host "[5/5] Ready to deploy full cluster (50 viewers)" -ForegroundColor Yellow
Write-Host ""
$response = Read-Host "Deploy full cluster? (y/n)"
if ($response -eq "y" -or $response -eq "Y") {
    Write-Host ""
    Write-Host "Stopping test container..." -ForegroundColor Gray
    docker stop watchtime-test 2>&1 | Out-Null
    docker rm watchtime-test 2>&1 | Out-Null
    
    Write-Host "Launching full deployment..." -ForegroundColor Cyan
    Write-Host ""
    & "$PSScriptRoot\DEPLOY_OVERNIGHT.ps1"
} else {
    Write-Host ""
    Write-Host "Test container is still running. Monitor with:" -ForegroundColor Yellow
    Write-Host "  docker logs -f watchtime-test" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To stop test:" -ForegroundColor Yellow
    Write-Host "  docker stop watchtime-test && docker rm watchtime-test" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   DEPLOYMENT READY                                         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

