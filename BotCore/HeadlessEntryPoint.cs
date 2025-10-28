using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotCore.Dto;
using Serilog;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    public class HeadlessEntryPoint
    {
        private static Core _botCore;
        private static bool _isRunning = true;

        public static async Task Main(string[] args)
        {
            SerilogLog.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "bot-.log"), 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            try
            {
                // Multi-platform configuration with single USERNAME
                var username = Environment.GetEnvironmentVariable("USERNAME") ?? Environment.GetEnvironmentVariable("STREAM_USERNAME");
                var platform = Environment.GetEnvironmentVariable("PLATFORM") ?? "trovo";
                var streamUrl = Environment.GetEnvironmentVariable("TARGET_STREAM_URL");
                var preStreamMode = bool.Parse(Environment.GetEnvironmentVariable("PRE_STREAM_MODE") ?? "false");
                
                // Auto-construct URL from username and platform if URL not provided
                if (string.IsNullOrEmpty(streamUrl) && !string.IsNullOrEmpty(username))
                {
                    streamUrl = ConstructStreamUrl(username, platform);
                }
                
                var maxViewers = int.Parse(Environment.GetEnvironmentVariable("MAX_VIEWERS") ?? "25");
                var minViewers = int.Parse(Environment.GetEnvironmentVariable("MIN_VIEWERS") ?? "10");
                var enableChat = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_CHAT") ?? "true");
                var proxyFile = Environment.GetEnvironmentVariable("PROXY_LIST_PATH") ?? "proxies.txt";
                
                // Advanced deployment configuration
                var enableStaggering = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_STAGGERING") ?? "true");
                var staggerDelayMin = int.Parse(Environment.GetEnvironmentVariable("STAGGER_DELAY_MIN") ?? "3000");
                var staggerDelayMax = int.Parse(Environment.GetEnvironmentVariable("STAGGER_DELAY_MAX") ?? "8000");
                var enableDynamicEntry = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_DYNAMIC_ENTRY") ?? "true");
                // FORCED: Always use visible Chrome instances, NOT headless
                var headlessMode = false;
                
                // Chat engagement settings
                var chatPercentage = int.Parse(Environment.GetEnvironmentVariable("CHAT_ENGAGEMENT_PERCENT") ?? "30");
                var minChatDelay = int.Parse(Environment.GetEnvironmentVariable("MIN_CHAT_DELAY") ?? "45");
                var maxChatDelay = int.Parse(Environment.GetEnvironmentVariable("MAX_CHAT_DELAY") ?? "180");
                var aggressiveChat = bool.Parse(Environment.GetEnvironmentVariable("AGGRESSIVE_CHAT") ?? "false");

                SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
                SerilogLog.Information("║   MULTI-PLATFORM STREAM VIEWER BOT - DOCKER HEADLESS      ║");
                SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");
                SerilogLog.Information($"Mode: {(preStreamMode ? "PRE-STREAM MONITOR (Auto-Deploy)" : "IMMEDIATE DEPLOY")}");
                SerilogLog.Information($"Username: {username ?? "Not specified"}");
                SerilogLog.Information($"Platform: {platform.ToUpper()}");
                SerilogLog.Information($"Target Stream: {streamUrl ?? "Not specified"}");
                SerilogLog.Information($"Headless Mode: DISABLED (Using actual Chrome windows)");
                SerilogLog.Information($"Headless Mode: {headlessMode}");
                SerilogLog.Information($"Staggered Entry: {enableStaggering} ({staggerDelayMin}-{staggerDelayMax}ms)");
                SerilogLog.Information($"Dynamic Entry: {enableDynamicEntry}");
                SerilogLog.Information($"Dynamic Fluctuation: Enabled (viewers join/leave naturally)");
                SerilogLog.Information($"Chat Engagement: {enableChat} ({chatPercentage}% active)");
                if (enableChat)
                {
                    SerilogLog.Information($"Chat Delays: {minChatDelay}-{maxChatDelay}s");
                    SerilogLog.Information($"Aggressive Mode: {aggressiveChat}");
                }
                if (preStreamMode)
                {
                    SerilogLog.Information($"Live Check Interval: Every 3 minutes");
                }

                if (string.IsNullOrEmpty(streamUrl))
                {
                    SerilogLog.Error("Unable to determine stream URL! Set USERNAME + PLATFORM or TARGET_STREAM_URL");
                    Environment.Exit(1);
                }

                if (!File.Exists(proxyFile))
                {
                    SerilogLog.Warning($"Proxy file not found: {proxyFile}. Bot will start without proxies.");
                }

                var proxies = File.Exists(proxyFile) ? await File.ReadAllLinesAsync(proxyFile) : Array.Empty<string>();
                SerilogLog.Information($"Loaded {proxies.Length} proxies");

                var needs = new ExecuteNeedsDto
                {
                    Stream = streamUrl,
                    BrowserLimit = maxViewers,
                    Headless = headlessMode, // Docker uses true, local can use false
                    UseLowCpuRam = headlessMode, // Enable CPU optimizations in Docker
                    ProxyListDirectory = proxyFile,
                    EnableChatEngagement = enableChat,
                    ChatEngagementPercentage = chatPercentage,
                    MinMessageDelaySeconds = minChatDelay,
                    MaxMessageDelaySeconds = maxChatDelay,
                    UseAggressiveChatMode = aggressiveChat,
                    Service = platform
                };

                // Pre-stream monitoring mode - wait for stream to go live
                if (preStreamMode)
                {
                    SerilogLog.Information("🕐 PRE-STREAM MODE: Waiting for stream to go live...");
                    var monitor = new PreStreamMonitor(username, platform, needs, minViewers, maxViewers);
                    await monitor.StartMonitoring();
                }
                else
                {
                    _botCore = new Core
                    {
                        EnableChatEngagement = enableChat,
                        EnableRealisticBehavior = true,
                        EnableAdaptiveViewing = enableDynamicEntry,
                        MaxConcurrentLaunches = enableStaggering ? 3 : 5,
                        PreferredQuality = "720p",
                        MinSessionDuration = 300,
                        MaxSessionDuration = 900
                    };

                    _botCore.LogMessage += (message, level) => 
                    {
                        SerilogLog.Information(message);
                    };

                    _botCore.IncreaseViewer += () => SerilogLog.Information("✓ Viewer joined");
                    _botCore.DecreaseViewer += () => SerilogLog.Information("✗ Viewer left");
                    _botCore.LiveViewer += (count) => SerilogLog.Information($"⚡ Live viewers: {count}");

                    SerilogLog.Information("🚀 Starting bot engine...");
                    
                    // Use staggered deployment if enabled
                    if (enableStaggering && enableDynamicEntry)
                    {
                        await StartWithStaggeredDeployment(needs, minViewers, maxViewers, staggerDelayMin, staggerDelayMax);
                    }
                    else
                    {
                        _botCore.Start(needs);
                    }
                }

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    SerilogLog.Information("Shutdown signal received");
                    _isRunning = false;
                };

                SerilogLog.Information("Bot is running. Press Ctrl+C to stop.");
                while (_isRunning)
                {
                    await Task.Delay(1000);
                }

                SerilogLog.Information("Stopping bot...");
                _botCore?.Dispose();
                SerilogLog.Information("Bot stopped successfully");
            }
            catch (Exception ex)
            {
                SerilogLog.Fatal(ex, "Fatal error in bot execution");
                Environment.Exit(1);
            }
            finally
            {
                SerilogLog.CloseAndFlush();
            }
        }

        private static string DetectPlatform(string url)
        {
            if (url.Contains("trovo.live")) return "trovo";
            if (url.Contains("kick.com")) return "kick";
            if (url.Contains("youtube.com") || url.Contains("youtu.be")) return "youtube";
            if (url.Contains("twitch.tv")) return "twitch";
            if (url.Contains("rumble.com")) return "rumble";
            return "auto";
        }

        private static string ConstructStreamUrl(string username, string platform)
        {
            return platform.ToLower() switch
            {
                "trovo" => $"https://trovo.live/s/{username}",
                "kick" => $"https://kick.com/{username}",
                "youtube" => $"https://youtube.com/@{username}/live",
                "twitch" => $"https://twitch.tv/{username}",
                "rumble" => $"https://rumble.com/c/{username}",
                _ => $"https://trovo.live/s/{username}" // Default to Trovo
            };
        }

        private static async Task StartWithStaggeredDeployment(ExecuteNeedsDto needs, int minViewers, int maxViewers, int delayMin, int delayMax)
        {
            var random = new Random();
            var targetViewers = random.Next(minViewers, maxViewers + 1);
            
            SerilogLog.Information($"📊 Dynamic deployment target: {targetViewers} viewers");
            
            // Calculate deployment phases
            var phases = CalculateDeploymentPhases(targetViewers);
            
            foreach (var phase in phases)
            {
                SerilogLog.Information($"🔄 Phase {phase.Item1}: Deploying {phase.Item2} viewers");
                
                for (int i = 0; i < phase.Item2; i++)
                {
                    if (!_isRunning) break;
                    
                    var singleViewerNeeds = new ExecuteNeedsDto
                    {
                        Stream = needs.Stream,
                        BrowserLimit = 1,
                        Headless = needs.Headless,
                        UseLowCpuRam = needs.UseLowCpuRam,
                        ProxyListDirectory = needs.ProxyListDirectory,
                        EnableChatEngagement = needs.EnableChatEngagement,
                        ChatEngagementPercentage = needs.ChatEngagementPercentage,
                        MinMessageDelaySeconds = needs.MinMessageDelaySeconds,
                        MaxMessageDelaySeconds = needs.MaxMessageDelaySeconds,
                        UseAggressiveChatMode = needs.UseAggressiveChatMode,
                        Service = needs.Service
                    };
                    
                    Task.Run(() => _botCore.Start(singleViewerNeeds));
                    
                    var delay = random.Next(delayMin, delayMax);
                    SerilogLog.Information($"⏳ Stagger delay: {delay}ms");
                    await Task.Delay(delay);
                }
                
                // Inter-phase delay
                if (phase.Item1 < phases.Count)
                {
                    var interPhaseDelay = random.Next(10000, 20000);
                    SerilogLog.Information($"⏸️  Phase break: {interPhaseDelay / 1000}s");
                    await Task.Delay(interPhaseDelay);
                }
            }
            
            SerilogLog.Information($"✅ Deployment complete: {targetViewers} viewers active");
        }

        private static List<(int, int)> CalculateDeploymentPhases(int totalViewers)
        {
            var phases = new List<(int, int)>(); // (PhaseNumber, ViewerCount)
            
            if (totalViewers <= 5)
            {
                phases.Add((1, totalViewers));
            }
            else if (totalViewers <= 20)
            {
                var phase1 = Math.Min(3, totalViewers);
                var phase2 = Math.Min(7, totalViewers - phase1);
                var phase3 = totalViewers - phase1 - phase2;
                
                if (phase1 > 0) phases.Add((1, phase1));
                if (phase2 > 0) phases.Add((2, phase2));
                if (phase3 > 0) phases.Add((3, phase3));
            }
            else
            {
                var phase1 = 5;
                var phase2 = 10;
                var remaining = totalViewers - 15;
                
                phases.Add((1, phase1));
                phases.Add((2, phase2));
                if (remaining > 0) phases.Add((3, remaining));
            }
            
            return phases;
        }
    }
}

