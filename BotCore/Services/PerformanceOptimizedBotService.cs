using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotCore.Services
{
    /// <summary>
    /// Performance-optimized bot service with resource management
    /// Provides automatic cleanup, caching, and memory optimization
    /// </summary>
    public class PerformanceOptimizedBotService : IDisposable
    {
        private readonly ILogger<PerformanceOptimizedBotService> _logger;
        private readonly IMemoryCache _cache;
        private readonly Timer _cleanupTimer;
        private readonly Timer _healthCheckTimer;
        private bool _disposed = false;

        public PerformanceOptimizedBotService(ILogger<PerformanceOptimizedBotService> logger)
        {
            _logger = logger;
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024,
                CompactionPercentage = 0.25
            });
            
            // Periodic cleanup every 5 minutes
            _cleanupTimer = new Timer(CleanupResources, null, 
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            
            // Health check every 30 seconds
            _healthCheckTimer = new Timer(PerformHealthCheck, null,
                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
                
            _logger.LogInformation("Performance optimization service initialized");
        }

        private void CleanupResources(object? state)
        {
            try
            {
                _logger.LogInformation("Starting performance cleanup cycle...");
                
                // Get memory before cleanup
                var memoryBefore = GC.GetTotalMemory(false);
                
                // Force garbage collection
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                
                // Clear expired cache entries
                _cache.Compact(0.25);
                
                var memoryAfter = GC.GetTotalMemory(false);
                var freedMemory = (memoryBefore - memoryAfter) / 1024.0 / 1024.0;
                
                _logger.LogInformation(
                    "Performance cleanup completed. Freed {FreedMB:F2} MB of memory",
                    freedMemory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Performance cleanup encountered an error");
            }
        }

        private void PerformHealthCheck(object? state)
        {
            try
            {
                var memoryUsed = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
                var gen0 = GC.CollectionCount(0);
                var gen1 = GC.CollectionCount(1);
                var gen2 = GC.CollectionCount(2);
                
                _logger.LogDebug(
                    "Health Check - Memory: {MemoryMB:F2} MB, GC(0/1/2): {Gen0}/{Gen1}/{Gen2}",
                    memoryUsed, gen0, gen1, gen2);
                
                // Alert if memory usage is too high
                if (memoryUsed > 400)
                {
                    _logger.LogWarning(
                        "High memory usage detected: {MemoryMB:F2} MB. Triggering cleanup...",
                        memoryUsed);
                    CleanupResources(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed");
            }
        }

        public T? GetOrCreateCached<T>(string key, Func<T> factory, TimeSpan expiration)
        {
            return _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expiration;
                entry.Size = 1;
                return factory();
            });
        }

        public async Task<T?> GetOrCreateCachedAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expiration;
                entry.Size = 1;
                return await factory();
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _logger.LogInformation("Disposing performance optimization service...");
            
            _cleanupTimer?.Dispose();
            _healthCheckTimer?.Dispose();
            _cache?.Dispose();
            
            _disposed = true;
            
            _logger.LogInformation("Performance optimization service disposed");
        }
    }
}

