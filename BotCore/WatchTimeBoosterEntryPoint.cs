using System;
using System.Threading.Tasks;
using BotCore;
using Serilog;
using SerilogLog = Serilog.Log;

class WatchTimeBoosterEntryPoint
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            // Read environment variables
            var channelUsername = Environment.GetEnvironmentVariable("CHANNEL_USERNAME") ?? "timmaythetoolman";
            var viewerCount = int.Parse(Environment.GetEnvironmentVariable("VIEWER_COUNT") ?? "50");
            var headless = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "false");
            var lowResourceMode = bool.Parse(Environment.GetEnvironmentVariable("LOW_CPU_RAM") ?? "true");

            SerilogLog.Information("Starting Adaptive Watch Time Booster...");
            
            var booster = new AdaptiveWatchTimeBooster(channelUsername, viewerCount, headless, lowResourceMode);
            await booster.StartAsync();

            // Keep running
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            SerilogLog.Fatal($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

