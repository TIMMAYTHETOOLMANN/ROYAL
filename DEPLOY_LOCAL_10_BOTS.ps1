# ============================================================
# LOCAL DEPLOYMENT - 10 BOTS WITH FULL GUI (HEADLESS DISABLED)
# ============================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  YouTube Watch Time Booster - LOCAL" -ForegroundColor Cyan
Write-Host "  10 Visible Firefox Windows" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables for local deployment
$env:CHANNEL_USERNAME = "timmaythetoolman"
$env:VIEWER_COUNT = "10"
$env:HEADLESS = "false"
$env:LOW_CPU_RAM = "false"

Write-Host "[1/4] Building application..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\BotCore"
dotnet build -c Release --runtime win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "[2/4] Installing Playwright browsers..." -ForegroundColor Yellow
dotnet tool install --global Microsoft.Playwright.CLI --ignore-failed-sources
playwright install firefox

Write-Host "[3/4] Configuration:" -ForegroundColor Green
Write-Host "  - Channel: $env:CHANNEL_USERNAME" -ForegroundColor White
Write-Host "  - Bots: $env:VIEWER_COUNT" -ForegroundColor White
Write-Host "  - Headless: $env:HEADLESS (FULL GUI VISIBLE)" -ForegroundColor White
Write-Host "  - Low Resource Mode: $env:LOW_CPU_RAM" -ForegroundColor White
Write-Host ""

Write-Host "[4/4] Launching Watch Time Booster..." -ForegroundColor Yellow
Write-Host "Firefox windows will open shortly - DO NOT CLOSE THEM!" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop all bots." -ForegroundColor Yellow
Write-Host ""

# Run the application
dotnet run --project BotCore.csproj -c Release --no-build -- WatchTimeBoosterEntryPoint

Write-Host ""
Write-Host "Deployment stopped." -ForegroundColor Red

