using System.Collections.Generic;

namespace BotCore.Dto
{
    public class ExecuteNeedsDto
    {
        public bool Headless { get; set; }
        public string Service { get; set; }
        public string Stream { get; set; }
        public int BrowserLimit { get; set; }
        public List<string> ChatMessages { get; set; }
        public List<object> LoginInfos { get; set; }
        public string PreferredQuality { get; set; }
        public int RefreshInterval { get; set; }
        public string ProxyListDirectory { get; set; }
        public List<string> UserAgentStrings { get; set; }
        public bool UseLowCpuRam { get; set; }
        
        // NEW: Chat engagement settings
        public bool EnableChatEngagement { get; set; } = true;
        public int ChatEngagementPercentage { get; set; } = 30; // % of viewers that chat
        public int MinMessageDelaySeconds { get; set; } = 45;
        public int MaxMessageDelaySeconds { get; set; } = 180;
        public bool UseAggressiveChatMode { get; set; } = false; // More frequent messages
    }
}

