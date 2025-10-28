# ========================================================================
# LIVE DEPLOYMENT MONITOR - JARVIS 3.0
# Real-time deployment monitoring and diagnostics
# ========================================================================

param(
    [string]$ComposeFile = "docker-compose.fortified.yml",
    [int]$MonitorDurationMinutes = 30
)

$startTime = Get-Date
$endTime = $startTime.AddMinutes($MonitorDurationMinutes)

Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           JARVIS 3.0 - LIVE DEPLOYMENT MONITOR                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

function Get-Timestamp {
    return (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
}

function Write-StatusLine {
    param([string]$Message, [string]$Color = "White")
    Write-Host "[$(Get-Timestamp)] $Message" -ForegroundColor $Color
}

function Check-ContainerHealth {
    param([string]$ContainerName)
    
    try {
        $health = docker inspect --format='{{.State.Health.Status}}' $ContainerName 2>$null
        if ($LASTEXITCODE -ne 0) {
            return @{ Status = "not_found"; Color = "Red" }
        }
        
        $state = docker inspect --format='{{.State.Status}}' $ContainerName 2>$null
        
        if ($state -eq "running") {
            if ($health -eq "healthy") {
                return @{ Status = "✓ healthy"; Color = "Green" }
            } elseif ($health -eq "starting") {
                return @{ Status = "⏳ starting"; Color = "Yellow" }
            } elseif ($health -eq "unhealthy") {
                return @{ Status = "✗ unhealthy"; Color = "Red" }
            } else {
                return @{ Status = "⚪ running"; Color = "Cyan" }
            }
        } else {
            return @{ Status = "✗ $state"; Color = "Red" }
        }
    } catch {
        return @{ Status = "✗ error"; Color = "Red" }
    }
}

function Get-ContainerLogs {
    param([string]$ContainerName, [int]$Lines = 10)
    
    try {
        $logs = docker logs --tail $Lines $ContainerName 2>&1
        return $logs
    } catch {
        return "Unable to retrieve logs"
    }
}

function Test-Endpoint {
    param([string]$Url)
    
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -ErrorAction Stop
        return @{ Available = $true; Status = $response.StatusCode; Color = "Green" }
    } catch {
        return @{ Available = $false; Status = "unreachable"; Color = "Red" }
    }
}

Write-StatusLine "🚀 Starting deployment monitor..." "Cyan"
Write-StatusLine "📊 Monitor duration: $MonitorDurationMinutes minutes" "White"
Write-StatusLine "🔄 Checking every 30 seconds" "White"
Write-Host ""

$containers = @(
    @{ Name = "botcore-twitch"; Port = 5000 },
    @{ Name = "botcore-youtube"; Port = 5001 },
    @{ Name = "prometheus-multiplatform"; Port = 9090 },
    @{ Name = "grafana-multiplatform"; Port = 3000 },
    @{ Name = "redis-botcore"; Port = 6379 }
)

$issuesDetected = @()
$checkCount = 0

while ((Get-Date) -lt $endTime) {
    $checkCount++
    Clear-Host
    
    Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║           LIVE DEPLOYMENT MONITOR - CHECK #$checkCount" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "⏰ Current Time: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor White
    Write-Host "⏱️  Monitoring until: $($endTime.ToString('HH:mm:ss'))" -ForegroundColor White
    Write-Host ""
    
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "CONTAINER HEALTH STATUS" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    
    $allHealthy = $true
    foreach ($container in $containers) {
        $health = Check-ContainerHealth -ContainerName $container.Name
        $status = $health.Status
        $color = $health.Color
        
        Write-Host "  $($container.Name.PadRight(30)) " -NoNewline
        Write-Host "$status" -ForegroundColor $color
        
        if ($color -eq "Red") {
            $allHealthy = $false
            $issue = "[$(Get-Timestamp)] Container $($container.Name) is $status"
            if ($issuesDetected -notcontains $issue) {
                $issuesDetected += $issue
            }
        }
    }
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "ENDPOINT AVAILABILITY" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    
    $endpoints = @(
        @{ Name = "Twitch Health"; Url = "http://localhost:5000/health" },
        @{ Name = "YouTube Health"; Url = "http://localhost:5001/health" },
        @{ Name = "Prometheus"; Url = "http://localhost:9090" },
        @{ Name = "Grafana"; Url = "http://localhost:3000" }
    )
    
    foreach ($endpoint in $endpoints) {
        $test = Test-Endpoint -Url $endpoint.Url
        Write-Host "  $($endpoint.Name.PadRight(30)) " -NoNewline
        if ($test.Available) {
            Write-Host "✓ Available (HTTP $($test.Status))" -ForegroundColor $test.Color
        } else {
            Write-Host "✗ $($test.Status)" -ForegroundColor $test.Color
        }
    }
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "STREAM MONITORING LOGS (Last 5 lines)" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    
    # Twitch logs
    Write-Host "🟣 TWITCH:" -ForegroundColor Magenta
    $twitchLogs = Get-ContainerLogs -ContainerName "botcore-twitch" -Lines 5
    if ($twitchLogs) {
        $twitchLogs | Select-Object -Last 5 | ForEach-Object { 
            if ($_ -match "LIVE|live|deployed|monitoring|viewer") {
                Write-Host "  $_" -ForegroundColor Cyan
            } elseif ($_ -match "error|failed|exception") {
                Write-Host "  $_" -ForegroundColor Red
            } else {
                Write-Host "  $_" -ForegroundColor Gray
            }
        }
    }
    
    Write-Host ""
    Write-Host "🔴 YOUTUBE:" -ForegroundColor Magenta
    $youtubeLogs = Get-ContainerLogs -ContainerName "botcore-youtube" -Lines 5
    if ($youtubeLogs) {
        $youtubeLogs | Select-Object -Last 5 | ForEach-Object { 
            if ($_ -match "LIVE|live|deployed|monitoring|viewer") {
                Write-Host "  $_" -ForegroundColor Cyan
            } elseif ($_ -match "error|failed|exception") {
                Write-Host "  $_" -ForegroundColor Red
            } else {
                Write-Host "  $_" -ForegroundColor Gray
            }
        }
    }
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    
    if ($allHealthy) {
        Write-Host "✅ ALL SYSTEMS OPERATIONAL" -ForegroundColor Green
    } else {
        Write-Host "⚠️  ISSUES DETECTED - Check logs above" -ForegroundColor Yellow
    }
    
    if ($issuesDetected.Count -gt 0) {
        Write-Host ""
        Write-Host "📋 Issues Log:" -ForegroundColor Yellow
        $issuesDetected | Select-Object -Last 5 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "Next check in 30 seconds... (Press Ctrl+C to stop monitoring)" -ForegroundColor DarkGray
    
    Start-Sleep -Seconds 30
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "MONITORING SESSION COMPLETE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total checks performed: $checkCount" -ForegroundColor White
Write-Host "Total issues detected: $($issuesDetected.Count)" -ForegroundColor White
Write-Host ""

if ($issuesDetected.Count -gt 0) {
    Write-Host "Issues Summary:" -ForegroundColor Yellow
    $issuesDetected | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
} else {
    Write-Host "✅ No issues detected during monitoring period!" -ForegroundColor Green
}

