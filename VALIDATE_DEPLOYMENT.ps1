# ========================================================================
# ENHANCED DEPLOYMENT VALIDATION SCRIPT
# Validates configuration, encoding, and system readiness
# ========================================================================

param(
    [switch]$FixEncoding = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      JARVIS 3.0 - ENHANCED DEPLOYMENT VALIDATION                ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$script:ValidationErrors = @()
$script:ValidationWarnings = @()

function Write-ValidationStep {
    param([string]$Message, [string]$Status = "INFO")
    
    $color = switch ($Status) {
        "OK" { "Green" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    
    $symbol = switch ($Status) {
        "OK" { "✓" }
        "WARN" { "⚠" }
        "ERROR" { "✗" }
        default { "→" }
    }
    
    Write-Host "[$symbol] $Message" -ForegroundColor $color
}

function Test-FileEncoding {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        return $false
    }
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # Check for UTF-8 BOM
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        return $true
    }
    
    # Check for UTF-16 BOM
    if ($bytes.Length -ge 2 -and 
        (($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) -or 
         ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF))) {
        return $true
    }
    
    return $false
}

function Fix-FileEncoding {
    param([string]$FilePath)
    
    try {
        $content = Get-Content -Path $FilePath -Raw
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($FilePath, $content, $utf8NoBom)
        return $true
    }
    catch {
        return $false
    }
}

# Step 1: Check configuration files
Write-ValidationStep "Checking configuration files..." "INFO"

$configFiles = @(
    "BotCore\appsettings.json",
    "BotCore\appsettings.Development.json",
    "BotCore\appsettings.Production.json"
)

foreach ($configFile in $configFiles) {
    if (Test-Path $configFile) {
        $hasBOM = Test-FileEncoding -FilePath $configFile
        if ($hasBOM) {
            Write-ValidationStep "BOM detected in $configFile" "WARN"
            $script:ValidationWarnings += "BOM in $configFile"
            
            if ($FixEncoding) {
                if (Fix-FileEncoding -FilePath $configFile) {
                    Write-ValidationStep "Fixed encoding in $configFile" "OK"
                }
                else {
                    Write-ValidationStep "Failed to fix encoding in $configFile" "ERROR"
                    $script:ValidationErrors += "Could not fix $configFile"
                }
            }
        }
        else {
            Write-ValidationStep "$configFile encoding OK" "OK"
        }
        
        # Validate JSON
        try {
            $null = Get-Content $configFile -Raw | ConvertFrom-Json
            Write-ValidationStep "$configFile JSON valid" "OK"
        }
        catch {
            Write-ValidationStep "$configFile JSON invalid: $($_.Exception.Message)" "ERROR"
            $script:ValidationErrors += "Invalid JSON in $configFile"
        }
    }
    else {
        Write-ValidationStep "$configFile not found" "WARN"
        $script:ValidationWarnings += "Missing $configFile"
    }
}

# Step 2: Check source files for BOM
Write-ValidationStep "Checking source files for BOM issues..." "INFO"

$sourceFiles = Get-ChildItem -Path "BotCore\*.cs" -Recurse -ErrorAction SilentlyContinue

$bomCount = 0
foreach ($sourceFile in $sourceFiles) {
    if (Test-FileEncoding -FilePath $sourceFile.FullName) {
        $bomCount++
        if ($Verbose) {
            Write-ValidationStep "BOM in $($sourceFile.Name)" "WARN"
        }
        
        if ($FixEncoding) {
            if (Fix-FileEncoding -FilePath $sourceFile.FullName) {
                Write-ValidationStep "Fixed $($sourceFile.Name)" "OK"
            }
        }
    }
}

if ($bomCount -eq 0) {
    Write-ValidationStep "No BOM issues in source files" "OK"
}
else {
    Write-ValidationStep "$bomCount source files have BOM" "WARN"
}

# Step 3: Check Docker files
Write-ValidationStep "Checking Docker configuration..." "INFO"

$dockerFiles = @(
    "Dockerfile",
    "Dockerfile.fortified",
    "docker-compose.yml",
    "docker-compose.fortified.yml"
)

foreach ($dockerFile in $dockerFiles) {
    if (Test-Path $dockerFile) {
        Write-ValidationStep "$dockerFile exists" "OK"
    }
    else {
        if ($Verbose) {
            Write-ValidationStep "$dockerFile not found" "WARN"
        }
    }
}

# Step 4: Check proxy file
Write-ValidationStep "Checking proxy configuration..." "INFO"

if (Test-Path "proxies.txt") {
    $proxyCount = (Get-Content "proxies.txt" | Where-Object { $_.Trim() -ne "" }).Count
    Write-ValidationStep "Found $proxyCount proxies" "OK"
}
else {
    Write-ValidationStep "proxies.txt not found" "WARN"
    $script:ValidationWarnings += "No proxy file"
}

# Step 5: Check .NET SDK
Write-ValidationStep "Checking .NET environment..." "INFO"

try {
    $dotnetVersion = dotnet --version
    Write-ValidationStep ".NET SDK $dotnetVersion" "OK"
}
catch {
    Write-ValidationStep ".NET SDK not found" "ERROR"
    $script:ValidationErrors += ".NET SDK not installed"
}

# Step 6: Validate project files
Write-ValidationStep "Checking project files..." "INFO"

if (Test-Path "BotCore\BotCore.csproj") {
    Write-ValidationStep "BotCore.csproj found" "OK"
    
    try {
        [xml]$csproj = Get-Content "BotCore\BotCore.csproj"
        $targetFramework = $csproj.Project.PropertyGroup.TargetFramework
        Write-ValidationStep "Target framework: $targetFramework" "OK"
    }
    catch {
        Write-ValidationStep "Could not parse BotCore.csproj" "WARN"
    }
}
else {
    Write-ValidationStep "BotCore.csproj not found" "ERROR"
    $script:ValidationErrors += "Missing project file"
}

# Summary
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                     VALIDATION SUMMARY                          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

if ($script:ValidationErrors.Count -eq 0) {
    Write-Host "✓ No critical errors found" -ForegroundColor Green
}
else {
    Write-Host "✗ $($script:ValidationErrors.Count) error(s) found:" -ForegroundColor Red
    foreach ($validationError in $script:ValidationErrors) {
        Write-Host "  - $validationError" -ForegroundColor Red
    }
}

if ($script:ValidationWarnings.Count -gt 0) {
    Write-Host "⚠ $($script:ValidationWarnings.Count) warning(s):" -ForegroundColor Yellow
    foreach ($validationWarning in $script:ValidationWarnings) {
        Write-Host "  - $validationWarning" -ForegroundColor Yellow
    }
}

Write-Host ""
if ($FixEncoding) {
    Write-Host "Encoding fixes applied. Re-run without -FixEncoding to validate." -ForegroundColor Cyan
}
else {
    Write-Host "Run with -FixEncoding to automatically fix encoding issues." -ForegroundColor Cyan
}

exit $script:ValidationErrors.Count

