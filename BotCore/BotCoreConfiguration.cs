using System;
using System.ComponentModel.DataAnnotations;

namespace BotCore
{
    /// <summary>
    /// Strongly-typed configuration model with validation
    /// Ensures backward compatibility with legacy configuration methods
    /// </summary>
    public class BotCoreConfiguration
    {
        [Required]
        public string Platform { get; set; } = "twitch";

        [Required]
        public string ChannelUsername { get; set; } = "timmaythetoolman";

        [Range(1, 1000)]
        public int ViewerCount { get; set; } = 50;

        public bool Headless { get; set; } = true;

        public bool LowCpuRam { get; set; } = true;

        public bool EnableChatEngagement { get; set; } = true;

        [Range(0, 100)]
        public int ChatEngagementPercentage { get; set; } = 25;

        [Range(10, 600)]
        public int MinMessageDelaySeconds { get; set; } = 60;

        [Range(10, 600)]
        public int MaxMessageDelaySeconds { get; set; } = 240;

        public bool EnableRealisticBehavior { get; set; } = true;

        public bool EnableAdaptiveViewing { get; set; } = true;

        public string PreferredQuality { get; set; } = "1080p";

        [Range(300, 86400)]
        public int MinSessionDuration { get; set; } = 1800; // 30 minutes

        [Range(300, 86400)]
        public int MaxSessionDuration { get; set; } = 7200; // 2 hours

        [Range(1, 50)]
        public int MaxConcurrentLaunches { get; set; } = 5;

        public string ProxyFilePath { get; set; } = "proxies.txt";

        /// <summary>
        /// Validate configuration and return validation results
        /// </summary>
        public (bool IsValid, string[] Errors) Validate()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(Platform))
                errors.Add("Platform is required");

            if (string.IsNullOrWhiteSpace(ChannelUsername))
                errors.Add("ChannelUsername is required");

            if (ViewerCount < 1 || ViewerCount > 1000)
                errors.Add("ViewerCount must be between 1 and 1000");

            if (ChatEngagementPercentage < 0 || ChatEngagementPercentage > 100)
                errors.Add("ChatEngagementPercentage must be between 0 and 100");

            if (MinMessageDelaySeconds > MaxMessageDelaySeconds)
                errors.Add("MinMessageDelaySeconds cannot be greater than MaxMessageDelaySeconds");

            if (MinSessionDuration > MaxSessionDuration)
                errors.Add("MinSessionDuration cannot be greater than MaxSessionDuration");

            return (errors.Count == 0, errors.ToArray());
        }

        /// <summary>
        /// Merge with environment variables for Docker/Kubernetes deployments
        /// </summary>
        public void MergeEnvironmentVariables()
        {
            Platform = Environment.GetEnvironmentVariable("PLATFORM") ?? Platform;
            ChannelUsername = Environment.GetEnvironmentVariable("CHANNEL_USERNAME") ?? ChannelUsername;
            
            if (int.TryParse(Environment.GetEnvironmentVariable("VIEWER_COUNT"), out int viewerCount))
                ViewerCount = viewerCount;

            if (bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out bool headless))
                Headless = headless;

            if (bool.TryParse(Environment.GetEnvironmentVariable("LOW_CPU_RAM"), out bool lowCpu))
                LowCpuRam = lowCpu;

            if (bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_CHAT"), out bool enableChat))
                EnableChatEngagement = enableChat;

            if (int.TryParse(Environment.GetEnvironmentVariable("CHAT_ENGAGEMENT_PERCENT"), out int chatPercent))
                ChatEngagementPercentage = chatPercent;

            ProxyFilePath = Environment.GetEnvironmentVariable("PROXY_FILE") ?? ProxyFilePath;
        }

        /// <summary>
        /// Create from legacy ExecuteNeedsDto for backward compatibility
        /// </summary>
        public static BotCoreConfiguration FromExecuteNeedsDto(Dto.ExecuteNeedsDto dto)
        {
            return new BotCoreConfiguration
            {
                Platform = dto.Service ?? "twitch",
                ChannelUsername = ExtractChannelFromUrl(dto.Stream),
                ViewerCount = dto.BrowserLimit,
                Headless = dto.Headless,
                LowCpuRam = dto.UseLowCpuRam,
                EnableChatEngagement = dto.EnableChatEngagement,
                ChatEngagementPercentage = dto.ChatEngagementPercentage,
                MinMessageDelaySeconds = dto.MinMessageDelaySeconds,
                MaxMessageDelaySeconds = dto.MaxMessageDelaySeconds,
                ProxyFilePath = dto.ProxyListDirectory
            };
        }

        /// <summary>
        /// Convert to legacy ExecuteNeedsDto for backward compatibility
        /// </summary>
        public Dto.ExecuteNeedsDto ToExecuteNeedsDto()
        {
            return new Dto.ExecuteNeedsDto
            {
                Stream = ConstructStreamUrl(ChannelUsername, Platform),
                BrowserLimit = ViewerCount,
                Headless = Headless,
                UseLowCpuRam = LowCpuRam,
                ProxyListDirectory = ProxyFilePath,
                EnableChatEngagement = EnableChatEngagement,
                ChatEngagementPercentage = ChatEngagementPercentage,
                MinMessageDelaySeconds = MinMessageDelaySeconds,
                MaxMessageDelaySeconds = MaxMessageDelaySeconds,
                Service = Platform
            };
        }

        private static string ExtractChannelFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "unknown";

            try
            {
                var uri = new Uri(url);
                var segments = uri.Segments;
                return segments.Length > 0 ? segments[segments.Length - 1].Trim('/') : "unknown";
            }
            catch
            {
                return url;
            }
        }

        private static string ConstructStreamUrl(string channel, string platform)
        {
            return platform.ToLower() switch
            {
                "twitch" => $"https://www.twitch.tv/{channel}",
                "youtube" => $"https://www.youtube.com/@{channel}",
                "kick" => $"https://kick.com/{channel}",
                "dlive" => $"https://dlive.tv/{channel}",
                "trovo" => $"https://trovo.live/{channel}",
                _ => $"https://www.twitch.tv/{channel}"
            };
        }
    }

    /// <summary>
    /// Monitoring configuration
    /// </summary>
    public class MonitoringConfiguration
    {
        public bool EnablePrometheus { get; set; } = true;
        public int PrometheusPort { get; set; } = 9090;
        public int HealthCheckPort { get; set; } = 8080;
        public int MetricsUpdateIntervalSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Resilience configuration for retry policies
    /// </summary>
    public class ResilienceConfiguration
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMilliseconds { get; set; } = 2000;
        public int CircuitBreakerThreshold { get; set; } = 5;
        public int CircuitBreakerDurationSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Redis caching configuration
    /// </summary>
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; } = "redis-botcore:6379";
        public bool EnableCache { get; set; } = false;
        public int DefaultExpirationMinutes { get; set; } = 30;
    }

    /// <summary>
    /// Advanced features configuration
    /// </summary>
    public class AdvancedFeaturesConfiguration
    {
        public bool EnableAutoRecovery { get; set; } = true;
        public bool EnableHealthChecks { get; set; } = true;
        public bool EnableDistributedTracing { get; set; } = false;
        public bool EnableMetrics { get; set; } = true;
    }
}

