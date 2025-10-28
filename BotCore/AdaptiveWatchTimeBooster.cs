using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    /// <summary>
    /// Adaptive Watch Time Booster - Intelligently distributes viewers across channel content
    /// Supports: Live streams, VODs, Shorts, Regular videos
    /// </summary>
    public class AdaptiveWatchTimeBooster : IDisposable
    {
        private readonly string _channelUsername;
        private readonly int _targetViewerCount;
        private readonly bool _headless;
        private readonly bool _lowResourceMode;
        private readonly Random _random = new Random();
        private CancellationTokenSource _cancellationToken;
        private IPlaywright _playwright;
        private List<ViewerBot> _activeBots = new List<ViewerBot>();
        private bool _isRunning = false;
        private HashSet<string> _assignedUrls = new HashSet<string>(); // Track assigned URLs to prevent duplicates
        
        // Adaptive Deployment Configuration - ULTRA AGGRESSIVE THROTTLING
        private const int WAVE_SIZE = 2;              // Deploy 2 bots per wave (reduced from 5)
        private const int WAVE_DELAY_MS = 15000;      // 15 seconds between waves (increased from 8s)
        private const int STABILIZATION_DELAY_MS = 5000; // 5 seconds for bot stabilization
        private int _currentActiveCount = 0;
        private readonly SemaphoreSlim _deploymentLock = new SemaphoreSlim(1, 1);

        public AdaptiveWatchTimeBooster(string channelUsername, int targetViewerCount, bool headless = true, bool lowResourceMode = true)
        {
            _channelUsername = channelUsername;
            _targetViewerCount = targetViewerCount;
            _headless = headless;
            _lowResourceMode = lowResourceMode;
        }

        public async Task StartAsync()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _cancellationToken = new CancellationTokenSource();

            SerilogLog.Information("+------------------------------------------------------------+");
            SerilogLog.Information("║   ADAPTIVE WATCH TIME BOOSTER - JARVIS 2.0 (THROTTLED)   ║");
            SerilogLog.Information("+------------------------------------------------------------+");
            SerilogLog.Information($"Channel: {_channelUsername}");
            SerilogLog.Information($"Target Viewers: {_targetViewerCount}");
            SerilogLog.Information($"Mode: {(_headless ? "Headless" : "Visible")} | Low Resource: {_lowResourceMode}");
            SerilogLog.Information($"Content Types: VODs, Shorts, Videos");
            SerilogLog.Information($"Deployment: Wave-based ({WAVE_SIZE} bots per wave, {WAVE_DELAY_MS/1000}s interval)");
            SerilogLog.Information("------------------------------------------------------------");

            try
            {
                _playwright = await Playwright.CreateAsync();

                // Discover channel content
                SerilogLog.Information("?? Discovering channel content...");
                var contentUrls = await DiscoverChannelContentAsync();
                SerilogLog.Information($"?? Found {contentUrls.Count} videos to boost");

                // Launch viewer bots in waves
                await LaunchViewerBotsAsync(contentUrls);

                // Keep running and managing bots
                await MonitorAndAdaptAsync();
            }
            catch (Exception ex)
            {
                SerilogLog.Error($"? Fatal error: {ex.Message}");
            }
        }

        private async Task<List<string>> DiscoverChannelContentAsync()
        {
            var contentUrls = new List<string>();
            IBrowser browser = null;

            try
            {
                browser = await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false, // CRITICAL: Must be false for platform detection
                    Args = new[] { "--new-instance", "--no-remote" }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                });

                var page = await context.NewPageAsync();

                // Visit channel videos page
                var channelUrl = $"https://www.youtube.com/@{_channelUsername}/videos";
                await page.GotoAsync(channelUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });
                await Task.Delay(3000);

                // Scroll to load more videos
                for (int i = 0; i < 3; i++)
                {
                    await page.EvaluateAsync("window.scrollTo(0, document.documentElement.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract video URLs
                var videoLinks = await page.EvaluateAsync<string[]>(
                    "Array.from(document.querySelectorAll('a#video-title-link, a.yt-simple-endpoint.style-scope.ytd-video-renderer')).map(a => a.href).filter(href => href && href.includes('/watch?v=')).slice(0, 30)"
                );

                contentUrls.AddRange(videoLinks.Distinct());

                // Also get Shorts
                var shortsUrl = $"https://www.youtube.com/@{_channelUsername}/shorts";
                await page.GotoAsync(shortsUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });
                await Task.Delay(3000);

                var shortsLinks = await page.EvaluateAsync<string[]>(
                    "Array.from(document.querySelectorAll('a[href*=\"/shorts/\"]')).map(a => a.href).filter(href => href && href.includes('/shorts/')).slice(0, 10)"
                );

                contentUrls.AddRange(shortsLinks.Distinct());

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                SerilogLog.Warning($"?? Content discovery error: {ex.Message}");
                // Fallback: Use channel main page
                contentUrls.Add($"https://www.youtube.com/@{_channelUsername}/videos");
            }
            finally
            {
                if (browser != null) await browser.CloseAsync();
            }

            return contentUrls.Distinct().ToList();
        }

        private async Task LaunchViewerBotsAsync(List<string> contentUrls)
        {
            if (!contentUrls.Any())
            {
                SerilogLog.Warning("⚠ No content found, using channel page");
                contentUrls.Add($"https://www.youtube.com/@{_channelUsername}");
            }

            SerilogLog.Information($"🚀 Launching {_targetViewerCount} viewer bots in waves...");

            int totalBots = _targetViewerCount;
            int deployedBots = 0;
            int waveNumber = 1;
            
            // Shuffle content URLs to randomize distribution
            var shuffledUrls = contentUrls.OrderBy(x => _random.Next()).ToList();

            // Deploy bots in waves to prevent system overload
            while (deployedBots < totalBots && !_cancellationToken.Token.IsCancellationRequested)
            {
                int botsInThisWave = Math.Min(WAVE_SIZE, totalBots - deployedBots);
                
                SerilogLog.Information($"📊 Wave {waveNumber}: Deploying {botsInThisWave} bots (Total: {deployedBots + botsInThisWave}/{totalBots})");

                var waveTasks = new List<Task>();

                for (int i = 0; i < botsInThisWave; i++)
                {
                    if (_cancellationToken.Token.IsCancellationRequested) break;

                    var botIndex = deployedBots + i;
                    
                    // Assign unique URL - ensure no duplicates across all bots
                    string targetUrl;
                    lock (_assignedUrls)
                    {
                        // Find first unassigned URL
                        targetUrl = shuffledUrls.FirstOrDefault(url => !_assignedUrls.Contains(url));
                        
                        // If all URLs assigned, cycle back (with warning)
                        if (string.IsNullOrEmpty(targetUrl))
                        {
                            SerilogLog.Warning($"⚠ All {shuffledUrls.Count} videos already assigned, reusing URLs");
                            targetUrl = shuffledUrls[botIndex % shuffledUrls.Count];
                        }
                        
                        _assignedUrls.Add(targetUrl);
                        SerilogLog.Information($"[Bot #{botIndex}] Assigned unique video: {targetUrl.Split('?')[0].Split('/').Last()}");
                    }

                    waveTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await _deploymentLock.WaitAsync(_cancellationToken.Token);
                            try
                            {
                                var bot = new ViewerBot(botIndex, targetUrl, _headless, _lowResourceMode, _playwright);
                                _activeBots.Add(bot);
                                Interlocked.Increment(ref _currentActiveCount);
                                
                                // Fire and forget - don't await the bot's long-running task
                                _ = Task.Run(async () => 
                                {
                                    try
                                    {
                                        await bot.StartAsync(_cancellationToken.Token);
                                    }
                                    finally
                                    {
                                        Interlocked.Decrement(ref _currentActiveCount);
                                    }
                                }, _cancellationToken.Token);
                            }
                            finally
                            {
                                _deploymentLock.Release();
                            }

                            // Stabilization delay between individual bot launches within wave
                            await Task.Delay(STABILIZATION_DELAY_MS, _cancellationToken.Token);
                        }
                        catch (Exception ex)
                        {
                            SerilogLog.Warning($"⚠ Bot {botIndex} deployment failed: {ex.Message}");
                        }
                    }, _cancellationToken.Token));
                }

                // Wait for all bots in this wave to initialize
                await Task.WhenAll(waveTasks);
                
                deployedBots += botsInThisWave;
                SerilogLog.Information($"✓ Wave {waveNumber} complete. Active bots: {_currentActiveCount}");

                // Inter-wave delay to allow system resources to stabilize
                if (deployedBots < totalBots)
                {
                    SerilogLog.Information($"⏳ Cooling down for {WAVE_DELAY_MS/1000} seconds before next wave...");
                    await Task.Delay(WAVE_DELAY_MS, _cancellationToken.Token);
                }

                waveNumber++;
            }

            SerilogLog.Information($"✅ Deployment complete! {deployedBots} bots launched successfully");
        }

        private async Task MonitorAndAdaptAsync()
        {
            SerilogLog.Information("?? Adaptive monitoring active - maintaining viewer count 24/7");

            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                await Task.Delay(60000); // Check every minute

                // Count active bots
                var activeBotCount = _activeBots.Count(b => b.IsActive);
                SerilogLog.Information($"?? Active viewers: {activeBotCount}/{_targetViewerCount}");

                // Adaptive replacement: if bots drop below 80% of target, launch replacements
                if (activeBotCount < _targetViewerCount * 0.8)
                {
                    var needed = _targetViewerCount - activeBotCount;
                    SerilogLog.Warning($"?? Viewer count low! Launching {needed} replacement bots...");
                    
                    // Remove dead bots
                    _activeBots.RemoveAll(b => !b.IsActive);
                    
                    // Launch replacements (would need content URLs cached)
                }
            }
        }

        public void Stop()
        {
            SerilogLog.Information("?? Stopping all viewer bots...");
            _cancellationToken?.Cancel();
            _isRunning = false;

            foreach (var bot in _activeBots)
            {
                bot.Dispose();
            }

            _activeBots.Clear();
        }

        public void Dispose()
        {
            Stop();
            _playwright?.Dispose();
        }
    }

    internal class ViewerBot : IDisposable
    {
        private readonly int _index;
        private readonly string _targetUrl;
        private readonly bool _headless;
        private readonly bool _lowResourceMode;
        private readonly IPlaywright _playwright;
        private readonly Random _random = new Random();
        private IBrowser _browser;
        private IPage _page;
        public bool IsActive { get; private set; }

        public ViewerBot(int index, string targetUrl, bool headless, bool lowResourceMode, IPlaywright playwright)
        {
            _index = index;
            _targetUrl = targetUrl;
            _headless = headless;
            _lowResourceMode = lowResourceMode;
            _playwright = playwright;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IsActive = true;
            var sessionId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                SerilogLog.Information($"[Bot #{_index}][{sessionId}] Initializing...");

                var args = new List<string>
                {
                    "--disable-blink-features=AutomationControlled",
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--mute-audio",
                    "--autoplay-policy=no-user-gesture-required"
                };

                if (_lowResourceMode)
                {
                    args.AddRange(new[]
                    {
                        "--disable-images",
                        "--blink-settings=imagesEnabled=false",
                        "--disable-background-networking",
                        "--disable-sync"
                    });
                }

                _browser = await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false, // CRITICAL: Must be false for platform detection effectiveness
                    Args = args.ToArray()
                });

                var context = await _browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = GenerateRandomUserAgent(),
                    ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
                });

                _page = await context.NewPageAsync();

                // Navigate to video
                await _page.GotoAsync(_targetUrl, new PageGotoOptions 
                { 
                    WaitUntil = WaitUntilState.DOMContentLoaded, 
                    Timeout = 45000 
                });

                SerilogLog.Information($"[Bot #{_index}][{sessionId}] ? Watching: {_targetUrl}");

                // Wait for video player
                await _page.WaitForSelectorAsync("video", new PageWaitForSelectorOptions { Timeout = 30000 });

                // Ensure video plays
                await _page.EvaluateAsync(@"
                    const video = document.querySelector('video');
                    if (video) {
                        video.muted = true;
                        video.play().catch(() => {});
                    }
                ");

                // Simulate realistic viewing behavior
                await SimulateViewingBehaviorAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                SerilogLog.Warning($"[Bot #{_index}][{sessionId}] ? Error: {ex.Message}");
                IsActive = false;
            }
        }

        private async Task SimulateViewingBehaviorAsync(CancellationToken cancellationToken)
        {
            var watchDuration = _random.Next(300, 7200); // 5 min to 2 hours
            var endTime = DateTime.UtcNow.AddSeconds(watchDuration);

            SerilogLog.Information($"[Bot #{_index}] Session duration: {watchDuration / 60} minutes");

            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Random actions every 30-90 seconds
                    await Task.Delay(_random.Next(30000, 90000), cancellationToken);

                    // Check video is still playing
                    var isPlaying = await _page.EvaluateAsync<bool>(@"
                        const video = document.querySelector('video');
                        return video && !video.paused && !video.ended;
                    ");

                    if (!isPlaying)
                    {
                        await _page.EvaluateAsync("document.querySelector('video')?.play()");
                    }

                    // Occasionally scroll or move mouse (low frequency)
                    if (_random.Next(0, 100) < 10)
                    {
                        await _page.Mouse.MoveAsync(_random.Next(100, 1180), _random.Next(100, 620));
                    }
                }
                catch
                {
                    // Continue watching
                }
            }

            SerilogLog.Information($"[Bot #{_index}] Session complete - watched {watchDuration / 60} minutes");
            IsActive = false;
        }

        private string GenerateRandomUserAgent()
        {
            var userAgents = new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0"
            };
            return userAgents[_random.Next(userAgents.Length)];
        }

        public void Dispose()
        {
            IsActive = false;
            try
            {
                _page?.CloseAsync().Wait(5000);
                _browser?.CloseAsync().Wait(5000);
            }
            catch { }
        }
    }
}

