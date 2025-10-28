#!/bin/bash
set -e

# Start Xvfb virtual display
echo "Starting Xvfb virtual display on :99..."
Xvfb :99 -screen 0 1920x1080x24 &
XVFB_PID=$!

# Wait for Xvfb to be ready
sleep 2

# Optional: Start VNC server for debugging (access via VNC client on port 5900)
if [ "$ENABLE_VNC" = "true" ]; then
    echo "Starting VNC server on port 5900..."
    x11vnc -display :99 -forever -shared -rfbport 5900 &
    echo "VNC server started. Connect to <container-ip>:5900 to view Chrome windows"
fi

# Start the bot application
echo "Starting Stream Viewer Bot..."
cd /app/BotCore
dotnet run --project BotCore.csproj --configuration Release --no-build

# Cleanup on exit
kill $XVFB_PID

