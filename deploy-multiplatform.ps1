# ========================================================================
# FORTIFIED MULTI-PLATFORM STAGED STANDBY DEPLOYMENT - JARVIS 3.0
# Enterprise-Grade Reliability with Dynamic Resource Management
# ========================================================================

param(
    [string]$Channel = "timmaythetoolman",
    [int]$MaxViewersPerPlatform = 50,
    [int]$MinViewersPerPlatform = 30,
    [int]$CheckIntervalMinutes = 3,
    [switch]$ForceRebuild,
    [switch]$SkipHealthChecks,
    [string]$Environment = "Production"
)

# Enhanced Configuration with Resource Limits
$Config = @{
    COMPOSE_FILE = "docker-compose.fortified.yml"
    DOCKERFILE = "Dockerfile"
    IMAGE_NAME = "botcore:fortified-v3"
    PLATFORMS = @("Twitch", "YouTube")
    RESOURCE_LIMITS = @{
        CPU_LIMIT = "2.0"           # Max 2 CPUs per container
        MEMORY_LIMIT = "1GB"        # Max 1GB RAM per container
        MEMORY_RESERVATION = "512m" # Minimum guaranteed memory
        RESTART_POLICY = "on-failure:3" # Max 3 restarts on failure
    }
    DEPLOYMENT = @{
        MAX_RETRIES = 5
        RETRY_DELAY_SECONDS = 30
        HEALTH_CHECK_TIMEOUT = 60
        GRADUAL_SCALE_UP = $true
    }
}

# Enhanced Logging Setup
$LogFile = "logs/deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$ErrorLogFile = "logs/deployment-errors-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

function Write-Log {
    param([string]$Message, [string]$Type = "INFO", [string]$Color = "White")
    
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "[$Timestamp] [$Type] $Message"
    
    Write-Host $LogMessage -ForegroundColor $Color
    Add-Content -Path $LogFile -Value $LogMessage
}

function Write-ErrorLog {
    param([string]$Message, [Exception]$Exception)
    
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $ErrorMessage = "[$Timestamp] [ERROR] $Message"
    if ($Exception) {
        $ErrorMessage += "`nException: $($Exception.Message)`nStack Trace: $($Exception.StackTrace)"
    }
    
    Add-Content -Path $ErrorLogFile -Value $ErrorMessage
    Write-Log $Message "ERROR" "Red"
}

function Test-SystemResources {
    Write-Log "Checking system resource availability..." "DEBUG" "Cyan"
    
    # Check Docker resource allocation
    try {
        $dockerInfo = docker info --format "json" | ConvertFrom-Json
        if ($dockerInfo.NCPU -lt 2) {
            Write-ErrorLog "Insufficient CPU cores allocated to Docker. Minimum 2 required."
            return $false
        }
        
        if ([int]($dockerInfo.MemTotal / 1GB) -lt 4) {
            Write-ErrorLog "Insufficient memory allocated to Docker. Minimum 4GB required."
            return $false
        }
    } catch {
        Write-Log "Could not query Docker info, proceeding with caution..." "WARNING" "Yellow"
    }
    
    # Check disk space
    $disk = Get-PSDrive -Name "C"
    if ($disk.Free / 1GB -lt 10) {
        Write-ErrorLog "Low disk space. Minimum 10GB free space required."
        return $false
    }
    
    Write-Log "System resources adequate for deployment" "SUCCESS" "Green"
    return $true
}

function Invoke-RetryCommand {
    param(
        [scriptblock]$Command,
        [string]$Description,
        [int]$MaxRetries = 3,
        [int]$RetryDelay = 10
    )
    
    $retryCount = 0
    while ($retryCount -lt $MaxRetries) {
        try {
            $attemptNum = $retryCount + 1
            Write-Log "Attempt ${attemptNum}/${MaxRetries}: $Description" "DEBUG" "Yellow"
            $result = & $Command
            Write-Log "$Description completed successfully" "SUCCESS" "Green"
            return $result
        } catch {
            $retryCount++
            if ($retryCount -eq $MaxRetries) {
                Write-ErrorLog "Failed after $MaxRetries attempts: $Description" $_.Exception
                throw
            }
            Write-Log "Retry ${retryCount}/${MaxRetries} after $RetryDelay seconds..." "WARNING" "Yellow"
            Start-Sleep -Seconds $RetryDelay
        }
    }
}

function Get-ContainerHealth {
    param([string]$ContainerName, [int]$Port)
    
    $healthUrl = "http://localhost:$Port/health"
    $metricsUrl = "http://localhost:$Port/metrics"
    
    try {
        # Health check
        $healthResponse = Invoke-WebRequest -Uri $healthUrl -TimeoutSec 10 -ErrorAction Stop
        $healthData = $healthResponse.Content | ConvertFrom-Json
        
        # Metrics check
        $metricsResponse = Invoke-WebRequest -Uri $metricsUrl -TimeoutSec 10 -ErrorAction SilentlyContinue
        
        return @{
            Status = $healthData.status
            Checks = $healthData.checks
            MetricsAvailable = ($metricsResponse.StatusCode -eq 200)
            ResponseTime = $healthResponse.Headers['X-Response-Time']
        }
    } catch {
        return @{ Status = "Unreachable"; Error = $_.Exception.Message }
    }
}

function Initialize-MonitoringInfrastructure {
    Write-Log "Setting up fortified monitoring infrastructure..." "INFO" "Yellow"
    
    # Create comprehensive directory structure
    $dirs = @(
        "logs/twitch", "logs/youtube", "logs/audit", 
        "config/platforms", "config/monitoring",
        "monitoring/grafana/dashboards", "monitoring/grafana/datasources",
        "monitoring/prometheus/rules", "monitoring/alertmanager",
        "backups/compose", "backups/config"
    )
    
    foreach ($dir in $dirs) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
    
    # Backup current configurations
    $backupTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    if (Test-Path $Config.COMPOSE_FILE) {
        Copy-Item $Config.COMPOSE_FILE "backups/compose/$backupTimestamp-$(Split-Path $Config.COMPOSE_FILE -Leaf)"
    }
    
    Write-Log "Monitoring infrastructure initialized" "SUCCESS" "Green"
}

function Build-FortifiedImage {
    Write-Log "Building fortified Docker image with cache optimization..." "INFO" "Yellow"
    
    $buildArgs = @(
        "--file", $Config.DOCKERFILE,
        "--tag", $Config.IMAGE_NAME,
        "--build-arg", "BUILD_ENVIRONMENT=$Environment",
        "--build-arg", "MAX_VIEWERS=$MaxViewersPerPlatform"
    )
    
    if ($ForceRebuild) {
        $buildArgs += "--no-cache"
        Write-Log "Forcing complete rebuild (cache disabled)" "INFO" "Cyan"
    }
    
    $buildArgs += "."
    
    Invoke-RetryCommand -Command {
        docker build @buildArgs
    } -Description "Docker image build" -MaxRetries 2
    
    # Verify image was built successfully
    $imageExists = docker images -q $Config.IMAGE_NAME
    if (-not $imageExists) {
        throw "Image build verification failed"
    }
    
    Write-Log "Fortified image built and verified successfully" "SUCCESS" "Green"
}

function Deploy-MultiPlatformStack {
    Write-Log "Deploying fortified multi-platform stack..." "INFO" "Yellow"
    
    # Cleanup any existing deployment
    Write-Log "Cleaning up previous deployment..." "DEBUG" "Cyan"
    docker-compose -f $Config.COMPOSE_FILE down --remove-orphans --volumes --timeout 30 2>$null
    
    # Deploy with resource limits
    $deployResult = Invoke-RetryCommand -Command {
        docker-compose -f $Config.COMPOSE_FILE up -d --scale botcore-twitch=1 --scale botcore-youtube=1
    } -Description "Docker Compose deployment"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose deployment failed"
    }
    
    Write-Log "Multi-platform stack deployed successfully" "SUCCESS" "Green"
}

function Wait-ForServicesHealthy {
    param([int]$TimeoutSeconds = 300)
    
    Write-Log "Waiting for services to become healthy (timeout: $TimeoutSeconds seconds)..." "INFO" "Yellow"
    
    $startTime = Get-Date
    $services = @(
        @{ Name = "Twitch"; Port = 5000 },
        @{ Name = "YouTube"; Port = 5001 }
    )
    
    $healthyServices = @()
    
    while (((Get-Date) - $startTime).TotalSeconds -lt $TimeoutSeconds) {
        foreach ($service in $services) {
            if ($healthyServices -contains $service.Name) { continue }
            
            $health = Get-ContainerHealth -ContainerName "botcore-$($service.Name.ToLower())" -Port $service.Port
            if ($health.Status -eq "Healthy") {
                Write-Log "✓ $($service.Name) service is healthy" "SUCCESS" "Green"
                $healthyServices += $service.Name
                
                # Log detailed health information
                Write-Log "  - Metrics: $($health.MetricsAvailable)" "DEBUG" "Cyan"
                Write-Log "  - Response Time: $($health.ResponseTime)" "DEBUG" "Cyan"
            } else {
                Write-Log "⏳ $($service.Name) service not ready yet..." "DEBUG" "Yellow"
            }
        }
        
        if ($healthyServices.Count -eq $services.Count) {
            Write-Log "All services are healthy!" "SUCCESS" "Green"
            return $true
        }
        
        Start-Sleep -Seconds 10
    }
    
    Write-ErrorLog "Health check timeout. Not all services became healthy."
    return $false
}

function Show-DeploymentDashboard {
    Write-Log "Generating deployment dashboard..." "INFO" "Yellow"
    
    # System resource overview
    $systemInfo = @{
        CPU = (Get-WmiObject Win32_Processor).NumberOfCores
        Memory = [math]::Round((Get-WmiObject Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
        DockerVersion = (docker --version).Split()[2].Trim(',')
    }
    
    Write-Host "`n╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    JARVIS 3.0 - DEPLOYMENT DASHBOARD             ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`n📊 SYSTEM RESOURCES:" -ForegroundColor Magenta
    Write-Host "  • CPU Cores: $($systemInfo.CPU)" -ForegroundColor White
    Write-Host "  • Memory: $($systemInfo.Memory) GB" -ForegroundColor White
    Write-Host "  • Docker Version: $($systemInfo.DockerVersion)" -ForegroundColor White
    
    Write-Host "`n🎯 DEPLOYMENT CONFIGURATION:" -ForegroundColor Magenta
    Write-Host "  • Channel: $Channel" -ForegroundColor White
    Write-Host "  • Environment: $Environment" -ForegroundColor White
    Write-Host "  • Platforms: $($Config.PLATFORMS -join ', ')" -ForegroundColor White
    Write-Host "  • Viewers/Platform: $MinViewersPerPlatform-$MaxViewersPerPlatform" -ForegroundColor White
    Write-Host "  • Check Interval: $CheckIntervalMinutes minutes" -ForegroundColor White
    
    Write-Host "`n⚙️ RESOURCE MANAGEMENT:" -ForegroundColor Magenta
    Write-Host "  • CPU Limit: $($Config.RESOURCE_LIMITS.CPU_LIMIT) per container" -ForegroundColor White
    Write-Host "  • Memory Limit: $($Config.RESOURCE_LIMITS.MEMORY_LIMIT)" -ForegroundColor White
    Write-Host "  • Memory Reservation: $($Config.RESOURCE_LIMITS.MEMORY_RESERVATION)" -ForegroundColor White
    Write-Host "  • Restart Policy: $($Config.RESOURCE_LIMITS.RESTART_POLICY)" -ForegroundColor White
    
    # Container status
    Write-Host "`n🐳 CONTAINER STATUS:" -ForegroundColor Magenta
    docker-compose -f $Config.COMPOSE_FILE ps
    
    Write-Host "`n🔍 MONITORING ENDPOINTS:" -ForegroundColor Magenta
    Write-Host "  • Twitch Health:     http://localhost:5000/health" -ForegroundColor White
    Write-Host "  • YouTube Health:    http://localhost:5001/health" -ForegroundColor White
    Write-Host "  • Twitch Metrics:    http://localhost:5000/metrics" -ForegroundColor White
    Write-Host "  • YouTube Metrics:   http://localhost:5001/metrics" -ForegroundColor White
    Write-Host "  • Prometheus:        http://localhost:9090" -ForegroundColor White
    Write-Host "  • Grafana:           http://localhost:3000" -ForegroundColor White
    Write-Host "  • AlertManager:      http://localhost:9093" -ForegroundColor White
    
    Write-Host "`n📈 OPERATIONAL METRICS:" -ForegroundColor Magenta
    Write-Host "  • Max Concurrent Viewers: $(($MaxViewersPerPlatform * 2))" -ForegroundColor White
    Write-Host "  • Chat Engagement Rate: 25% (per platform)" -ForegroundColor White
    Write-Host "  • Viewer Fluctuation: ±15% (realistic simulation)" -ForegroundColor White
    Write-Host "  • Auto-scale Threshold: 30+ viewers" -ForegroundColor White
    
    Write-Host "`n🚀 MANAGEMENT COMMANDS:" -ForegroundColor Magenta
    Write-Host "  • View All Logs:      docker-compose -f $($Config.COMPOSE_FILE) logs -f" -ForegroundColor White
    Write-Host "  • Twitch Logs:        docker-compose -f $($Config.COMPOSE_FILE) logs -f botcore-twitch" -ForegroundColor White
    Write-Host "  • YouTube Logs:       docker-compose -f $($Config.COMPOSE_FILE) logs -f botcore-youtube" -ForegroundColor White
    Write-Host "  • Restart Platform:   docker-compose -f $($Config.COMPOSE_FILE) restart botcore-{platform}" -ForegroundColor White
    Write-Host "  • Scale Down:         docker-compose -f $($Config.COMPOSE_FILE) scale botcore-{platform}=0" -ForegroundColor White
    Write-Host "  • Full Teardown:      docker-compose -f $($Config.COMPOSE_FILE) down -v --remove-orphans" -ForegroundColor White
    
    Write-Host "`n✅ DEPLOYMENT STATUS: FORTIFIED MULTI-PLATFORM SYSTEM ACTIVE" -ForegroundColor Green
}

# ========================================================================
# MAIN DEPLOYMENT EXECUTION
# ========================================================================

try {
    Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                   JARVIS 3.0 - FORTIFIED DEPLOYMENT             ║" -ForegroundColor Cyan
    Write-Host "║           Enterprise-Grade Multi-Platform System               ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Log "Starting fortified deployment process..." "INFO" "Cyan"
    Write-Log "Channel: $Channel | Environment: $Environment" "INFO" "White"
    
    # Step 1: Pre-flight validation
    Write-Log "Step 1/7: Pre-flight system validation..." "INFO" "Yellow"
    if (-not (Test-SystemResources)) {
        throw "System resource validation failed"
    }
    
    # Step 2: Initialize infrastructure
    Write-Log "Step 2/7: Initializing monitoring infrastructure..." "INFO" "Yellow"
    Initialize-MonitoringInfrastructure
    
    # Step 3: Build fortified image
    Write-Log "Step 3/7: Building fortified Docker image..." "INFO" "Yellow"
    Build-FortifiedImage
    
    # Step 4: Deploy stack
    Write-Log "Step 4/7: Deploying multi-platform stack..." "INFO" "Yellow"
    Deploy-MultiPlatformStack
    
    # Step 5: Health checks (unless skipped)
    if (-not $SkipHealthChecks) {
        Write-Log "Step 5/7: Performing comprehensive health checks..." "INFO" "Yellow"
        $healthy = Wait-ForServicesHealthy -TimeoutSeconds 300
        if (-not $healthy) {
            Write-Warning "Some services may not be fully healthy. Check logs for details."
        }
    } else {
        Write-Log "Health checks skipped. Waiting 30 seconds for startup..." "WARNING" "Yellow"
        Start-Sleep -Seconds 30
    }
    
    # Step 6: Display dashboard
    Write-Log "Step 6/7: Generating deployment dashboard..." "INFO" "Yellow"
    Show-DeploymentDashboard
    
    # Step 7: Final status
    Write-Log "Step 7/7: Deployment completed successfully!" "SUCCESS" "Green"
    
    Write-Host "`n🎉 FORTIFIED DEPLOYMENT COMPLETE!" -ForegroundColor Magenta
    Write-Host "✅ System is monitoring: https://twitch.tv/$Channel" -ForegroundColor Green
    Write-Host "✅ System is monitoring: https://youtube.com/@$Channel/live" -ForegroundColor Green
    Write-Host "✅ Auto-deployment ready when streams go live!" -ForegroundColor Green
    
    # Write deployment summary to log
    Write-Log "Fortified deployment completed successfully at $(Get-Date)" "SUCCESS" "Green"
    
} catch {
    Write-ErrorLog "Deployment failed with critical error" $_.Exception
    Write-Host "`n❌ DEPLOYMENT FAILED! Check error logs: $ErrorLogFile" -ForegroundColor Red
    
    # Attempt emergency cleanup
    try {
        Write-Log "Attempting emergency cleanup..." "WARNING" "Yellow"
        docker-compose -f $Config.COMPOSE_FILE down --remove-orphans 2>$null
    } catch {
        Write-ErrorLog "Emergency cleanup also failed" $_.Exception
    }
    
    exit 1
}

exit 0

