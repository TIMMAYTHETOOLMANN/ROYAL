using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BotCore;
using BotCore.Dto;

namespace AutoDeploy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  JARVIS 2.0 - AUTONOMOUS TROVO DEPLOYMENT                 ║");
            Console.WriteLine("║  Target: https://trovo.live/s/timmaythetoolman            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Load proxies
            List<string> proxies = new List<string>();
            string proxyPath = Path.Combine(Directory.GetCurrentDirectory(), "proxies.txt");
            if (File.Exists(proxyPath))
            {
                proxies = File.ReadAllLines(proxyPath).Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")).ToList();
                Console.WriteLine($"✓ Loaded {proxies.Count} proxies");
            }
            else
            {
                Console.WriteLine("⚠ No proxies.txt found - deploying without proxies");
            }

            // Load user agents
            List<string> userAgents = new List<string>
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            };

            Console.WriteLine("✓ Configuration loaded");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🎯 Target: https://trovo.live/s/timmaythetoolman");
            Console.WriteLine("📊 Viewers: 15 (adaptive phased deployment)");
            Console.WriteLine("🖥️  Mode: FULL VISIBLE BROWSERS");
            Console.WriteLine("🛡️  Anti-Detection: ACTIVE");
            Console.ResetColor();
            Console.WriteLine();

            // Initialize deployment manager
            var deploymentManager = new AdaptiveDeploymentManager();
            deploymentManager.DeploymentLog += (msg) =>
            {
                if (msg.Contains("[CORE]"))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (msg.Contains("[DEPLOY]"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (msg.Contains("[PHASE"))
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (msg.Contains("[COMPLETE]"))
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(msg);
                Console.ResetColor();
            };

            // Configure deployment
            var deploymentConfig = new ExecuteNeedsDto
            {
                Stream = "https://trovo.live/s/timmaythetoolman",
                BrowserLimit = 15, // Deploy 15 viewers in adaptive phases
                Headless = false, // VISIBLE BROWSERS for Trovo
                UseLowCpuRam = false,
                ProxyListDirectory = proxyPath,
                UserAgentStrings = userAgents,
                Service = StreamService.Service.TrovoLive
            };

            Console.WriteLine("🚀 INITIATING DEPLOYMENT...\n");

            try
            {
                await deploymentManager.DeployAdaptiveAsync(deploymentConfig);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ DEPLOYMENT COMPLETE");
                Console.WriteLine($"✅ {deploymentManager.GetActiveViewers()} viewers active on Trovo");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Press Ctrl+C to stop all viewers...");

                await Task.Delay(-1); // Run indefinitely
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Deployment error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}

