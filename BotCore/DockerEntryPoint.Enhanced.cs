using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Microsoft.Playwright;
using Polly;
using Prometheus;
using SerilogLog = Serilog.Log;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BotCore
{
    /// <summary>
    /// Enhanced Docker Entry Point with sophisticated enterprise-level deployment capabilities
    /// </summary>
    public class DockerEntryPointEnhanced
    {
        public static async Task Main(string[] args)
        {
            // Build configuration first
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // Setup Serilog with advanced structured logging
            SerilogLog.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Application", "BotCore")
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Async(a => a.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
                .WriteTo.Async(a => a.File(
                    path: "logs/botcore-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
                .CreateLogger();

            try
            {
                SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
                SerilogLog.Information("║   JARVIS 2.0 - SOPHISTICATED DOCKER DEPLOYMENT ENGINE     ║");
                SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");
                SerilogLog.Information("Starting BotCore Enhanced Application");

                var host = CreateHostBuilder(args, configuration).Build();

                // Health check startup validation
                await ValidateHealthOnStartup(host.Services);

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                SerilogLog.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                SerilogLog.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    // Configuration
                    services.AddSingleton(configuration);

                    // Health Checks with custom checks
                    services.AddHealthChecks()
                        .AddCheck<BrowserHealthCheck>("browser-health")
                        .AddCheck<MemoryHealthCheck>("memory-health")
                        .AddCheck<SystemResourcesHealthCheck>("system-resources");

                    // Hosted Services
                    services.AddHostedService<BrowserPoolService>();
                    services.AddHostedService<MetricsService>();

                    // Playwright Factory
                    services.AddSingleton<IPlaywrightFactory, PlaywrightFactory>();

                    // Browser Manager with resilience
                    services.AddSingleton<IBrowserManager, ResilientBrowserManager>();

                    // Distributed Cache (Redis - optional, configure via appsettings)
                    var redisConnection = configuration.GetConnectionString("Redis");
                    if (!string.IsNullOrEmpty(redisConnection))
                    {
                        services.AddStackExchangeRedisCache(options =>
                        {
                            options.Configuration = redisConnection;
                        });
                    }

                    // HTTP Client with Polly resilience policies
                    services.AddHttpClient("external-api")
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());

                    // Message Bus (RabbitMQ - optional)
                    var rabbitMqHost = configuration.GetValue<string>("RabbitMQ:Host");
                    if (!string.IsNullOrEmpty(rabbitMqHost))
                    {
                        services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
                    }
                });

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        SerilogLog.Warning($"Retry {retryCount} after {timespan} seconds due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (result, duration) =>
                    {
                        SerilogLog.Warning($"Circuit breaker opened for {duration.TotalSeconds}s");
                    },
                    onReset: () =>
                    {
                        SerilogLog.Information("Circuit breaker reset");
                    });
        }

        private static async Task ValidateHealthOnStartup(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

            var result = await healthCheckService.CheckHealthAsync();
            if (result.Status != HealthStatus.Healthy)
            {
                SerilogLog.Error("Health check failed on startup: {Status}", result.Status);
                foreach (var entry in result.Entries)
                {
                    if (entry.Value.Status != HealthStatus.Healthy)
                    {
                        SerilogLog.Error("  {Key}: {Status} - {Description}", entry.Key, entry.Value.Status, entry.Value.Description);
                    }
                }
                throw new InvalidOperationException("Application failed health check on startup");
            }

            SerilogLog.Information("✓ All health checks passed");
        }
    }

    // ========================================================================
    // PLAYWRIGHT FACTORY
    // ========================================================================
    public interface IPlaywrightFactory
    {
        Task<IPlaywright> CreateAsync();
    }

    public class PlaywrightFactory : IPlaywrightFactory
    {
        private IPlaywright? _playwright;

        public async Task<IPlaywright> CreateAsync()
        {
            if (_playwright == null)
            {
                _playwright = await Playwright.CreateAsync();
                SerilogLog.Information("Playwright instance created");
            }
            return _playwright;
        }
    }

    // ========================================================================
    // RESILIENT BROWSER MANAGER
    // ========================================================================
    public interface IBrowserManager
    {
        Task<IBrowser> GetBrowserAsync();
        Task ReturnBrowserAsync(IBrowser browser);
        Task HealthCheckAsync();
    }

    public class ResilientBrowserManager : IBrowserManager
    {
        private readonly IPlaywrightFactory _playwrightFactory;
        private readonly ILogger<ResilientBrowserManager> _logger;
        private readonly IAsyncPolicy _resiliencePolicy;
        private IBrowser? _browser;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public ResilientBrowserManager(IPlaywrightFactory playwrightFactory, ILogger<ResilientBrowserManager> logger)
        {
            _playwrightFactory = playwrightFactory;
            _logger = logger;

            _resiliencePolicy = Policy
                .Handle<PlaywrightException>()
                .Or<TimeoutException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Browser operation retry {RetryCount} after {TimeSpan} seconds", retryCount, timeSpan);
                    });
        }

        public async Task<IBrowser> GetBrowserAsync()
        {
            return await _resiliencePolicy.ExecuteAsync(async () =>
            {
                await _lock.WaitAsync();
                try
                {
                    if (_browser == null || !_browser.IsConnected)
                    {
                        await InitializeBrowserAsync();
                    }
                    return _browser!;
                }
                finally
                {
                    _lock.Release();
                }
            });
        }

        private async Task InitializeBrowserAsync()
        {
            var playwright = await _playwrightFactory.CreateAsync();
            
            var headless = Environment.GetEnvironmentVariable("HEADLESS") != "false";
            
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args = new[] {
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-web-security",
                    "--disable-features=VizDisplayCompositor",
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-renderer-backgrounding",
                    "--disable-blink-features=AutomationControlled"
                }
            });

            _logger.LogInformation("Browser initialized successfully (Headless: {Headless})", headless);
        }

        public async Task ReturnBrowserAsync(IBrowser browser)
        {
            // Browser pooling logic - for now, just keep it alive
            await Task.CompletedTask;
        }

        public async Task HealthCheckAsync()
        {
            try
            {
                var browser = await GetBrowserAsync();
                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();

                // Simple health check by navigating to about:blank
                await page.GotoAsync("about:blank", new PageGotoOptions { Timeout = 10000 });

                await page.CloseAsync();
                await context.CloseAsync();

                _logger.LogDebug("Browser health check passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Browser health check failed");
                throw;
            }
        }
    }

    // ========================================================================
    // HEALTH CHECK IMPLEMENTATIONS
    // ========================================================================
    public class BrowserHealthCheck : IHealthCheck
    {
        private readonly IBrowserManager _browserManager;

        public BrowserHealthCheck(IBrowserManager browserManager)
        {
            _browserManager = browserManager;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _browserManager.HealthCheckAsync();
                return HealthCheckResult.Healthy("Browser is operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Browser health check failed", ex);
            }
        }
    }

    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var memoryInfo = GC.GetGCMemoryInfo();
            var totalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024;
            var gen2Collections = GC.CollectionCount(2);

            var data = new Dictionary<string, object>
            {
                { "MemoryMB", totalMemoryMB },
                { "Gen2Collections", gen2Collections },
                { "HeapSizeMB", memoryInfo.HeapSizeBytes / 1024 / 1024 }
            };

            if (totalMemoryMB > 1000) // Alert if using more than 1GB
            {
                return Task.FromResult(HealthCheckResult.Degraded($"High memory usage: {totalMemoryMB}MB", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy($"Memory OK: {totalMemoryMB}MB", data: data));
        }
    }

    public class SystemResourcesHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var cpuUsage = Environment.ProcessorCount;
                var threadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
                var handleCount = System.Diagnostics.Process.GetCurrentProcess().HandleCount;

                var data = new Dictionary<string, object>
                {
                    { "ProcessorCount", cpuUsage },
                    { "ThreadCount", threadCount },
                    { "HandleCount", handleCount }
                };

                if (threadCount > 200)
                {
                    return Task.FromResult(HealthCheckResult.Degraded($"High thread count: {threadCount}", data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("System resources OK", data: data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("System resource check failed", ex));
            }
        }
    }

    // ========================================================================
    // HOSTED SERVICES
    // ========================================================================
    public class BrowserPoolService : BackgroundService
    {
        private readonly ILogger<BrowserPoolService> _logger;
        private readonly IBrowserManager _browserManager;

        public BrowserPoolService(ILogger<BrowserPoolService> logger, IBrowserManager browserManager)
        {
            _logger = logger;
            _browserManager = browserManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Browser Pool Service started");

            // Pre-warm browser pool
            try
            {
                await _browserManager.GetBrowserAsync();
                _logger.LogInformation("Browser pool pre-warmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to pre-warm browser pool");
            }

            // Periodic health checks
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                try
                {
                    await _browserManager.HealthCheckAsync();
                    _logger.LogDebug("Browser pool health check passed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Browser pool health check failed");
                }
            }
        }
    }

    public class MetricsService : BackgroundService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly MetricServer _metricServer;

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
            _metricServer = new MetricServer(port: 5000);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Prometheus metrics server on port 5000");
            _metricServer.Start();

            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Stopping metrics server");
                _metricServer.Stop();
            });

            return Task.CompletedTask;
        }
    }

    // ========================================================================
    // MESSAGE BUS INTERFACE (Optional)
    // ========================================================================
    public interface IMessageBus
    {
        Task PublishAsync<T>(string topic, T message);
        Task SubscribeAsync<T>(string topic, Func<T, Task> handler);
    }

    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly ILogger<RabbitMQMessageBus> _logger;

        public RabbitMQMessageBus(ILogger<RabbitMQMessageBus> logger, IConfiguration configuration)
        {
            _logger = logger;
            // Initialize RabbitMQ connection
            _logger.LogInformation("RabbitMQ Message Bus initialized");
        }

        public Task PublishAsync<T>(string topic, T message)
        {
            // Implement RabbitMQ publish
            _logger.LogDebug("Publishing message to {Topic}", topic);
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
        {
            // Implement RabbitMQ subscribe
            _logger.LogDebug("Subscribing to {Topic}", topic);
            return Task.CompletedTask;
        }
    }
}

