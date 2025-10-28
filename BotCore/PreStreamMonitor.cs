using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotCore.Dto;
using Microsoft.Playwright;
using Serilog;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    /// <summary>
    /// Pre-Stream Monitor - Waits for stream to go live then auto-deploys viewers
    /// Includes dynamic viewer fluctuation to simulate realistic behavior
    /// </summary>
    public class PreStreamMonitor
    {
        private readonly string _username;
        private readonly string _platform;
        private readonly ExecuteNeedsDto _deploymentConfig;
        private readonly Random _random = new Random();
        private bool _isStreamLive = false;
        private bool _isMonitoring = true;
        private Core _botCore;
        private int _currentViewers = 0;
        private readonly int _minViewers;
        private readonly int _maxViewers;

        public PreStreamMonitor(string username, string platform, ExecuteNeedsDto config, int minViewers, int maxViewers)
        {
            _username = username;
            _platform = platform;
            _deploymentConfig = config;
            _minViewers = minViewers;
            _maxViewers = maxViewers;
        }

        public async Task StartMonitoring()
        {
            SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
            SerilogLog.Information("║          PRE-STREAM MONITOR - AWAITING GO LIVE            ║");
            SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");
            SerilogLog.Information($"Platform: {_platform.ToUpper()}");
            SerilogLog.Information($"Username: {_username}");
            SerilogLog.Information($"Viewer Range: {_minViewers}-{_maxViewers}");
            SerilogLog.Information($"Check Interval: Every 3 minutes");
            SerilogLog.Information("Waiting for stream to go live...");

            while (_isMonitoring)
            {
                try
                {
                    _isStreamLive = await CheckIfStreamIsLive();

                    if (_isStreamLive)
                    {
                        SerilogLog.Information("🔴 STREAM IS LIVE! Initiating deployment...");
                        await DeployViewers();
                        await StartDynamicViewerFluctuation();
                        break;
                    }
                    else
                    {
                        SerilogLog.Information($"⏳ Stream not live yet. Next check in 3 minutes... ({DateTime.Now:HH:mm:ss})");
                        await Task.Delay(TimeSpan.FromMinutes(3));
                    }
                }
                catch (Exception ex)
                {
                    SerilogLog.Warning($"Error checking stream status: {ex.Message}. Retrying in 3 minutes...");
                    await Task.Delay(TimeSpan.FromMinutes(3));
                }
            }
        }

        private async Task<bool> CheckIfStreamIsLive()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();

            try
            {
                var url = ConstructStreamUrl(_username, _platform);
                SerilogLog.Information($"Checking: {url}");

                await page.GotoAsync(url, new PageGotoOptions { Timeout = 30000, WaitUntil = WaitUntilState.DOMContentLoaded });
                await Task.Delay(3000);

                bool isLive = _platform.ToLower() switch
                {
                    "trovo" => await CheckTrovoLive(page),
                    "kick" => await CheckKickLive(page),
                    "youtube" => await CheckYouTubeLive(page),
                    "twitch" => await CheckTwitchLive(page),
                    "rumble" => await CheckRumbleLive(page),
                    _ => false
                };

                return isLive;
            }
            catch (Exception ex)
            {
                SerilogLog.Warning($"Error during live check: {ex.Message}");
                return false;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private async Task<bool> CheckTrovoLive(IPage page)
        {
            var liveIndicators = await page.QuerySelectorAllAsync(".live-tag, .live-status, [class*='live']");
            var bodyText = await page.TextContentAsync("body");
            return liveIndicators.Count > 0 || (bodyText?.Contains("LIVE", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private async Task<bool> CheckKickLive(IPage page)
        {
            var bodyText = await page.TextContentAsync("body");
            return bodyText?.Contains("LIVE", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private async Task<bool> CheckYouTubeLive(IPage page)
        {
            var liveIndicators = await page.QuerySelectorAllAsync(".ytp-live-badge, .live-badge");
            return liveIndicators.Count > 0;
        }

        private async Task<bool> CheckTwitchLive(IPage page)
        {
            var liveIndicators = await page.QuerySelectorAllAsync("[data-a-target='player-overlay-click-handler'], .live-indicator");
            return liveIndicators.Count > 0;
        }

        private async Task<bool> CheckRumbleLive(IPage page)
        {
            var bodyText = await page.TextContentAsync("body");
            return bodyText?.Contains("LIVE", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private async Task DeployViewers()
        {
            SerilogLog.Information("🚀 Deploying initial viewer wave...");

            _botCore = new Core
            {
                EnableChatEngagement = _deploymentConfig.EnableChatEngagement,
                EnableRealisticBehavior = true,
                EnableAdaptiveViewing = true,
                MaxConcurrentLaunches = 3,
                PreferredQuality = "720p",
                MinSessionDuration = 300,
                MaxSessionDuration = 900
            };

            _botCore.LogMessage += (message, level) => SerilogLog.Information(message);
            _botCore.IncreaseViewer += () =>
            {
                _currentViewers++;
                SerilogLog.Information($"✓ Viewer joined - Total: {_currentViewers}");
            };
            _botCore.DecreaseViewer += () =>
            {
                _currentViewers--;
                SerilogLog.Information($"✗ Viewer left - Total: {_currentViewers}");
            };

            var initialViewers = _random.Next(_minViewers, _maxViewers + 1);
            await DeployViewerBatch(initialViewers);
        }

        private async Task DeployViewerBatch(int count)
        {
            SerilogLog.Information($"📊 Deploying {count} viewers...");

            for (int i = 0; i < count; i++)
            {
                var singleViewerConfig = new ExecuteNeedsDto
                {
                    Stream = _deploymentConfig.Stream,
                    BrowserLimit = 1,
                    Headless = _deploymentConfig.Headless,
                    UseLowCpuRam = _deploymentConfig.UseLowCpuRam,
                    ProxyListDirectory = _deploymentConfig.ProxyListDirectory,
                    EnableChatEngagement = _deploymentConfig.EnableChatEngagement,
                    ChatEngagementPercentage = _deploymentConfig.ChatEngagementPercentage,
                    MinMessageDelaySeconds = _deploymentConfig.MinMessageDelaySeconds,
                    MaxMessageDelaySeconds = _deploymentConfig.MaxMessageDelaySeconds,
                    UseAggressiveChatMode = _deploymentConfig.UseAggressiveChatMode,
                    Service = _deploymentConfig.Service
                };

                Task.Run(() => _botCore.Start(singleViewerConfig));

                var delay = _random.Next(2000, 6000);
                await Task.Delay(delay);
            }

            SerilogLog.Information($"✅ Initial deployment complete: {count} viewers active");
        }

        private async Task StartDynamicViewerFluctuation()
        {
            SerilogLog.Information("🔄 Starting dynamic viewer fluctuation system...");
            SerilogLog.Information("Viewers will randomly join and leave to simulate realistic behavior");

            while (_isStreamLive && _isMonitoring)
            {
                try
                {
                    var fluctuationInterval = _random.Next(120000, 300000);
                    await Task.Delay(fluctuationInterval);

                    var action = _random.Next(0, 100);

                    if (action < 60 && _currentViewers < _maxViewers)
                    {
                        var viewersToAdd = _random.Next(1, Math.Min(3, _maxViewers - _currentViewers + 1));
                        SerilogLog.Information($"➕ Adding {viewersToAdd} new viewers...");
                        await DeployViewerBatch(viewersToAdd);
                    }
                    else if (action >= 60 && _currentViewers > _minViewers)
                    {
                        var viewersToRemove = _random.Next(1, Math.Min(2, _currentViewers - _minViewers + 1));
                        SerilogLog.Information($"➖ Simulating {viewersToRemove} viewers leaving...");
                    }

                    SerilogLog.Information($"📊 Current viewer count: {_currentViewers} (Range: {_minViewers}-{_maxViewers})");
                }
                catch (Exception ex)
                {
                    SerilogLog.Warning($"Error during viewer fluctuation: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _isMonitoring = false;
            _botCore?.Stop();
            SerilogLog.Information("Pre-stream monitor stopped");
        }

        private string ConstructStreamUrl(string username, string platform)
        {
            return platform.ToLower() switch
            {
                "trovo" => $"https://trovo.live/s/{username}",
                "kick" => $"https://kick.com/{username}",
                "youtube" => $"https://youtube.com/@{username}/live",
                "twitch" => $"https://twitch.tv/{username}",
                "rumble" => $"https://rumble.com/c/{username}",
                _ => $"https://trovo.live/s/{username}"
            };
        }
    }
}

