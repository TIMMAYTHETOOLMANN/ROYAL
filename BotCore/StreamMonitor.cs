using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace BotCore
{
    public class StreamMonitor
    {
        private readonly HttpClient _httpClient;
        private bool _isLive = false;
        private DateTime _lastCheck = DateTime.MinValue;
        
        public event Action<string> StreamWentLive;
        public event Action StreamWentOffline;
        public event Action<string> StatusUpdate;
        
        public bool IsLive => _isLive;
        
        public StreamMonitor()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        
        public async Task<bool> CheckTrovoStreamAsync(string streamUrl)
        {
            try
            {
                if (!streamUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !streamUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    streamUrl = "https://" + streamUrl;
                }
                
                var uri = new Uri(streamUrl);
                var channelName = uri.AbsolutePath.TrimStart('/').Split('/')[0].ToLower();
                
                if (string.IsNullOrEmpty(channelName))
                {
                    channelName = uri.Host.Replace("www.", "").Replace("trovo.live", "").Trim('.', '/');
                }
                
                if (string.IsNullOrEmpty(channelName))
                    return false;
                
                StatusUpdate?.Invoke($"[MONITOR] Checking Trovo channel: {channelName}");
                StatusUpdate?.Invoke($"[MONITOR] Deep browser scan for Trovo...");
                
                return await CheckWithPlaywrightAsync(streamUrl, channelName);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke($"[MONITOR] Error: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> CheckWithPlaywrightAsync(string streamUrl, string channelName)
        {
            IPlaywright playwright = null;
            IBrowser browser = null;
            
            try
            {
                playwright = await Playwright.CreateAsync();
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-blink-features=AutomationControlled" },
                    Channel = "chrome"
                });
                
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                });
                
                var page = await context.NewPageAsync();
                await page.GotoAsync(streamUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 45000 });
                
                await Task.Delay(4000);
                
                // Trovo-specific live detection
                var isLive = await page.EvaluateAsync<bool>(@"
                    () => {
                        const video = document.querySelector('video');
                        
                        if (video && video.src) {
                            return true;
                        }
                        
                        if (video && !video.paused && video.readyState >= 2) {
                            return true;
                        }
                        
                        const bodyText = document.body.innerText || '';
                        if ((bodyText.includes('LIVE') || bodyText.includes('watching')) && video) {
                            return true;
                        }
                        
                        return false;
                    }
                ");
                
                await context.CloseAsync();
                
                StatusUpdate?.Invoke($"[MONITOR] Result: {(isLive ? "🔴 LIVE" : "⚫ OFFLINE")}");
                return isLive;
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke($"[MONITOR] Detection error: {ex.Message}");
                return false;
            }
            finally
            {
                if (browser != null) await browser.CloseAsync();
                playwright?.Dispose();
            }
        }
        
        public async Task MonitorContinuouslyAsync(string streamUrl, int checkIntervalMinutes, CancellationToken cancellationToken)
        {
            StatusUpdate?.Invoke($"[MONITOR] Starting Trovo monitoring (check every {checkIntervalMinutes} minutes)");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentStatus = await CheckTrovoStreamAsync(streamUrl);
                    
                    if (currentStatus && !_isLive)
                    {
                        _isLive = true;
                        StatusUpdate?.Invoke($"[ALERT] 🔴 TROVO STREAM LIVE! Deploying...");
                        StreamWentLive?.Invoke(streamUrl);
                    }
                    else if (!currentStatus && _isLive)
                    {
                        _isLive = false;
                        StatusUpdate?.Invoke($"[ALERT] ⚫ Stream offline");
                        StreamWentOffline?.Invoke();
                    }
                    
                    _lastCheck = DateTime.Now;
                    var nextCheck = DateTime.Now.AddMinutes(checkIntervalMinutes);
                    StatusUpdate?.Invoke($"[MONITOR] Next check: {nextCheck:HH:mm:ss}");
                    
                    await Task.Delay(TimeSpan.FromMinutes(checkIntervalMinutes), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke($"[MONITOR] Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }
            
            StatusUpdate?.Invoke("[MONITOR] Monitoring stopped");
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

