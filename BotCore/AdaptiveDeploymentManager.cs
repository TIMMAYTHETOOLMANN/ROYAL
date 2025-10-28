using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotCore.Dto;

namespace BotCore
{
    public class AdaptiveDeploymentManager
    {
        private readonly Core _core;
        private CancellationTokenSource _cancellationTokenSource;
        private int _deployedViewers = 0;
        private DateTime _lastDeployment = DateTime.MinValue;
        
        public event Action<string> DeploymentLog;
        
        private readonly Random _random = new Random();
        
        public AdaptiveDeploymentManager()
        {
            _core = new Core();
            _core.LogMessage += (message, level) => DeploymentLog?.Invoke($"[CORE] {message}");
            _core.IncreaseViewer += () => 
            {
                _deployedViewers++;
                DeploymentLog?.Invoke($"[DEPLOY] Viewer #{_deployedViewers} launched");
            };
            _core.DecreaseViewer += () =>
            {
                _deployedViewers--;
                DeploymentLog?.Invoke($"[DEPLOY] Active viewers: {_deployedViewers}");
            };
            _core.LiveViewer += (count) => DeploymentLog?.Invoke($"[STATUS] Live viewers: {count}");
        }
        
        public async Task DeployAdaptiveAsync(ExecuteNeedsDto needs)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            var platformName = DetectPlatformName(needs.Stream, needs.Service);
            
            DeploymentLog?.Invoke("════════════════════════════════════════════════════════");
            DeploymentLog?.Invoke($"  ADAPTIVE DEPLOYMENT - {platformName.ToUpper()}");
            DeploymentLog?.Invoke($"  TARGET: {needs.Stream}");
            DeploymentLog?.Invoke($"  MODE: {(needs.Headless ? "HEADLESS" : "VISIBLE")} BROWSERS");
            DeploymentLog?.Invoke($"  CHAT: {(needs.EnableChatEngagement ? $"ENABLED ({needs.ChatEngagementPercentage}%)" : "DISABLED")}");
            DeploymentLog?.Invoke("════════════════════════════════════════════════════════");
            
            var targetViewers = needs.BrowserLimit;
            var deploymentPhases = CalculateDeploymentPhases(targetViewers);
            
            DeploymentLog?.Invoke($"[STRATEGY] Deploying {targetViewers} viewers in {deploymentPhases.Count} phases");
            
            int viewersDeployed = 0;
            
            foreach (var phase in deploymentPhases)
            {
                DeploymentLog?.Invoke($"[PHASE {phase.PhaseNumber}] Deploying {phase.ViewerCount} viewers with {phase.Delay}ms intervals");
                
                for (int i = 0; i < phase.ViewerCount; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                    
                    // Deploy single viewer with full configuration
                    var viewerNeeds = new ExecuteNeedsDto
                    {
                        Stream = needs.Stream,
                        BrowserLimit = 1,
                        Headless = needs.Headless,
                        UseLowCpuRam = needs.UseLowCpuRam,
                        ProxyListDirectory = needs.ProxyListDirectory,
                        UserAgentStrings = needs.UserAgentStrings,
                        Service = needs.Service,
                        EnableChatEngagement = needs.EnableChatEngagement,
                        ChatEngagementPercentage = needs.ChatEngagementPercentage,
                        MinMessageDelaySeconds = needs.MinMessageDelaySeconds,
                        MaxMessageDelaySeconds = needs.MaxMessageDelaySeconds,
                        UseAggressiveChatMode = needs.UseAggressiveChatMode,
                        PreferredQuality = needs.PreferredQuality,
                        RefreshInterval = needs.RefreshInterval
                    };
                    
                    Task.Run(() => _core.Start(viewerNeeds));
                    
                    viewersDeployed++;
                    
                    var delay = phase.Delay + _random.Next(-500, 500);
                    DeploymentLog?.Invoke($"[THROTTLE] Waiting {delay}ms...");
                    await Task.Delay(delay, _cancellationTokenSource.Token);
                }
                
                if (phase.PhaseNumber < deploymentPhases.Count)
                {
                    var interPhaseDelay = _random.Next(10000, 20000);
                    DeploymentLog?.Invoke($"[PHASE BREAK] {interPhaseDelay/1000}s pause...");
                    await Task.Delay(interPhaseDelay, _cancellationTokenSource.Token);
                }
            }
            
            DeploymentLog?.Invoke($"[COMPLETE] {viewersDeployed}/{targetViewers} viewers active on {platformName}");
            _lastDeployment = DateTime.Now;
        }
        
        private string DetectPlatformName(string url, string service)
        {
            if (!string.IsNullOrEmpty(service) && service != "auto")
            {
                return service switch
                {
                    "trovo" => "Trovo",
                    "kick" => "Kick",
                    "youtube" => "YouTube",
                    "twitch" => "Twitch",
                    "rumble" => "Rumble",
                    _ => service
                };
            }
            
            if (url.Contains("trovo.live")) return "Trovo";
            if (url.Contains("kick.com")) return "Kick";
            if (url.Contains("youtube.com") || url.Contains("youtu.be")) return "YouTube";
            if (url.Contains("twitch.tv")) return "Twitch";
            if (url.Contains("rumble.com")) return "Rumble";
            return "Unknown Platform";
        }
        
        private List<DeploymentPhase> CalculateDeploymentPhases(int totalViewers)
        {
            var phases = new List<DeploymentPhase>();
            
            if (totalViewers <= 5)
            {
                phases.Add(new DeploymentPhase { PhaseNumber = 1, ViewerCount = totalViewers, Delay = 8000 });
            }
            else if (totalViewers <= 20)
            {
                var phase1 = Math.Min(3, totalViewers);
                var phase2 = Math.Min(7, totalViewers - phase1);
                var phase3 = totalViewers - phase1 - phase2;
                
                if (phase1 > 0) phases.Add(new DeploymentPhase { PhaseNumber = 1, ViewerCount = phase1, Delay = 6000 });
                if (phase2 > 0) phases.Add(new DeploymentPhase { PhaseNumber = 2, ViewerCount = phase2, Delay = 4000 });
                if (phase3 > 0) phases.Add(new DeploymentPhase { PhaseNumber = 3, ViewerCount = phase3, Delay = 5000 });
            }
            else
            {
                var phase1 = 5;
                var phase2 = 10;
                var remaining = totalViewers - 15;
                
                phases.Add(new DeploymentPhase { PhaseNumber = 1, ViewerCount = phase1, Delay = 7000 });
                phases.Add(new DeploymentPhase { PhaseNumber = 2, ViewerCount = phase2, Delay = 5000 });
                if (remaining > 0) phases.Add(new DeploymentPhase { PhaseNumber = 3, ViewerCount = remaining, Delay = 3500 });
            }
            
            return phases;
        }
        
        public void Stop()
        {
            DeploymentLog?.Invoke("[DEPLOY] Stopping all viewers...");
            _cancellationTokenSource?.Cancel();
            _core.Stop();
        }
        
        public int GetActiveViewers() => _deployedViewers;
        
        private class DeploymentPhase
        {
            public int PhaseNumber { get; set; }
            public int ViewerCount { get; set; }
            public int Delay { get; set; }
        }
    }
}

