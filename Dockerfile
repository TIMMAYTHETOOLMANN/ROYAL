# Multi-stage build for optimized Docker deployment
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

# Copy only BotCore project file (skip Windows GUI projects)
COPY ["BotCore/BotCore.csproj", "BotCore/"]

# Restore dependencies for BotCore only
WORKDIR "/src/BotCore"
RUN dotnet restore "BotCore.csproj"

# Copy BotCore source code
COPY ["BotCore/", "BotCore/"]

# Build BotCore as headless service
WORKDIR "/src/BotCore"
RUN dotnet clean "BotCore.csproj" && \
    dotnet build "BotCore.csproj" -c Release -o /app/build

# Publish optimized release
FROM build AS publish
WORKDIR "/src/BotCore"
RUN dotnet publish "BotCore.csproj" -c Release -o /app/publish \
    --runtime linux-x64 \
    --self-contained false \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Install Playwright browsers in build stage (has SDK)
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN playwright install chromium

# Runtime image with Playwright dependencies
FROM mcr.microsoft.com/dotnet/aspnet:3.1-bullseye-slim AS runtime
WORKDIR /app

# Install Playwright system dependencies + Chromium + Xvfb for non-headless mode
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    procps \
    fonts-liberation \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libatspi2.0-0 \
    libcups2 \
    libdbus-1-3 \
    libdrm2 \
    libgbm1 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libwayland-client0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    xdg-utils \
    libu2f-udev \
    libvulkan1 \
    xvfb \
    x11vnc \
    fluxbox \
    && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Copy Playwright browsers from build stage to /root/.cache (standard location)
COPY --from=publish /root/.cache/ms-playwright /root/.cache/ms-playwright

# Environment configuration
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=0
ENV DISPLAY=:99
ENV DISPLAY_WIDTH=1920
ENV DISPLAY_HEIGHT=1080

# Optimize Chromium for Docker (non-headless with Xvfb)
ENV CHROME_BIN=/ms-playwright/chromium-*/chrome-linux/chrome
ENV PLAYWRIGHT_CHROMIUM_ARGS="--no-sandbox --disable-setuid-sandbox --disable-dev-shm-usage"

# Create startup script that launches Xvfb then the bot
RUN printf '#!/bin/bash\n\
Xvfb :99 -screen 0 ${DISPLAY_WIDTH}x${DISPLAY_HEIGHT}x24 -ac +extension GLX +render -noreset &\n\
sleep 2\n\
exec dotnet BotCore.dll "$@"\n' > /app/start.sh && chmod +x /app/start.sh

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD pgrep -f dotnet || exit 1

ENTRYPOINT ["/app/start.sh"]

