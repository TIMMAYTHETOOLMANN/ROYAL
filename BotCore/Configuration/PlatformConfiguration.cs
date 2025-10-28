using System;
using System.Collections.Generic;

namespace BotCore.Configuration
{
    /// <summary>
    /// Unified platform configuration supporting Twitch and YouTube
    /// </summary>
    public class PlatformConfiguration
    {
        public string Platform { get; set; } = "Twitch";
        public int MaxViewers { get; set; } = 10;
        public TimeSpan WatchDuration { get; set; } = TimeSpan.FromMinutes(30);
        public bool EnableChat { get; set; } = true;
        public bool EnableMetrics { get; set; } = true;
        public string[] ProxyList { get; set; } = Array.Empty<string>();
        
        // Performance settings
        public PerformanceSettings Performance { get; set; } = new();
        
        // Platform-specific configurations
        public TwitchConfig? Twitch { get; set; }
        public YouTubeConfig? YouTube { get; set; }
    }

    public class PerformanceSettings
    {
        public int MaxConcurrentBrowsers { get; set; } = 10;
        public int BrowserTimeoutMinutes { get; set; } = 30;
        public int CleanupIntervalMinutes { get; set; } = 5;
        public int MemoryLimitMB { get; set; } = 512;
        public bool EnableAutoCleanup { get; set; } = true;
        public bool EnableResourceMonitoring { get; set; } = true;
    }

    public class TwitchConfig
    {
        public string? OAuthToken { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RefreshToken { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string[] ChatMessages { get; set; } = Array.Empty<string>();
        public int ChatIntervalSeconds { get; set; } = 60;
        public bool EnableFollowBot { get; set; } = false;
        public bool EnableViewerBot { get; set; } = true;
        public string StreamQuality { get; set; } = "160p";
    }

    public class YouTubeConfig
    {
        public string VideoPlaylist { get; set; } = string.Empty;
        public bool AutoSkipAds { get; set; } = true;
        public int ViewRotationMinutes { get; set; } = 15;
        public bool EnableLikes { get; set; } = false;
        public bool EnableSubscribe { get; set; } = false;
        public bool WatchShorts { get; set; } = true;
        public bool WatchLongForm { get; set; } = true;
        public string[] TargetChannels { get; set; } = Array.Empty<string>();
    }
}

