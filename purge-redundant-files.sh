#!/bin/bash
set -e

echo "🔥 PURGING REDUNDANT/NON-ESSENTIAL FILES..."

# Critical files to KEEP
KEEP_FILES=(
    "BotCore/" "config/" "monitoring/" ".dockerignore" ".env" ".env.example" 
    "Dockerfile" "Dockerfile.enterprise" "deploy-enterprise.sh" "validate-enterprise.sh"
    "README.md" "StreamViewerBot.sln" "LICENSE"
)

# Files to PURGE (redundant deployment scripts)
PURGE_FILES=(
    "*.bat" "*.ps1" "DEPLOYMENT_*.md" "CHAT_*.md" "*.txt" "*.png"
    "Dockerfile.twitch" "Dockerfile.youtube" "Dockerfile.fortified" "Dockerfile.visible"
    "deploy-twitch.sh" "deploy-youtube.sh" "deploy-*.sh" "deploy-*.ps1"
    "LAUNCH_*.bat" "STOP_*.bat" "TEST_*.bat" "QUICK_*.md" "*.SUMMARY.md"
    "HEADLESS_*.txt" "FORTIFIED_*.md" "SOPHISTICATED_*.md" "STANDBY_*.md"
    "MULTIPLATFORM_*.md" "PRE_STREAM_*.md" "WATCHTIME_*.md" "OAUTH_*.md"
    "IMPLEMENTATION_*.md" "CRITICAL_*.md" "COMPLETE_*.md" "FINAL_*.md"
    "ENHANCED_*.md" "PLATFORM_*.md" "HOW_TO_*.md" "DOCKER_*.md"
    "AutoDeploy.cs" "ChatEngagementDemo.cs" "AutoUpdater/" "InstantDeploy/"
)

echo "🗑️ Removing redundant files..."
for pattern in "${PURGE_FILES[@]}"; do
    find . -name "$pattern" -type f -delete 2>/dev/null || true
    echo "✅ Purged: $pattern"
done

echo "🧹 Cleaning empty directories..."
find . -type d -empty -delete 2>/dev/null || true

echo "📦 Repository purification complete!"
echo "✅ Kept: ${KEEP_FILES[*]}"

