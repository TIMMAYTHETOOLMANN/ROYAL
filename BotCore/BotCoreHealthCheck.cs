using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BotCore
{
    /// <summary>
    /// Health check for BotCore operations
    /// </summary>
    public class BotCoreHealthCheck : IHealthCheck
    {
        private readonly ILogger<BotCoreHealthCheck> _logger;

        public BotCoreHealthCheck(ILogger<BotCoreHealthCheck> logger = null)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if configuration is valid
                if (!ConfigurationManager.ValidateConfiguration(out var missingKeys))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        $"Configuration invalid: {string.Join(", ", missingKeys)}"));
                }

                // Check encoding
                if (Console.OutputEncoding.WebName != "utf-8")
                {
                    _logger?.LogWarning("Encoding is not UTF-8: {Encoding}", Console.OutputEncoding.WebName);
                }

                return Task.FromResult(HealthCheckResult.Healthy("BotCore is operational"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "BotCore health check failed", ex));
            }
        }
    }
}

