I. # QUICK REFERENCE GUIDE - STREAM VIEWER BOT
**Environment**: Git Bash (Windows)  
**Docker**: Legacy builder (BuildKit DISABLED)

---

## CRITICAL CONFIGURATION

### Environment Variables (MUST BE CORRECT)
```yaml
HEADLESS: false          # CRITICAL - must be false for visual rendering
DISPLAY: :99             # Xvfb virtual display
LOW_CPU_RAM: false       # Full GPU acceleration
ENABLE_CHAT: true        # Chat engagement enabled
VIEWER_COUNT: 50/30      # Concurrent browser instances
```

### Why HEADLESS=false is Critical
- Twitch/YouTube anti-bot detection flags pure headless browsers
- System uses Xvfb (virtual framebuffer) for visual rendering
- Browsers render full UI to virtual display :99
- Anti-bot systems see legitimate browser, not headless automation

---

## DEPLOYMENT COMMANDS

### Fresh Deployment
```bash
# Stop any running containers
docker-compose -f docker-compose.gitbash.yml down

# Build image (legacy builder)
DOCKER_BUILDKIT=0 docker build -t botcore-base:latest -f Dockerfile.fortified .

# Deploy containers
docker-compose -f docker-compose.gitbash.yml up -d
```

### Quick Redeploy (image already built)
```bash
docker-compose -f docker-compose.gitbash.yml down
docker-compose -f docker-compose.gitbash.yml up -d
```

### Force Rebuild (use if containers crash-loop)
```bash
docker-compose -f docker-compose.gitbash.yml down
DOCKER_BUILDKIT=0 docker build --no-cache -t botcore-base:latest -f Dockerfile.fortified .
docker-compose -f docker-compose.gitbash.yml up -d
```

---

## MONITORING COMMANDS

### Check Container Status
```bash
docker ps
docker ps -a  # Include stopped containers
```

### View Logs
```bash
# Real-time logs
docker logs -f botcore-twitch
docker logs -f botcore-youtube

# Last 50 lines
docker logs botcore-twitch --tail 50
docker logs botcore-youtube --tail 50
```

### Check Resource Usage
```bash
docker stats
```

### Test Health Endpoints
```bash
curl http://localhost:8080/health  # Twitch bot
curl http://localhost:8081/health  # YouTube bot
```

### Verify Xvfb is Running
```bash
docker exec botcore-twitch ps aux | grep Xvfb
```

---

## TROUBLESHOOTING

### Issue: "BotCore.dll does not exist"
**Cause**: Docker build failed to copy compiled application  
**Fix**:
```bash
docker-compose -f docker-compose.gitbash.yml down
DOCKER_BUILDKIT=0 docker build --no-cache -t botcore-base:latest -f Dockerfile.fortified .
docker-compose -f docker-compose.gitbash.yml up -d
```

### Issue: "Server is already active for display 99"
**Cause**: Xvfb lock file from previous crash  
**Fix**:
```bash
docker-compose -f docker-compose.gitbash.yml down
docker system prune -f
docker-compose -f docker-compose.gitbash.yml up -d
```

### Issue: Containers in "Restarting" status
**Cause**: Application crashing on startup  
**Fix**:
```bash
# Check logs for specific error
docker logs botcore-twitch --tail 100

# Common causes:
# 1. Missing BotCore.dll (rebuild image)
# 2. Xvfb display conflict (restart containers)
# 3. Missing config files (check volumes)
```

### Issue: YAML syntax errors
**Cause**: Natural language contamination or encoding issues  
**Fix**:
```bash
# Validate YAML syntax
cat docker-compose.gitbash.yml | grep -E "^\s*-\s*[A-Z_]+"

# Check for duplicate entries
cat docker-compose.gitbash.yml | sort | uniq -d

# Recreate from template if corrupted
```

---

## KNOWN ISSUES

### 1. Git Bash Path Translation
- Git Bash translates `/app/` to `C:/Program Files/Git/app/`
- **Solution**: Avoid absolute paths in docker commands
- Use `winpty docker run` if path issues persist

### 2. PowerShell Syntax Incompatibility
- **Never use**: `&&` operators, PowerShell-style environment variables
- **Always use**: `;` for command chaining, Bash error handling with `||`

### 3. UTF-8 BOM Corruption
- Files created in Windows IDEs may contain BOM
- **Solution**: Use Git Bash heredoc or strip BOM with `sed -i '1s/^\xEF\xBB\xBF//'`

---

## FILE STRUCTURE

```
Stream-Viewer-Chat-Bot/
├── deploy-gitbash.sh              # Main deployment script
├── monitor.sh                     # Monitoring script
├── Dockerfile.fortified           # Multi-stage build (Xvfb + Playwright)
├── docker-compose.gitbash.yml     # Git Bash compatible config
├── BotCore/                       # .NET application source
│   ├── BotCore.csproj
│   ├── Core.cs
│   ├── ChatEngagementEngine.cs
│   └── ...
├── proxies.txt                    # Proxy list (one per line)
├── logs/                          # Container logs
└── prometheus.yml                 # Metrics config
```

---

## ENVIRONMENT REQUIREMENTS

### System Resources
- **CPU**: 8+ cores recommended
- **RAM**: 16+ GB (8GB minimum)
- **Disk**: 10+ GB free space
- **Network**: Stable connection for 50+ concurrent streams

### Software Requirements
- Docker Desktop 4.48.0+
- Git Bash (MSYS2)
- .NET 8.0 SDK (embedded in Docker image)

---

## PORT MAPPINGS

| Service    | Port | Purpose                    |
|------------|------|----------------------------|
| Twitch     | 5000 | Internal app endpoint      |
| Twitch     | 8080 | Health check               |
| YouTube    | 5001 | Internal app endpoint      |
| YouTube    | 8081 | Health check               |
| Prometheus | 9090 | Metrics API/UI             |
| Grafana    | 3000 | Dashboard (admin/admin)    |

**Note**: Ports 5000/5001 are NOT user interfaces - they're internal endpoints for container health monitoring.

---

## SUCCESS INDICATORS

### Container Status
```
NAMES                      STATUS
botcore-twitch             Up 2 minutes (healthy)
botcore-youtube            Up 2 minutes (healthy)
prometheus-multiplatform   Up 2 minutes
grafana-multiplatform      Up 2 minutes
```

### Expected Logs
- "Xvfb started on display :99"
- "Starting BotCore application"
- "Launching X viewer instances"
- NO "BotCore.dll does not exist" errors
- NO "Server is already active for display 99" errors

### Resource Usage
- CPU: 200-400% (2-4 cores)
- RAM: 4-8 GB per bot container

---

## CONTACT INFORMATION

For additional assistance, provide:
1. Output of `docker ps -a`
2. Output of `docker logs botcore-twitch --tail 100`
3. Contents of docker-compose.gitbash.yml
4. System resource availability (CPU, RAM, Disk)

**Report Generated**: October 27, 2025
#!/bin/bash
# ========================================================================
# STREAM VIEWER BOT - GIT BASH DEPLOYMENT SCRIPT
# ========================================================================
# Purpose: Deploy multi-platform viewer emulation system
# Environment: Git Bash (Windows)
# Docker: BuildKit DISABLED (legacy builder required)
# ========================================================================

set -e  # Exit on any error

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║       STREAM VIEWER BOT - GIT BASH NATIVE DEPLOYMENT            ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

# Step 1: Verify Docker is running
echo "[1/4] Checking Docker status..."
if ! docker info > /dev/null 2>&1; then
    echo "✗ Docker is not running"
    echo "  Please start Docker Desktop and run this script again"
    exit 1
fi
echo "✓ Docker is running"
echo ""

# Step 2: Build Docker image with legacy builder (no BuildKit)
echo "[2/4] Building Docker image..."
echo "  → Using legacy Docker builder (BuildKit disabled)"
DOCKER_BUILDKIT=0 docker build -t botcore-base:latest -f Dockerfile.fortified . || {
    echo "✗ Build failed"
    echo ""
    echo "Troubleshooting:"
    echo "  1. Check BotCore/ directory exists"
    echo "  2. Verify BotCore/BotCore.csproj is present"
    echo "  3. Ensure Docker has sufficient resources (8GB+ RAM)"
    echo "  4. Review build logs above for specific errors"
    exit 1
}
echo "✓ Image built successfully"
echo ""

# Step 3: Deploy containers
echo "[3/4] Deploying containers..."
DOCKER_BUILDKIT=0 COMPOSE_DOCKER_CLI_BUILD=0 docker-compose -f docker-compose.gitbash.yml up -d || {
    echo "✗ Deployment failed"
    echo ""
    echo "Troubleshooting:"
    echo "  1. Check docker-compose.gitbash.yml for YAML syntax errors"
    echo "  2. Verify all volume paths exist (proxies.txt, logs/, appsettings.json)"
    echo "  3. Ensure ports 5000, 5001, 8080, 8081, 9090, 3000 are available"
    exit 1
}
echo "✓ Containers deployed"
echo ""

# Step 4: Verify deployment
echo "[4/4] Verifying deployment..."
sleep 5
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║                  DEPLOYMENT COMPLETE                             ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Services:"
echo "  • Twitch Bot: 50 viewers, 25% chat engagement"
echo "  • YouTube Bot: 30 viewers, 20% chat engagement"
echo "  • Prometheus: http://localhost:9090 (metrics)"
echo "  • Grafana: http://localhost:3000 (admin/admin)"
echo ""
echo "Configuration:"
echo "  • Visual Rendering: ENABLED (Xvfb on DISPLAY=:99)"
echo "  • Headless Mode: DISABLED (defeats anti-bot detection)"
echo "  • GPU Acceleration: ENABLED (LOW_CPU_RAM=false)"
echo ""
echo "Monitor logs:"
echo "  docker logs -f botcore-twitch"
echo "  docker logs -f botcore-youtube"
echo ""
echo "Check container health:"
echo "  docker ps"
echo "  curl http://localhost:8080/health  # Twitch bot"
echo "  curl http://localhost:8081/health  # YouTube bot"
echo ""

