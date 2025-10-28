using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    /// <summary>
    /// Enhanced startup with encoding safeguards and backward compatibility
    /// </summary>
    public static class EnhancedStartup
    {
        private static IHost _host;

        public static IHost BuildHost(string[] args = null)
        {
            // Fix encoding issues immediately
            SetupEncoding();

            // Initialize configuration with BOM handling
            ConfigurationManager.Initialize();
            ConfigurationManager.FixEncodingIssues();

            // Configure Serilog from configuration
            ConfigureSerilog();

            SerilogLog.Information("╔════════════════════════════════════════════════════════════╗");
            SerilogLog.Information("║          JARVIS 3.0 - ENHANCED STARTUP SEQUENCE           ║");
            SerilogLog.Information("╚════════════════════════════════════════════════════════════╝");

            try
            {
                _host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(AppContext.BaseDirectory)
                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                                          optional: true, reloadOnChange: true)
                              .AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services, context.Configuration);
                    })
                    .UseSerilog()
                    .Build();

                SerilogLog.Information("✓ Host built successfully");
                return _host;
            }
            catch (Exception ex)
            {
                SerilogLog.Fatal(ex, "✗ Failed to build host");
                throw;
            }
        }

        private static void SetupEncoding()
        {
            try
            {
                // Register code pages encoding provider for full encoding support
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Set console encoding to UTF-8 without BOM
                Console.OutputEncoding = new UTF8Encoding(false);
                Console.InputEncoding = new UTF8Encoding(false);

                // Set environment variables for .NET runtime
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

                SerilogLog.Debug("✓ Encoding configured: UTF-8 without BOM");
            }
            catch (Exception ex)
            {
                SerilogLog.Warning(ex, "⚠ Could not fully configure encoding, using defaults");
            }
        }

        private static void ConfigureSerilog()
        {
            var configuration = ConfigurationManager.Configuration;

            SerilogLog.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "BotCore")
                .Enrich.WithProperty("Version", "3.0")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(
                    path: "logs/botcore-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    encoding: new UTF8Encoding(false), // UTF-8 without BOM
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            SerilogLog.Information("✓ Serilog configured successfully");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration sections
            services.Configure<BotCoreConfiguration>(configuration.GetSection("BotCore"));
            services.Configure<MonitoringConfiguration>(configuration.GetSection("Monitoring"));
            services.Configure<ResilienceConfiguration>(configuration.GetSection("Resilience"));
            services.Configure<RedisConfiguration>(configuration.GetSection("Redis"));
            services.Configure<AdvancedFeaturesConfiguration>(configuration.GetSection("AdvancedFeatures"));

            // Register core services
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<Core>();

            // Add health checks if enabled
            var monitoringConfig = configuration.GetSection("Monitoring").Get<MonitoringConfiguration>();
            if (monitoringConfig?.EnablePrometheus ?? false)
            {
                services.AddHealthChecks()
                    .AddCheck<BotCoreHealthCheck>("botcore_health");
            }

            // Add memory cache
            services.AddMemoryCache();

            // Add distributed cache if Redis is enabled
            var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
            if (redisConfig?.EnableCache ?? false)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfig.ConnectionString;
                });
            }

            SerilogLog.Information("✓ Services configured successfully");
        }

        public static BotCoreConfiguration GetBotConfiguration()
        {
            var config = ConfigurationManager.Configuration.GetSection("BotCore").Get<BotCoreConfiguration>()
                        ?? new BotCoreConfiguration();
            
            // Merge environment variables (Docker/Kubernetes support)
            config.MergeEnvironmentVariables();

            // Validate configuration
            var (isValid, errors) = config.Validate();
            if (!isValid)
            {
                SerilogLog.Warning("⚠ Configuration validation warnings:");
                foreach (var error in errors)
                {
                    SerilogLog.Warning($"  - {error}");
                }
            }

            return config;
        }

        public static MonitoringConfiguration GetMonitoringConfiguration()
        {
            return ConfigurationManager.Configuration.GetSection("Monitoring").Get<MonitoringConfiguration>()
                   ?? new MonitoringConfiguration();
        }

        public static ResilienceConfiguration GetResilienceConfiguration()
        {
            return ConfigurationManager.Configuration.GetSection("Resilience").Get<ResilienceConfiguration>()
                   ?? new ResilienceConfiguration();
        }

        /// <summary>
        /// Backward compatibility: Create ExecuteNeedsDto from configuration
        /// </summary>
        public static Dto.ExecuteNeedsDto CreateExecuteNeedsDto()
        {
            var config = GetBotConfiguration();
            return config.ToExecuteNeedsDto();
        }

        /// <summary>
        /// Perform pre-flight checks before starting
        /// </summary>
        public static bool PreFlightCheck()
        {
            SerilogLog.Information("🔍 Running pre-flight checks...");

            try
            {
                // Check configuration validity
                if (!ConfigurationManager.ValidateConfiguration(out var missingKeys))
                {
                    SerilogLog.Error("✗ Configuration validation failed. Missing keys:");
                    foreach (var key in missingKeys)
                    {
                        SerilogLog.Error($"  - {key}");
                    }
                    return false;
                }

                // Check encoding setup
                if (Console.OutputEncoding.WebName != "utf-8")
                {
                    SerilogLog.Warning("⚠ Console encoding is not UTF-8: {Encoding}", Console.OutputEncoding.WebName);
                }

                // Check proxy file if specified
                var config = GetBotConfiguration();
                if (!string.IsNullOrEmpty(config.ProxyFilePath) && !System.IO.File.Exists(config.ProxyFilePath))
                {
                    SerilogLog.Warning("⚠ Proxy file not found: {FilePath}", config.ProxyFilePath);
                }

                SerilogLog.Information("✓ Pre-flight checks passed");
                return true;
            }
            catch (Exception ex)
            {
                SerilogLog.Error(ex, "✗ Pre-flight check failed");
                return false;
            }
        }
    }
}

