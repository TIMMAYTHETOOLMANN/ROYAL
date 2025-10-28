﻿using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotCore.Services
{
    /// <summary>
    /// Resilient stream watcher with automatic retry and error recovery
    /// Implements exponential backoff and circuit breaker patterns
    /// </summary>
    public class ResilientStreamWatcher
    {
        private readonly ILogger<ResilientStreamWatcher> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseRetryDelay;

        public ResilientStreamWatcher(ILogger<ResilientStreamWatcher> logger, int maxRetries = 3)
        {
            _logger = logger;
            _maxRetries = maxRetries;
            _baseRetryDelay = TimeSpan.FromSeconds(30);

            // Configure retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>(ex => !(ex is OperationCanceledException))
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt) * 15),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        var streamUrl = context.GetValueOrDefault("StreamUrl", "Unknown");
                        _logger.LogWarning(
                            exception,
                            "Retry {RetryCount}/{MaxRetries} for stream {StreamUrl}. Waiting {Seconds}s before next attempt",
                            retryCount, _maxRetries, streamUrl, timeSpan.TotalSeconds);
                    });
        }

        public async Task<bool> WatchStreamWithRetryAsync(
            string streamUrl,
            Func<string, CancellationToken, Task> watchStreamFunc,
            CancellationToken cancellationToken = default)
        {
            var context = new Context { { "StreamUrl", streamUrl } };

            try
            {
                _logger.LogInformation("Starting resilient stream watch for {StreamUrl}", streamUrl);

                await _retryPolicy.ExecuteAsync(
                    async (ctx, ct) =>
                    {
                        await watchStreamFunc(streamUrl, ct);
                    },
                    context,
                    cancellationToken);

                _logger.LogInformation("Successfully completed watching stream {StreamUrl}", streamUrl);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stream watch cancelled for {StreamUrl}", streamUrl);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "All retry attempts exhausted for stream {StreamUrl} after {MaxRetries} attempts",
                    streamUrl, _maxRetries);
                return false;
            }
        }

        public async Task<TResult?> ExecuteWithRetryAsync<TResult>(
            Func<Task<TResult>> operation,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            var context = new Context { { "OperationName", operationName } };

            try
            {
                _logger.LogDebug("Executing operation {OperationName} with retry", operationName);

                var result = await _retryPolicy.ExecuteAsync(
                    async (ctx, ct) => await operation(),
                    context,
                    cancellationToken);

                _logger.LogDebug("Operation {OperationName} completed successfully", operationName);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation {OperationName} was cancelled", operationName);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Operation {OperationName} failed after {MaxRetries} attempts",
                    operationName, _maxRetries);
                return default;
            }
        }

        public async Task<bool> ExecuteWithCircuitBreakerAsync(
            Func<CancellationToken, Task> operation,
            string operationName,
            int failureThreshold = 5,
            TimeSpan circuitBreakDuration = default,
            CancellationToken cancellationToken = default)
        {
            if (circuitBreakDuration == default)
                circuitBreakDuration = TimeSpan.FromMinutes(5);

            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: failureThreshold,
                    durationOfBreak: circuitBreakDuration,
                    onBreak: (exception, duration) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Circuit breaker opened for {OperationName}. Duration: {DurationSeconds}s",
                            operationName, duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation(
                            "Circuit breaker reset for {OperationName}",
                            operationName);
                    });

            try
            {
                await circuitBreakerPolicy.ExecuteAsync(
                    async ct => await operation(ct),
                    cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Circuit breaker execution failed for {OperationName}", operationName);
                return false;
            }
        }
    }
}

echo ""
echo "✅ Twitch Bot deployed successfully!"
echo "========================================="
echo "📊 Monitoring Commands:"
echo "   Logs:    docker logs -f botcore-twitch"
echo "   Stats:   docker stats botcore-twitch"
echo "   Health:  curl http://localhost:8080/health"
echo "========================================="
echo ""

# Wait for startup
sleep 5

# Check if container is running
if docker ps | grep -q botcore-twitch; then
    echo "✅ Container is running"
    docker logs --tail 20 botcore-twitch
else
    echo "❌ Container failed to start"
    docker logs botcore-twitch
    exit 1
fi

