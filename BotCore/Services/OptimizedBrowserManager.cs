using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BotCore.Services
{
    /// <summary>
    /// Optimized browser manager with connection pooling and resource limits
    /// Prevents browser instance leaks and manages concurrent browser sessions
    /// </summary>
    public class OptimizedBrowserManager : IDisposable
    {
        private const int MAX_CONCURRENT_BROWSERS = 10;
        private const int BROWSER_TIMEOUT_MINUTES = 30;
        
        private readonly ILogger<OptimizedBrowserManager> _logger;
        private readonly SemaphoreSlim _browserSemaphore;
        private readonly ConcurrentDictionary<string, BrowserSession> _browsers;
        private readonly Timer _cleanupTimer;
        private readonly string _platform;
        private bool _disposed = false;

        public OptimizedBrowserManager(ILogger<OptimizedBrowserManager> logger, string platform = "Twitch")
        {
            _logger = logger;
            _platform = platform;
            _browserSemaphore = new SemaphoreSlim(MAX_CONCURRENT_BROWSERS);
            _browsers = new ConcurrentDictionary<string, BrowserSession>();
            
            // Cleanup stale browsers every 5 minutes
            _cleanupTimer = new Timer(CleanupStaleBrowsers, null,
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                
            _logger.LogInformation(
                "Browser manager initialized for {Platform} with max {MaxBrowsers} concurrent browsers",
                platform, MAX_CONCURRENT_BROWSERS);
        }

        public async Task<IBrowser> GetBrowserAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            await _browserSemaphore.WaitAsync(cancellationToken);
            
            try
            {
                if (_browsers.TryGetValue(sessionId, out var session))
                {
                    session.LastAccessed = DateTime.UtcNow;
                    _logger.LogDebug("Reusing existing browser for session {SessionId}", sessionId);
                    return session.Browser;
                }

                _logger.LogInformation("Creating new browser instance for session {SessionId}", sessionId);
                var newBrowser = await CreateOptimizedBrowserAsync();
                
                var browserSession = new BrowserSession
                {
                    Browser = newBrowser,
                    SessionId = sessionId,
                    Created = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow
                };
                
                _browsers[sessionId] = browserSession;
                
                _logger.LogInformation(
                    "Browser created for session {SessionId}. Active browsers: {ActiveCount}",
                    sessionId, _browsers.Count);
                
                return newBrowser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create browser for session {SessionId}", sessionId);
                throw;
            }
            finally
            {
                _browserSemaphore.Release();
            }
        }

        private async Task<IBrowser> CreateOptimizedBrowserAsync()
        {
            var playwright = await Playwright.CreateAsync();
            
            // Platform-specific browser selection
            var browserType = _platform.ToLower() switch
            {
                "youtube" => playwright.Firefox,
                "twitch" => playwright.Chromium,
                _ => playwright.Chromium
            };

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-web-security",
                    "--disable-features=VizDisplayCompositor",
                    "--disable-background-timer-throttling",
                    "--disable-renderer-backgrounding",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-ipc-flooding-protection",
                    "--enable-features=NetworkService,NetworkServiceInProcess",
                    "--disable-blink-features=AutomationControlled"
                }
            };

            _logger.LogDebug("Launching {BrowserType} browser with optimized settings", browserType.Name);
            return await browserType.LaunchAsync(launchOptions);
        }

        public async Task ReleaseBrowserAsync(string sessionId)
        {
            if (_browsers.TryRemove(sessionId, out var session))
            {
                try
                {
                    await session.Browser.CloseAsync();
                    _logger.LogInformation(
                        "Browser released for session {SessionId}. Active browsers: {ActiveCount}",
                        sessionId, _browsers.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing browser for session {SessionId}", sessionId);
                }
            }
        }

        private async void CleanupStaleBrowsers(object? state)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-BROWSER_TIMEOUT_MINUTES);
                var staleCount = 0;

                foreach (var kvp in _browsers)
                {
                    if (kvp.Value.LastAccessed < cutoffTime)
                    {
                        if (_browsers.TryRemove(kvp.Key, out var session))
                        {
                            try
                            {
                                await session.Browser.CloseAsync();
                                staleCount++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error closing stale browser {SessionId}", kvp.Key);
                            }
                        }
                    }
                }

                if (staleCount > 0)
                {
                    _logger.LogInformation(
                        "Cleaned up {StaleCount} stale browser(s). Active browsers: {ActiveCount}",
                        staleCount, _browsers.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during browser cleanup");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Disposing browser manager...");
            
            _cleanupTimer?.Dispose();
            
            foreach (var session in _browsers.Values)
            {
                try
                {
                    session.Browser.CloseAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing browser session {SessionId}", session.SessionId);
                }
            }
            
            _browsers.Clear();
            _browserSemaphore?.Dispose();
            
            _disposed = true;
            
            _logger.LogInformation("Browser manager disposed");
        }

        private class BrowserSession
        {
            public required IBrowser Browser { get; init; }
            public required string SessionId { get; init; }
            public required DateTime Created { get; init; }
            public DateTime LastAccessed { get; set; }
        }
    }
}

