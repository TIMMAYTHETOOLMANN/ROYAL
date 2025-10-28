using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotCore.Dto;
using Serilog;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    public class DockerEntryPoint
    {
        private static Core _botCore;
        private static bool _isRunning = true;
        private static readonly string Display = Environment.GetEnvironmentVariable("DISPLAY") ?? ":99";

        public static async Task Main(string[] args)
        {
            // ENHANCED: Initialize encoding and configuration system
            InitializeEnhancedSystem();
            
            try
            {
                var mode = Environment.GetEnvironmentVariable("MODE") ?? "LIVESTREAM";
                
                if (mode.ToUpper() == "WATCHTIME")
                {
                    // Watch Time Booster Mode
                    await RunWatchTimeBoosterAsync();
                }
                else if (mode.ToUpper() == "PRESTREAM")
                {
                    // Pre-Stream Monitor Mode (Staged Standby)
                    await RunPreStreamMonitorAsync();
                }
                else
                {
                    // Live Stream Mode (existing functionality)
                    await RunLiveStreamBotAsync();
                }
            }
            catch (Exception ex)
            {
                SerilogLog.Fatal(ex, "Fatal error in Docker deployment");
                Environment.Exit(1);
            }
        }

        private static async Task RunWatchTimeBoosterAsync()
        {
            SerilogLog.Information("Starting in Watch Time Booster mode");
            
            var channelUsername = Environment.GetEnvironmentVariable("CHANNEL_USERNAME") ?? "timmaythetoolman";
            var viewerCount = int.Parse(Environment.GetEnvironmentVariable("VIEWER_COUNT") ?? "50");
            var headless = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "false");
            var lowResourceMode = bool.Parse(Environment.GetEnvironmentVariable("LOW_CPU_RAM") ?? "true");

            var booster = new AdaptiveWatchTimeBooster(channelUsername, viewerCount, headless, lowResourceMode);
            
            Console.CancelKeyPress += (sender, e) => 
            {
                e.Cancel = true;
                _isRunning = false;
                booster.Stop();
            };
            
            await booster.StartAsync();
            
            while (_isRunning)
            {
                await Task.Delay(1000);
            }
        }

        private static async Task RunPreStreamMonitorAsync()
        {
            SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
            SerilogLog.Information("║          STAGED STANDBY MODE - PRE-STREAM MONITOR         ║");
            SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");
            
            var channelUsername = Environment.GetEnvironmentVariable("CHANNEL_USERNAME") ?? "timmaythetoolman";
            var platform = Environment.GetEnvironmentVariable("PLATFORM") ?? "twitch";
            var minViewers = int.Parse(Environment.GetEnvironmentVariable("MIN_VIEWERS") ?? "30");
            var maxViewers = int.Parse(Environment.GetEnvironmentVariable("MAX_VIEWERS") ?? "50");
            var headless = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");
            var enableChat = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_CHAT") ?? "true");
            var chatPercentage = int.Parse(Environment.GetEnvironmentVariable("CHAT_ENGAGEMENT_PERCENT") ?? "25");
            
            // Configure deployment settings for when stream goes live
            var deploymentConfig = new ExecuteNeedsDto
            {
                Stream = ConstructStreamUrl(channelUsername, platform),
                BrowserLimit = maxViewers,
                Headless = headless,
                UseLowCpuRam = true,
                ProxyListDirectory = "proxies.txt",
                EnableChatEngagement = enableChat,
                ChatEngagementPercentage = chatPercentage,
                MinMessageDelaySeconds = 60,
                MaxMessageDelaySeconds = 240,
                Service = platform
            };
            
            var monitor = new PreStreamMonitor(channelUsername, platform, deploymentConfig, minViewers, maxViewers);
            
            Console.CancelKeyPress += (sender, e) => 
            {
                e.Cancel = true;
                _isRunning = false;
                SerilogLog.Information("Stopping pre-stream monitor...");
            };
            
            await monitor.StartMonitoring();
            
            while (_isRunning)
            {
                await Task.Delay(1000);
            }
        }

        private static async Task RunLiveStreamBotAsync()
        {
            // Skip virtual display setup - running in headless mode
            SerilogLog.Information("Starting in Docker headless mode");
            
            var needs = ConfigureBot();
            _botCore = InitializeBotCore(needs);
            
            // Start bot with Docker-specific optimizations
            _botCore.Start(needs);
            
            // Handle graceful shutdown
            Console.CancelKeyPress += HandleShutdown;
            
            SerilogLog.Information("Bot running in Docker container. Press Ctrl+C to stop.");
            while (_isRunning)
            {
                await Task.Delay(1000);
            }
            
            SerilogLog.Information("Stopping bot...");
            _botCore?.Dispose();
            SerilogLog.Information("Bot stopped successfully");
        }

        /// <summary>
        /// ENHANCED: Initialize system with encoding safeguards and configuration management
        /// </summary>
        private static void InitializeEnhancedSystem()
        {
            try
            {
                // Setup encoding to prevent BOM and encoding issues
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Console.OutputEncoding = new UTF8Encoding(false);
                Console.InputEncoding = new UTF8Encoding(false);

                // Initialize enhanced configuration system
                ConfigurationManager.Initialize();
                ConfigurationManager.FixEncodingIssues();

                // Configure logging from configuration
                ConfigureLogging();

                // Run pre-flight checks
                if (!EnhancedStartup.PreFlightCheck())
                {
                    SerilogLog.Warning("⚠ Pre-flight checks reported warnings, continuing...");
                }

                SerilogLog.Information("✓ Enhanced system initialized successfully");
            }
            catch (Exception ex)
            {
                // Fallback to basic logging if enhanced initialization fails
                ConfigureLogging();
                SerilogLog.Warning(ex, "⚠ Enhanced initialization failed, using fallback mode");
            }
        }

        private static void ConfigureLogging()
        {
            var utf8WithoutBom = new UTF8Encoding(false);
            
            SerilogLog.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BotCore")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    Path.Combine("logs", "bot-.log"), 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    encoding: utf8WithoutBom)
                .CreateLogger();
        }

        private static void SetupVirtualDisplay()
        {
            // No virtual display needed in headless mode
            SerilogLog.Information("Running in headless mode - no virtual display required");
        }

        private static ExecuteNeedsDto ConfigureBot()
        {
            SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
            SerilogLog.Information("║   MULTI-PLATFORM STREAM VIEWER BOT - DOCKER DEPLOYMENT    ║");
            SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");

            try
            {
                // ENHANCED: Try to use new configuration system first
                var enhancedConfig = EnhancedStartup.GetBotConfiguration();
                var dto = enhancedConfig.ToExecuteNeedsDto();
                
                SerilogLog.Information("✓ Using enhanced configuration system");
                SerilogLog.Information($"Platform: {enhancedConfig.Platform}");
                SerilogLog.Information($"Channel: {enhancedConfig.ChannelUsername}");
                SerilogLog.Information($"Viewers: {enhancedConfig.ViewerCount}");
                SerilogLog.Information($"Headless: {enhancedConfig.Headless}");
                SerilogLog.Information($"Chat Engagement: {enhancedConfig.EnableChatEngagement} ({enhancedConfig.ChatEngagementPercentage}%)");
                
                return dto;
            }
            catch (Exception ex)
            {
                // FALLBACK: Use legacy environment variable configuration
                SerilogLog.Warning(ex, "⚠ Enhanced config failed, using legacy mode");
                
                var username = Environment.GetEnvironmentVariable("USERNAME") ?? 
                               Environment.GetEnvironmentVariable("STREAM_USERNAME") ?? 
                               "timmaythetoolman";
                
                var workerId = Environment.GetEnvironmentVariable("WORKER_ID") ?? "0";
                var platformDistribution = new[] { "twitch", "youtube", "kick" };
                var platformIndex = int.Parse(workerId) % platformDistribution.Length;
                var platform = Environment.GetEnvironmentVariable("PLATFORM") ?? platformDistribution[platformIndex];
                
                var streamUrl = Environment.GetEnvironmentVariable("TARGET_STREAM_URL");
                if (string.IsNullOrEmpty(streamUrl))
                {
                    streamUrl = ConstructStreamUrl(username, platform);
                }
                
                var maxViewers = int.Parse(Environment.GetEnvironmentVariable("MAX_VIEWERS") ?? "8");
                var enableChat = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_CHAT") ?? "true");
                var proxyFile = Environment.GetEnvironmentVariable("PROXY_LIST_PATH") ?? "proxies.txt";
                var headlessMode = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS_MODE") ?? "false");
                var useLowCpuRam = true;
                var chatPercentage = int.Parse(Environment.GetEnvironmentVariable("CHAT_ENGAGEMENT_PERCENT") ?? "20");
                var minChatDelay = int.Parse(Environment.GetEnvironmentVariable("MIN_CHAT_DELAY") ?? "60");
                var maxChatDelay = int.Parse(Environment.GetEnvironmentVariable("MAX_CHAT_DELAY") ?? "240");

                SerilogLog.Information($"Username: {username}");
                SerilogLog.Information($"Platform: {platform.ToUpper()}");
                SerilogLog.Information($"Target Stream: {streamUrl}");
                SerilogLog.Information($"Viewers: {maxViewers} (optimized for stability)");
                SerilogLog.Information($"Headless Mode: {(headlessMode ? "ENABLED" : "DISABLED")}");
                SerilogLog.Information($"Resource Optimization: ENABLED");
                SerilogLog.Information($"Chat Engagement: {enableChat} ({chatPercentage}% active)");
                SerilogLog.Information("════════════════════════════════════════════════════════════");

                return new ExecuteNeedsDto
                {
                    Stream = streamUrl,
                    BrowserLimit = maxViewers,
                    Headless = headlessMode,
                    UseLowCpuRam = useLowCpuRam,
                    ProxyListDirectory = proxyFile,
                    EnableChatEngagement = enableChat,
                    ChatEngagementPercentage = chatPercentage,
                    MinMessageDelaySeconds = minChatDelay,
                    MaxMessageDelaySeconds = maxChatDelay,
                    Service = platform
                };
            }
        }

        private static Core InitializeBotCore(ExecuteNeedsDto needs)
        {
            var core = new Core
            {
                EnableChatEngagement = needs.EnableChatEngagement,
                EnableRealisticBehavior = true,
                EnableAdaptiveViewing = true,
                MaxConcurrentLaunches = 8, // INCREASED to prevent bottlenecks
                PreferredQuality = "720p",
                MinSessionDuration = 1800, // 30 minutes
                MaxSessionDuration = 3600  // 60 minutes
            };

            core.LogMessage += (message, level) => 
            {
                switch (level)
                {
                    case LogLevel.Error:
                        SerilogLog.Error(message);
                        break;
                    case LogLevel.Warning:
                        SerilogLog.Warning(message);
                        break;
                    case LogLevel.Success:
                        SerilogLog.Information($"✓ {message}");
                        break;
                    case LogLevel.Chat:
                        SerilogLog.Information($"💬 {message}");
                        break;
                    default:
                        SerilogLog.Information(message);
                        break;
                }
            };

            core.IncreaseViewer += () => SerilogLog.Information("👤 Viewer joined");
            core.DecreaseViewer += () => SerilogLog.Information("👋 Viewer left");
            core.LiveViewer += (count) => SerilogLog.Information($"👥 Live viewers: {count}");

            return core;
        }

        private static void HandleShutdown(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            SerilogLog.Information("Shutdown signal received");
            _isRunning = false;
        }

        private static string ConstructStreamUrl(string username, string platform)
        {
            // Check for explicit stream URL override first
            var explicitUrl = Environment.GetEnvironmentVariable("STREAM_URL");
            if (!string.IsNullOrEmpty(explicitUrl))
            {
                SerilogLog.Information($"Using explicit stream URL: {explicitUrl}");
                return explicitUrl;
            }

            return platform.ToLower() switch
            {
                "trovo" => $"https://trovo.live/s/{username}",
                "kick" => $"https://kick.com/{username}",
                "youtube" => $"https://www.youtube.com/@{username}/live",  // YouTube live page
                "twitch" => $"https://www.twitch.tv/{username}",
                "rumble" => $"https://rumble.com/c/{username}",
                _ => $"https://www.twitch.tv/{username}"  // Default to Twitch
            };
        }
    }
}

