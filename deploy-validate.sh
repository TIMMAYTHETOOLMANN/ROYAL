#!/bin/bash
set -e

echo "========================================="
echo "🔍 DEPLOYMENT VALIDATION SUITE"
echo "========================================="
echo ""

VALIDATION_FAILED=0

# Function to validate Dockerfile
validate_dockerfile() {
    local platform=$1
    local dockerfile="Dockerfile.$platform"
    
    echo "Validating $dockerfile..."
    
    if [ ! -f "$dockerfile" ]; then
        echo "  ❌ Missing $dockerfile"
        return 1
    fi
    
    if ! grep -q "PLATFORM=$platform" "$dockerfile" 2>/dev/null; then
        echo "  ⚠️  Warning: $dockerfile missing PLATFORM environment variable"
    fi
    
    if ! grep -q "FROM.*dotnet.*sdk" "$dockerfile"; then
        echo "  ❌ $dockerfile missing .NET SDK build stage"
        return 1
    fi
    
    if ! grep -q "FROM.*dotnet.*runtime" "$dockerfile"; then
        echo "  ❌ $dockerfile missing .NET runtime stage"
        return 1
    fi
    
    echo "  ✅ $dockerfile validation passed"
    return 0
}

# Function to validate configuration
validate_config() {
    local platform=$1
    local config="config/appsettings.$platform.json"
    
    echo "Validating $config..."
    
    if [ ! -f "$config" ]; then
        echo "  ❌ Missing $config"
        return 1
    fi
    
    # Check if jq is available for JSON validation
    if command -v jq &> /dev/null; then
        if ! jq empty "$config" 2>/dev/null; then
            echo "  ❌ $config contains invalid JSON"
            return 1
        fi
        
        # Validate required fields
        if ! jq -e '.Platform' "$config" > /dev/null 2>&1; then
            echo "  ❌ $config missing Platform field"
            return 1
        fi
    else
        echo "  ⚠️  Warning: jq not installed, skipping JSON validation"
    fi
    
    echo "  ✅ $config validation passed"
    return 0
}

# Function to validate Docker environment
validate_docker() {
    echo "Validating Docker environment..."
    
    if ! command -v docker &> /dev/null; then
        echo "  ❌ Docker is not installed"
        return 1
    fi
    
    if ! docker info > /dev/null 2>&1; then
        echo "  ❌ Docker daemon is not running"
        return 1
    fi
    
    echo "  ✅ Docker environment validated"
    return 0
}

# Function to validate directory structure
validate_directories() {
    echo "Validating directory structure..."
    
    local required_dirs=("logs" "data" "config")
    
    for dir in "${required_dirs[@]}"; do
        if [ ! -d "$dir" ]; then
            echo "  ⚠️  Creating missing directory: $dir"
            mkdir -p "$dir"
        fi
    done
    
    echo "  ✅ Directory structure validated"
    return 0
}

# Function to validate services
validate_services() {
    local services_dir="BotCore/Services"
    
    echo "Validating service files..."
    
    if [ ! -d "$services_dir" ]; then
        echo "  ❌ Services directory not found: $services_dir"
        return 1
    fi
    
    local required_services=(
        "PerformanceOptimizedBotService.cs"
        "OptimizedBrowserManager.cs"
        "ResilientStreamWatcher.cs"
    )
    
    local missing_count=0
    for service in "${required_services[@]}"; do
        if [ ! -f "$services_dir/$service" ]; then
            echo "  ⚠️  Warning: Missing service file: $service"
            missing_count=$((missing_count + 1))
        fi
    done
    
    if [ $missing_count -gt 0 ]; then
        echo "  ⚠️  $missing_count service file(s) missing (non-critical)"
    else
        echo "  ✅ All service files present"
    fi
    
    return 0
}

echo "Starting validation checks..."
echo ""

# Run Docker validation
validate_docker || VALIDATION_FAILED=1
echo ""

# Run directory validation
validate_directories || VALIDATION_FAILED=1
echo ""

# Validate both platforms
for platform in twitch youtube; do
    echo "--- Validating $platform platform ---"
    validate_dockerfile "$platform" || VALIDATION_FAILED=1
    validate_config "$platform" || VALIDATION_FAILED=1
    echo ""
done

# Validate services
validate_services || VALIDATION_FAILED=1
echo ""

echo "========================================="
if [ $VALIDATION_FAILED -eq 0 ]; then
    echo "🎉 ALL VALIDATIONS PASSED"
    echo "========================================="
    echo ""
    echo "Ready to deploy! Use:"
    echo "  ./deploy-twitch-optimized.sh"
    echo "  ./deploy-youtube-optimized.sh"
    exit 0
else
    echo "⚠️  VALIDATION COMPLETED WITH WARNINGS"
    echo "========================================="
    echo ""
    echo "Review warnings above before deployment"
    exit 1
fi

