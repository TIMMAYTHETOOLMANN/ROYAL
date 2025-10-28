# DIAGNOSE_WATCHTIME.ps1
# Diagnostic script to check watchtime container health

Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  WATCHTIME BOOSTER DIAGNOSTIC" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check container status
Write-Host "[1/5] Container Status Check..." -ForegroundColor Yellow
$containers = docker ps -a --filter "name=watchtime-" --format "{{.Names}}|{{.Status}}|{{.State}}" 2>$null

if ($containers) {
    foreach ($container in $containers) {
        $parts = $container -split '\|'
        Write-Host "  $($parts[0]): $($parts[1])" -ForegroundColor White
    }
} else {
    Write-Host "  [WARNING] No watchtime containers found!" -ForegroundColor Red
}
Write-Host ""

# Check container logs for errors
Write-Host "[2/5] Checking logs for watchtime-1..." -ForegroundColor Yellow
$logs = docker logs watchtime-1 2>&1 | Out-String
if ($logs -and $logs.Trim().Length -gt 0) {
    Write-Host $logs.Substring(0, [Math]::Min(1000, $logs.Length))
} else {
    Write-Host "  [WARNING] No logs available from watchtime-1" -ForegroundColor Red
    Write-Host "  This could mean:" -ForegroundColor Yellow
    Write-Host "    - Container is stuck during startup" -ForegroundColor Yellow
    Write-Host "    - Xvfb failed to start" -ForegroundColor Yellow
    Write-Host "    - Entrypoint script has an error" -ForegroundColor Yellow
}
Write-Host ""

# Check running processes
Write-Host "[3/5] Checking running processes..." -ForegroundColor Yellow
try {
    $processes = docker top watchtime-1 2>&1 | Out-String
    if ($processes -match "Xvfb" -or $processes -match "dotnet") {
        Write-Host "  [✓] Processes detected:" -ForegroundColor Green
        Write-Host $processes.Substring(0, [Math]::Min(500, $processes.Length))
    } else {
        Write-Host "  [WARNING] Cannot read process list" -ForegroundColor Red
    }
} catch {
    Write-Host "  [ERROR] Cannot access container processes" -ForegroundColor Red
}
Write-Host ""

# Check if we can exec into container
Write-Host "[4/5] Testing container shell access..." -ForegroundColor Yellow
$testExec = docker exec watchtime-1 echo "test" 2>&1
if ($testExec -match "test") {
    Write-Host "  [✓] Container shell access working" -ForegroundColor Green
    
    # Check if Xvfb is running
    $xvfbCheck = docker exec watchtime-1 pgrep -f "Xvfb" 2>&1
    if ($xvfbCheck -match "\d+") {
        Write-Host "  [✓] Xvfb virtual display is running (PID: $xvfbCheck)" -ForegroundColor Green
    } else {
        Write-Host "  [✗] Xvfb is NOT running - this is the problem!" -ForegroundColor Red
    }
    
    # Check if dotnet is running
    $dotnetCheck = docker exec watchtime-1 pgrep -f "dotnet" 2>&1
    if ($dotnetCheck -match "\d+") {
        Write-Host "  [✓] Bot process is running (PID: $dotnetCheck)" -ForegroundColor Green
    } else {
        Write-Host "  [✗] Bot process is NOT running!" -ForegroundColor Red
    }
} else {
    Write-Host "  [✗] Cannot execute commands in container" -ForegroundColor Red
}
Write-Host ""

# Resource usage
Write-Host "[5/5] Resource Usage..." -ForegroundColor Yellow
$stats = docker stats --no-stream --format "{{.Name}}: CPU={{.CPUPerc}} MEM={{.MemUsage}}" watchtime-1 watchtime-2 watchtime-3 watchtime-4 watchtime-5 2>$null
if ($stats) {
    foreach ($stat in $stats) {
        Write-Host "  $stat" -ForegroundColor White
    }
} else {
    Write-Host "  [WARNING] Cannot read container stats" -ForegroundColor Red
}
Write-Host ""

Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DIAGNOSIS COMPLETE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "RECOMMENDATION:" -ForegroundColor Yellow
Write-Host "If Xvfb or bot process is not running, the startup script may have failed." -ForegroundColor White
Write-Host "Try rebuilding the image and redeploying:" -ForegroundColor White
Write-Host "  1. docker build -t streamviewerbot:latest ." -ForegroundColor Cyan
Write-Host "  2. .\STOP_WATCHTIME_ALL.bat" -ForegroundColor Cyan
Write-Host "  3. .\DEPLOY_OVERNIGHT.ps1" -ForegroundColor Cyan
Write-Host ""

pause

