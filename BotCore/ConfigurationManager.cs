using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogLog = Serilog.Log;

namespace BotCore
{
    /// <summary>
    /// Enhanced Configuration Manager with BOM handling and encoding safeguards
    /// </summary>
    public static class ConfigurationManager
    {
        private static IConfiguration _configuration;
        private static bool _isInitialized = false;

        public static IConfiguration Configuration
        {
            get
            {
                if (!_isInitialized)
                {
                    Initialize();
                }
                return _configuration;
            }
        }

        public static void Initialize(string environment = null)
        {
            if (_isInitialized) return;

            try
            {
                // Ensure UTF-8 encoding without BOM
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Console.OutputEncoding = Encoding.UTF8;

                environment ??= Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                             ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                             ?? "Production";

                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(GetConfigurationBasePath())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                _configuration = configBuilder.Build();
                _isInitialized = true;

                SerilogLog.Information("✓ Configuration initialized successfully for environment: {Environment}", environment);
            }
            catch (Exception ex)
            {
                SerilogLog.Error(ex, "✗ Failed to initialize configuration");
                throw;
            }
        }

        private static string GetConfigurationBasePath()
        {
            // Try multiple locations for configuration files
            var locations = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.GetDirectoryName(typeof(ConfigurationManager).Assembly.Location)
            };

            foreach (var location in locations)
            {
                if (File.Exists(Path.Combine(location, "appsettings.json")))
                {
                    return location;
                }
            }

            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Safely read configuration file with BOM detection and removal
        /// </summary>
        public static string ReadConfigFileWithoutBOM(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            // Read with BOM detection
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            
            // Remove BOM if present
            if (content.Length > 0 && content[0] == '\uFEFF')
            {
                content = content.Substring(1);
                SerilogLog.Warning("BOM detected and removed from {FilePath}", filePath);
            }

            return content;
        }

        /// <summary>
        /// Write configuration file without BOM
        /// </summary>
        public static void WriteConfigFileWithoutBOM(string filePath, string content)
        {
            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(filePath, content, utf8WithoutBom);
            SerilogLog.Information("Configuration file written without BOM: {FilePath}", filePath);
        }

        /// <summary>
        /// Get configuration value with fallback
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default)
        {
            try
            {
                var value = Configuration[key];
                if (string.IsNullOrEmpty(value))
                {
                    return defaultValue;
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get section with null safety
        /// </summary>
        public static IConfigurationSection GetSectionSafe(string key)
        {
            return Configuration.GetSection(key) ?? new ConfigurationBuilder().Build().GetSection(key);
        }

        /// <summary>
        /// Validate all required configuration keys are present
        /// </summary>
        public static bool ValidateConfiguration(out string[] missingKeys)
        {
            var required = new[]
            {
                "BotCore:Platform",
                "BotCore:ChannelUsername",
                "BotCore:ViewerCount"
            };

            var missing = new System.Collections.Generic.List<string>();

            foreach (var key in required)
            {
                if (string.IsNullOrEmpty(Configuration[key]))
                {
                    missing.Add(key);
                }
            }

            missingKeys = missing.ToArray();
            return missing.Count == 0;
        }

        /// <summary>
        /// Fix encoding issues in existing configuration files
        /// </summary>
        public static void FixEncodingIssues()
        {
            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Development.json",
                "appsettings.Production.json"
            };

            foreach (var file in configFiles)
            {
                var filePath = Path.Combine(GetConfigurationBasePath(), file);
                if (File.Exists(filePath))
                {
                    try
                    {
                        var content = ReadConfigFileWithoutBOM(filePath);
                        WriteConfigFileWithoutBOM(filePath, content);
                    }
                    catch (Exception ex)
                    {
                        SerilogLog.Warning(ex, "Could not fix encoding for {File}", file);
                    }
                }
            }
        }
    }
}

