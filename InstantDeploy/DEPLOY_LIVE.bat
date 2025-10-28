@echo off
echo ================================================================
echo  JARVIS 2.0 - AUTONOMOUS MULTI-PLATFORM DEPLOYMENT
echo  Auto-Detect + Auto-Minimize + Performance Optimized
echo ================================================================
echo.
echo Building system...
echo.

cd /d "%~dp0"
dotnet build -c Release --verbosity minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================================
    echo  BUILD SUCCESSFUL - LAUNCHING INTELLIGENT DEPLOYMENT SYSTEM
    echo ================================================================
    echo.
    echo Features Active:
    echo   [*] Live Stream Auto-Detection (checks every 3 minutes)
    echo   [*] Auto-Minimize Windows (all browsers start minimized)
    echo   [*] IPRoyal Residential Proxies (unique IP per viewer)
    echo   [*] Anti-Detection Systems (fingerprint spoofing + human behavior)
    echo   [*] Intelligent Platform Fallback
    echo.
    echo Target Distribution:
    echo   - Trovo: 5 viewers
    echo   - Kick: 6 viewers  
    echo   - Twitch: 4 viewers
    echo.
    echo System will monitor for live streams and deploy automatically
    echo All browser windows will launch MINIMIZED (check taskbar)
    echo.
    echo Press Ctrl+C to stop monitoring/viewers
    echo ================================================================
    echo.
    
    dotnet run -c Release --no-build
) else (
    echo.
    echo ================================================================
    echo  BUILD FAILED - Check errors above
    echo ================================================================
    pause
)

