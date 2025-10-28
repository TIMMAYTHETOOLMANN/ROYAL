using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace BotCore.YouTubeEngine
{
    public class YouTubeViewerEngine
    {
        private readonly ILogger<YouTubeViewerEngine> _logger;
        private readonly Random _random = new Random();

        public YouTubeViewerEngine(ILogger<YouTubeViewerEngine> logger)
        {
            _logger = logger;
        }

        public async Task InitializeYouTubeSession(IPage page)
        {
            try
            {
                _logger.LogInformation("🔄 Initializing YouTube viewer session...");
                
                // Navigate to videos page
                await page.GotoAsync("https://www.youtube.com/@timmaythetoolman/videos",
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                
                // Wait for page load
                await page.WaitForTimeoutAsync(5000);
                
                // Select and play a random video
                await PlayRandomVideo(page);
                
                // Ensure video playback
                await EnsureYouTubePlayback(page);
                
                _logger.LogInformation("✅ YouTube session initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ YouTube session initialization failed");
            }
        }

        private async Task PlayRandomVideo(IPage page)
        {
            try
            {
                // Click on a random video from the channel
                await page.ClickAsync("#video-title:first-child");
                await page.WaitForTimeoutAsync(5000);
                
                // Handle YouTube consent if present
                await HandleYouTubeConsent(page);
                
                _logger.LogInformation("✅ YouTube video playback initiated");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Video selection failed: {ex.Message}");
            }
        }

        private async Task HandleYouTubeConsent(IPage page)
        {
            try
            {
                // Handle YouTube consent popup
                if (await page.IsVisibleAsync("button[aria-label='Accept all']"))
                {
                    await page.ClickAsync("button[aria-label='Accept all']");
                    await page.WaitForTimeoutAsync(2000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"YouTube consent handling failed: {ex.Message}");
            }
        }

        private async Task EnsureYouTubePlayback(IPage page)
        {
            try
            {
                // Wait for video player
                await page.WaitForSelectorAsync("video", new PageWaitForSelectorOptions { 
                    State = WaitForSelectorState.Attached,
                    Timeout = 10000 
                });

                // Ensure video is playing
                await page.EvaluateAsync(@"
                    const video = document.querySelector('video');
                    if (video && video.paused) {
                        video.play().catch(e => console.log('YouTube auto-play attempted'));
                    }
                    
                    // Set reasonable volume
                    if (video) {
                        video.volume = 0.3;
                    }
                ");

                // Simulate YouTube engagement
                await SimulateYouTubeEngagement(page);
                
                _logger.LogInformation("✅ YouTube playback ensured");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"YouTube playback assurance failed: {ex.Message}");
            }
        }

        private async Task SimulateYouTubeEngagement(IPage page)
        {
            try
            {
                // Randomly like the video (10% chance)
                if (_random.Next(100) > 90)
                {
                    await page.ClickAsync("button[aria-label*='like']");
                    await page.WaitForTimeoutAsync(1000);
                }

                // Simulate watching related videos (after some time)
                if (_random.Next(100) > 80)
                {
                    await page.WaitForTimeoutAsync(30000); // Watch for 30 seconds first
                    await ClickRelatedVideo(page);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"YouTube engagement simulation failed: {ex.Message}");
            }
        }

        private async Task ClickRelatedVideo(IPage page)
        {
            try
            {
                // Click on a related video to simulate natural viewing
                await page.ClickAsync("ytd-compact-video-renderer:first-child a");
                await page.WaitForTimeoutAsync(5000);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Related video click failed: {ex.Message}");
            }
        }
    }
}

