using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using BotCore.TwitchEngine;
using BotCore.YouTubeEngine;
using BotCore.BehaviorEngine;

namespace BotCore
{
    public class EnterpriseBotService
    {
        private readonly ILogger<EnterpriseBotService> _logger;
        private readonly TwitchAuthBypassEngine _twitchEngine;
        private readonly YouTubeViewerEngine _youtubeEngine;
        private readonly AdvancedViewerBehaviorEngine _behaviorEngine;
        
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private string _currentPlatform;

        public EnterpriseBotService(
            ILogger<EnterpriseBotService> logger,
            TwitchAuthBypassEngine twitchEngine,
            YouTubeViewerEngine youtubeEngine,
            AdvancedViewerBehaviorEngine behaviorEngine)
        {
            _logger = logger;
            _twitchEngine = twitchEngine;
            _youtubeEngine = youtubeEngine;
            _behaviorEngine = behaviorEngine;
        }

        public async Task StartAsync()
        {
            try
            {
                _logger.LogInformation("🚀 STARTING ENTERPRISE BOT SERVICE - TWITCH & YOUTUBE");
                
                _currentPlatform = Environment.GetEnvironmentVariable("PLATFORM")?.ToLower() ?? "youtube";
                
                await InitializeTrueVisualBrowser();
                await NavigateToPlatform();
                
                _behaviorEngine.SetBrowserPage(_page);
                
                // Platform-specific initialization
                if (_currentPlatform == "twitch")
                {
                    await InitializeTwitchSession();
                }
                else
                {
                    await InitializeYouTubeSession();
                }
                
                _logger.LogInformation($"✅ ENTERPRISE BOT SERVICE ACTIVE - PLATFORM: {_currentPlatform.ToUpper()}");
                
                // Keep service running indefinitely
                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ENTERPRISE BOT SERVICE FAILED");
                throw;
            }
        }

        private async Task InitializeTrueVisualBrowser()
        {
            _playwright = await Playwright.CreateAsync();
            
            // TRUE NON-HEADLESS BROWSER - CRITICAL FOR PLATFORM REGISTRATION
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // MUST BE FALSE FOR VISUAL REGISTRATION
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-web-security",
                    "--autoplay-policy=no-user-gesture-required",
                    "--disable-features=VizDisplayCompositor",
                    "--enable-features=NetworkService,NetworkServiceInProcess",
                    "--disable-background-timer-throttling",
                    "--disable-renderer-backgrounding",
                    "--disable-ipc-flooding-protection",
                    "--enable-logging",
                    "--v=1",
                    "--window-size=1920,1080",
                    "--start-maximized"
                }
            });

            // Create page with realistic viewport and settings
            _page = await _browser.NewPageAsync();
            await _page.SetViewportSizeAsync(new ViewportSize { Width = 1920, Height = 1080 });

            // Set realistic user agent for platform detection evasion
            await _page.SetExtraHTTPHeadersAsync(new System.Collections.Generic.Dictionary<string, string>
            {
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"},
                {"Accept-Language", "en-US,en;q=0.9"},
                {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"}
            });

            // Enable realistic browser features
            await _page.AddInitScriptAsync(@"
                Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });
                Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });
            ");

            _logger.LogInformation("✅ TRUE VISUAL BROWSER INITIALIZED");
        }

        private async Task NavigateToPlatform()
        {
            var url = _currentPlatform == "twitch" 
                ? "https://www.twitch.tv/timmaythetoolman" 
                : "https://www.youtube.com/@timmaythetoolman/videos";

            await _page.GotoAsync(url, new PageGotoOptions { 
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60000 
            });
            
            _logger.LogInformation($"✅ NAVIGATED TO {_currentPlatform.ToUpper()}: {url}");
        }

        private async Task InitializeTwitchSession()
        {
            _logger.LogInformation("🔄 INITIALIZING TWITCH SESSION WITH AUTH BYPASS");
            
            // Bypass Twitch authentication
            var authSuccess = await _twitchEngine.BypassAuthAndWatchStream(_page);
            
            if (!authSuccess)
            {
                _logger.LogWarning("⚠️ Twitch auth bypass failed, attempting fallback...");
                await FallbackTwitchAccess(_page);
            }

            // Initialize Twitch-specific behaviors
            _behaviorEngine.SetPlatformBehaviors(new System.Collections.Generic.List<System.Action>
            {
                async () => await _twitchEngine.SimulateTwitchViewerBehavior(_page),
                async () => await SimulateTwitchChatPresence(),
                async () => await SimulateTwitchStreamInteraction()
            });

            _logger.LogInformation("✅ TWITCH SESSION INITIALIZED - AUTH BYPASS ACTIVE");
        }

        private async Task InitializeYouTubeSession()
        {
            _logger.LogInformation("🔄 INITIALIZING YOUTUBE SESSION");
            
            await _youtubeEngine.InitializeYouTubeSession(_page);
            
            _logger.LogInformation("✅ YOUTUBE SESSION INITIALIZED");
        }

        private async Task FallbackTwitchAccess(IPage page)
        {
            try
            {
                // Alternative Twitch access methods
                await page.GotoAsync("https://www.twitch.tv/timmaythetoolman/videos");
                await page.WaitForTimeoutAsync(5000);
                
                // Try to play a recent video instead of live stream
                await page.ClickAsync("a[data-a-target=preview-card-thumbnail]:first-child");
                await page.WaitForTimeoutAsync(3000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Twitch fallback access failed");
            }
        }

        private async Task SimulateTwitchChatPresence()
        {
            // Simulate reading chat without authentication
            await _page.EvaluateAsync(@"
                const chatMessages = document.querySelectorAll('[data-test-selector=chat-message]');
                if (chatMessages.length > 0) {
                    const randomMessage = chatMessages[Math.floor(Math.random() * chatMessages.length)];
                    randomMessage.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            ");
        }

        private async Task SimulateTwitchStreamInteraction()
        {
            // Simulate stream interactions without auth
            await _page.Keyboard.PressAsync("m"); // Toggle mute
            await _page.WaitForTimeoutAsync(1000);
            await _page.Keyboard.PressAsync("m"); // Toggle back
        }

        public async Task StopAsync()
        {
            if (_browser != null)
                await _browser.CloseAsync();
            
            if (_playwright != null)
                _playwright.Dispose();
            
            _logger.LogInformation("🛑 ENTERPRISE BOT SERVICE STOPPED");
        }
    }
}

