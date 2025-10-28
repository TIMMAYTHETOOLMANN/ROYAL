using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BotCore
{
    /// <summary>
    /// Advanced configuration manager for hyperrealistic chat engagement system
    /// </summary>
    public class ChatEngagementConfig
    {
        // Core Engagement Settings
        public bool Enabled { get; set; } = true;
        public int EngagementPercentage { get; set; } = 30; // % of viewers that chat
        public int MinDelaySeconds { get; set; } = 45;
        public int MaxDelaySeconds { get; set; } = 180;
        public bool AggressiveMode { get; set; } = false;
        public bool StealthMode { get; set; } = false; // Ultra-realistic low-profile chatting
        public int ViewerMimicryLevel { get; set; } = 5; // 1-10 scale of human-like behavior
        
        // Advanced Timing Models
        public TimingModel TimingBehavior { get; set; } = TimingModel.NaturalVariation;
        public TimeOfDayPattern TimePattern { get; set; } = TimeOfDayPattern.PeakHours;
        public bool EnableBurstMessaging { get; set; } = true;
        public int BurstProbability { get; set; } = 15; // % chance of burst messaging
        public int BurstMessageCount { get; set; } = 3; // Messages in burst
        public int BurstDelaySeconds { get; set; } = 10; // Delay between burst messages
        
        // Psychological Messaging Patterns
        public int ReactionFrequency { get; set; } = 35;
        public int EmoteFrequency { get; set; } = 20;
        public int ComplimentFrequency { get; set; } = 15;
        public int HypeFrequency { get; set; } = 10;
        public int EngagementFrequency { get; set; } = 10;
        public int QuestionFrequency { get; set; } = 10;
        public int ContextualFrequency { get; set; } = 15;
        public int MemeReferenceFrequency { get; set; } = 8;
        public int SelfReferenceFrequency { get; set; } = 5; // "I'm back", "been here 2 hours", etc.
        public int StreamerMentionFrequency { get; set; } = 12; // @streamername mentions
        
        // Cognitive Realism Settings
        public bool EnableMessageMemory { get; set; } = true; // Remembers recent messages
        public int MessageMemoryDepth { get; set; } = 15; // How many recent messages to remember
        public bool EnableContextAwareness { get; set; } = true; // Adapts to stream content
        public bool EnableEmotionalModeling { get; set; } = true; // Mood-based message selection
        public EmotionalState InitialEmotionalState { get; set; } = EmotionalState.Neutral;
        public int EmotionalShiftProbability { get; set; } = 25; // % chance to change mood
        
        // Social Behavior Modeling
        public bool EnableSocialProofing { get; set; } = true; // More likely to chat when others chat
        public int SocialInfluenceThreshold { get; set; } = 5; // Min chat activity to influence
        public bool EnableResponseBehavior { get; set; } = true; // Responds to streamer messages
        public int ResponseProbability { get; set; } = 40; // % chance to respond to streamer
        public bool EnableConversationSimulation { get; set; } = true; // Simulates mini-conversations
        public int ConversationProbability { get; set; } = 20; // % chance to start conversation
        
        // Typing Behavior Realism
        public TypingBehavior TypingStyle { get; set; } = TypingBehavior.HumanLike;
        public int MinTypingSpeed { get; set; } = 70; // WPM
        public int MaxTypingSpeed { get; set; } = 180; // WPM
        public int TypingErrorProbability { get; set; } = 8; // % chance of typo
        public int CorrectionProbability { get; set; } = 70; // % chance to correct typo
        
        // Custom Content
        public List<string> CustomMessages { get; set; } = new List<string>();
        public List<string> CustomReactions { get; set; } = new List<string>();
        public List<string> CustomQuestions { get; set; } = new List<string>();
        public List<string> CustomCompliments { get; set; } = new List<string>();
        public List<string> CustomEmotes { get; set; } = new List<string>();
        public List<string> CustomHypePhrases { get; set; } = new List<string>();
        public List<string> CustomContextualPhrases { get; set; } = new List<string>();
        
        // Platform-specific settings
        public Dictionary<string, PlatformChatSettings> PlatformSettings { get; set; } = new Dictionary<string, PlatformChatSettings>();
        
        // Advanced Behavioral Profiles
        public Dictionary<string, ViewerPersonalityProfile> PersonalityProfiles { get; set; } = new Dictionary<string, ViewerPersonalityProfile>();
        public bool EnablePersonalitySimulation { get; set; } = true;
        public int PersonalityVariation { get; set; } = 7; // 1-10 scale of personality diversity
        
        // Anti-Detection Measures
        public bool EnableAntiDetection { get; set; } = true;
        public int MinMessageLength { get; set; } = 1;
        public int MaxMessageLength { get; set; } = 200;
        public bool EnableMessageRandomization { get; set; } = true;
        public int RandomizationFactor { get; set; } = 30; // % of message modification
        
        public static ChatEngagementConfig LoadFromFile(string filePath)
        {
            var config = new ChatEngagementConfig();
            
            if (!File.Exists(filePath))
            {
                // Create default config file with hyperrealistic settings
                SaveToFile(config, filePath);
                return config;
            }
            
            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
                        continue;
                    
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2) continue;
                    
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    switch (key.ToLower())
                    {
                        // Core Settings
                        case "enabled":
                            config.Enabled = bool.Parse(value);
                            break;
                        case "engagementpercentage":
                            config.EngagementPercentage = int.Parse(value);
                            break;
                        case "mindelayseconds":
                            config.MinDelaySeconds = int.Parse(value);
                            break;
                        case "maxdelayseconds":
                            config.MaxDelaySeconds = int.Parse(value);
                            break;
                        case "aggressivemode":
                            config.AggressiveMode = bool.Parse(value);
                            break;
                        case "stealthmode":
                            config.StealthMode = bool.Parse(value);
                            break;
                        case "viewermimicrylevel":
                            config.ViewerMimicryLevel = Math.Max(1, Math.Min(10, int.Parse(value)));
                            break;
                        
                        // Timing Models
                        case "timingbehavior":
                            config.TimingBehavior = Enum.TryParse<TimingModel>(value, true, out var timing) ? timing : TimingModel.NaturalVariation;
                            break;
                        case "timepattern":
                            config.TimePattern = Enum.TryParse<TimeOfDayPattern>(value, true, out var pattern) ? pattern : TimeOfDayPattern.PeakHours;
                            break;
                        case "enableburstmessaging":
                            config.EnableBurstMessaging = bool.Parse(value);
                            break;
                        case "burstprobability":
                            config.BurstProbability = int.Parse(value);
                            break;
                        case "burstmessagecount":
                            config.BurstMessageCount = int.Parse(value);
                            break;
                        case "burstdelayseconds":
                            config.BurstDelaySeconds = int.Parse(value);
                            break;
                        
                        // Psychological Patterns
                        case "reactionfrequency":
                            config.ReactionFrequency = int.Parse(value);
                            break;
                        case "emotefrequency":
                            config.EmoteFrequency = int.Parse(value);
                            break;
                        case "complimentfrequency":
                            config.ComplimentFrequency = int.Parse(value);
                            break;
                        case "hypefrequency":
                            config.HypeFrequency = int.Parse(value);
                            break;
                        case "engagementfrequency":
                            config.EngagementFrequency = int.Parse(value);
                            break;
                        case "questionfrequency":
                            config.QuestionFrequency = int.Parse(value);
                            break;
                        case "contextualfrequency":
                            config.ContextualFrequency = int.Parse(value);
                            break;
                        case "memereferencefrequency":
                            config.MemeReferenceFrequency = int.Parse(value);
                            break;
                        case "selfreferencefrequency":
                            config.SelfReferenceFrequency = int.Parse(value);
                            break;
                        case "streamermentionfrequency":
                            config.StreamerMentionFrequency = int.Parse(value);
                            break;
                        
                        // Cognitive Settings
                        case "enablemessagememory":
                            config.EnableMessageMemory = bool.Parse(value);
                            break;
                        case "messagememorydepth":
                            config.MessageMemoryDepth = int.Parse(value);
                            break;
                        case "enablecontextawareness":
                            config.EnableContextAwareness = bool.Parse(value);
                            break;
                        case "enableemotionalmodeling":
                            config.EnableEmotionalModeling = bool.Parse(value);
                            break;
                        case "initialemotionalstate":
                            config.InitialEmotionalState = Enum.TryParse<EmotionalState>(value, true, out var emotion) ? emotion : EmotionalState.Neutral;
                            break;
                        case "emotionalshiftprobability":
                            config.EmotionalShiftProbability = int.Parse(value);
                            break;
                        
                        // Social Behavior
                        case "enablesocialproofing":
                            config.EnableSocialProofing = bool.Parse(value);
                            break;
                        case "socialinfluencethreshold":
                            config.SocialInfluenceThreshold = int.Parse(value);
                            break;
                        case "enableresponsebehavior":
                            config.EnableResponseBehavior = bool.Parse(value);
                            break;
                        case "responseprobability":
                            config.ResponseProbability = int.Parse(value);
                            break;
                        case "enableconversationsimulation":
                            config.EnableConversationSimulation = bool.Parse(value);
                            break;
                        case "conversationprobability":
                            config.ConversationProbability = int.Parse(value);
                            break;
                        
                        // Typing Behavior
                        case "typingstyle":
                            config.TypingStyle = Enum.TryParse<TypingBehavior>(value, true, out var typing) ? typing : TypingBehavior.HumanLike;
                            break;
                        case "mintypingspeed":
                            config.MinTypingSpeed = int.Parse(value);
                            break;
                        case "maxtypingspeed":
                            config.MaxTypingSpeed = int.Parse(value);
                            break;
                        case "typingerrorprobability":
                            config.TypingErrorProbability = int.Parse(value);
                            break;
                        case "correctionprobability":
                            config.CorrectionProbability = int.Parse(value);
                            break;
                        
                        // Anti-Detection
                        case "enableantidetection":
                            config.EnableAntiDetection = bool.Parse(value);
                            break;
                        case "minmessagelength":
                            config.MinMessageLength = int.Parse(value);
                            break;
                        case "maxmessagelength":
                            config.MaxMessageLength = int.Parse(value);
                            break;
                        case "enablemessagerandomization":
                            config.EnableMessageRandomization = bool.Parse(value);
                            break;
                        case "randomizationfactor":
                            config.RandomizationFactor = int.Parse(value);
                            break;
                        
                        // Personality Simulation
                        case "enablepersonalitysimulation":
                            config.EnablePersonalitySimulation = bool.Parse(value);
                            break;
                        case "personalityvariation":
                            config.PersonalityVariation = Math.Max(1, Math.Min(10, int.Parse(value)));
                            break;
                        
                        // Custom Content
                        case "custommessage":
                            config.CustomMessages.Add(value);
                            break;
                        case "customreaction":
                            config.CustomReactions.Add(value);
                            break;
                        case "customquestion":
                            config.CustomQuestions.Add(value);
                            break;
                        case "customcompliment":
                            config.CustomCompliments.Add(value);
                            break;
                        case "customemote":
                            config.CustomEmotes.Add(value);
                            break;
                        case "customhypephrase":
                            config.CustomHypePhrases.Add(value);
                            break;
                        case "customcontextualphrase":
                            config.CustomContextualPhrases.Add(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chat config: {ex.Message}");
            }
            
            return config;
        }
        
        public static void SaveToFile(ChatEngagementConfig config, string filePath)
        {
            try
            {
                var lines = new List<string>
                {
                    "# ========================================================",
                    "# HYPERREALISTIC CHAT ENGAGEMENT CONFIGURATION",
                    "# ========================================================",
                    "",
                    "# Core Engagement Settings",
                    "# Enable/disable chat engagement system",
                    $"Enabled={config.Enabled}",
                    "# Percentage of viewers that will engage in chat (0-100)",
                    $"EngagementPercentage={config.EngagementPercentage}",
                    "# Message timing (seconds)",
                    $"MinDelaySeconds={config.MinDelaySeconds}",
                    $"MaxDelaySeconds={config.MaxDelaySeconds}",
                    "# Aggressive mode sends messages more frequently",
                    $"AggressiveMode={config.AggressiveMode}",
                    "# Stealth mode mimics real lurkers",
                    $"StealthMode={config.StealthMode}",
                    "# Viewer mimicry level (1-10 scale of human-like behavior)",
                    $"ViewerMimicryLevel={config.ViewerMimicryLevel}",
                    "",
                    "# Advanced Timing Models",
                    $"TimingBehavior={config.TimingBehavior}",
                    $"TimePattern={config.TimePattern}",
                    $"EnableBurstMessaging={config.EnableBurstMessaging}",
                    $"BurstProbability={config.BurstProbability}",
                    $"BurstMessageCount={config.BurstMessageCount}",
                    $"BurstDelaySeconds={config.BurstDelaySeconds}",
                    "",
                    "# Psychological Messaging Patterns",
                    $"ReactionFrequency={config.ReactionFrequency}",
                    $"EmoteFrequency={config.EmoteFrequency}",
                    $"ComplimentFrequency={config.ComplimentFrequency}",
                    $"HypeFrequency={config.HypeFrequency}",
                    $"EngagementFrequency={config.EngagementFrequency}",
                    $"QuestionFrequency={config.QuestionFrequency}",
                    $"ContextualFrequency={config.ContextualFrequency}",
                    $"MemeReferenceFrequency={config.MemeReferenceFrequency}",
                    $"SelfReferenceFrequency={config.SelfReferenceFrequency}",
                    $"StreamerMentionFrequency={config.StreamerMentionFrequency}",
                    "",
                    "# Cognitive Realism Settings",
                    $"EnableMessageMemory={config.EnableMessageMemory}",
                    $"MessageMemoryDepth={config.MessageMemoryDepth}",
                    $"EnableContextAwareness={config.EnableContextAwareness}",
                    $"EnableEmotionalModeling={config.EnableEmotionalModeling}",
                    $"InitialEmotionalState={config.InitialEmotionalState}",
                    $"EmotionalShiftProbability={config.EmotionalShiftProbability}",
                    "",
                    "# Social Behavior Modeling",
                    $"EnableSocialProofing={config.EnableSocialProofing}",
                    $"SocialInfluenceThreshold={config.SocialInfluenceThreshold}",
                    $"EnableResponseBehavior={config.EnableResponseBehavior}",
                    $"ResponseProbability={config.ResponseProbability}",
                    $"EnableConversationSimulation={config.EnableConversationSimulation}",
                    $"ConversationProbability={config.ConversationProbability}",
                    "",
                    "# Typing Behavior Realism",
                    $"TypingStyle={config.TypingStyle}",
                    $"MinTypingSpeed={config.MinTypingSpeed}",
                    $"MaxTypingSpeed={config.MaxTypingSpeed}",
                    $"TypingErrorProbability={config.TypingErrorProbability}",
                    $"CorrectionProbability={config.CorrectionProbability}",
                    "",
                    "# Anti-Detection Measures",
                    $"EnableAntiDetection={config.EnableAntiDetection}",
                    $"MinMessageLength={config.MinMessageLength}",
                    $"MaxMessageLength={config.MaxMessageLength}",
                    $"EnableMessageRandomization={config.EnableMessageRandomization}",
                    $"RandomizationFactor={config.RandomizationFactor}",
                    "",
                    "# Personality Simulation",
                    $"EnablePersonalitySimulation={config.EnablePersonalitySimulation}",
                    $"PersonalityVariation={config.PersonalityVariation}",
                    "",
                    "# Custom Content (one per line, prefix with appropriate key)",
                    "# Example: CustomMessage=This is awesome!",
                    "# Example: CustomReaction=LOL!",
                    "# Example: CustomQuestion=How long have you been streaming?",
                    "# Example: CustomCompliment=Great gameplay!",
                    "# Example: CustomEmote=PogChamp",
                    "# Example: CustomHypePhrase=NO WAY!",
                    "# Example: CustomContextualPhrase=First blood!"
                };
                
                // Add custom content
                foreach (var msg in config.CustomMessages)
                    lines.Add($"CustomMessage={msg}");
                foreach (var reaction in config.CustomReactions)
                    lines.Add($"CustomReaction={reaction}");
                foreach (var question in config.CustomQuestions)
                    lines.Add($"CustomQuestion={question}");
                foreach (var compliment in config.CustomCompliments)
                    lines.Add($"CustomCompliment={compliment}");
                foreach (var emote in config.CustomEmotes)
                    lines.Add($"CustomEmote={emote}");
                foreach (var hype in config.CustomHypePhrases)
                    lines.Add($"CustomHypePhrase={hype}");
                foreach (var contextual in config.CustomContextualPhrases)
                    lines.Add($"CustomContextualPhrase={contextual}");
                
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chat config: {ex.Message}");
            }
        }
        
        public ChatEngagementConfig Clone()
        {
            return new ChatEngagementConfig
            {
                Enabled = this.Enabled,
                EngagementPercentage = this.EngagementPercentage,
                MinDelaySeconds = this.MinDelaySeconds,
                MaxDelaySeconds = this.MaxDelaySeconds,
                AggressiveMode = this.AggressiveMode,
                StealthMode = this.StealthMode,
                ViewerMimicryLevel = this.ViewerMimicryLevel,
                TimingBehavior = this.TimingBehavior,
                TimePattern = this.TimePattern,
                EnableBurstMessaging = this.EnableBurstMessaging,
                BurstProbability = this.BurstProbability,
                BurstMessageCount = this.BurstMessageCount,
                BurstDelaySeconds = this.BurstDelaySeconds,
                ReactionFrequency = this.ReactionFrequency,
                EmoteFrequency = this.EmoteFrequency,
                ComplimentFrequency = this.ComplimentFrequency,
                HypeFrequency = this.HypeFrequency,
                EngagementFrequency = this.EngagementFrequency,
                QuestionFrequency = this.QuestionFrequency,
                ContextualFrequency = this.ContextualFrequency,
                MemeReferenceFrequency = this.MemeReferenceFrequency,
                SelfReferenceFrequency = this.SelfReferenceFrequency,
                StreamerMentionFrequency = this.StreamerMentionFrequency,
                EnableMessageMemory = this.EnableMessageMemory,
                MessageMemoryDepth = this.MessageMemoryDepth,
                EnableContextAwareness = this.EnableContextAwareness,
                EnableEmotionalModeling = this.EnableEmotionalModeling,
                InitialEmotionalState = this.InitialEmotionalState,
                EmotionalShiftProbability = this.EmotionalShiftProbability,
                EnableSocialProofing = this.EnableSocialProofing,
                SocialInfluenceThreshold = this.SocialInfluenceThreshold,
                EnableResponseBehavior = this.EnableResponseBehavior,
                ResponseProbability = this.ResponseProbability,
                EnableConversationSimulation = this.EnableConversationSimulation,
                ConversationProbability = this.ConversationProbability,
                TypingStyle = this.TypingStyle,
                MinTypingSpeed = this.MinTypingSpeed,
                MaxTypingSpeed = this.MaxTypingSpeed,
                TypingErrorProbability = this.TypingErrorProbability,
                CorrectionProbability = this.CorrectionProbability,
                EnableAntiDetection = this.EnableAntiDetection,
                MinMessageLength = this.MinMessageLength,
                MaxMessageLength = this.MaxMessageLength,
                EnableMessageRandomization = this.EnableMessageRandomization,
                RandomizationFactor = this.RandomizationFactor,
                EnablePersonalitySimulation = this.EnablePersonalitySimulation,
                PersonalityVariation = this.PersonalityVariation,
                CustomMessages = new List<string>(this.CustomMessages),
                CustomReactions = new List<string>(this.CustomReactions),
                CustomQuestions = new List<string>(this.CustomQuestions),
                CustomCompliments = new List<string>(this.CustomCompliments),
                CustomEmotes = new List<string>(this.CustomEmotes),
                CustomHypePhrases = new List<string>(this.CustomHypePhrases),
                CustomContextualPhrases = new List<string>(this.CustomContextualPhrases),
                PlatformSettings = this.PlatformSettings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()),
                PersonalityProfiles = this.PersonalityProfiles.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone())
            };
        }
        
        // Advanced configuration methods
        public void SetFrequencyProfile(FrequencyProfile profile)
        {
            switch (profile)
            {
                case FrequencyProfile.Casual:
                    ReactionFrequency = 40;
                    EmoteFrequency = 25;
                    ComplimentFrequency = 10;
                    HypeFrequency = 5;
                    EngagementFrequency = 10;
                    QuestionFrequency = 10;
                    ContextualFrequency = 15;
                    MemeReferenceFrequency = 5;
                    SelfReferenceFrequency = 3;
                    StreamerMentionFrequency = 8;
                    break;
                case FrequencyProfile.Active:
                    ReactionFrequency = 30;
                    EmoteFrequency = 20;
                    ComplimentFrequency = 15;
                    HypeFrequency = 15;
                    EngagementFrequency = 10;
                    QuestionFrequency = 15;
                    ContextualFrequency = 20;
                    MemeReferenceFrequency = 10;
                    SelfReferenceFrequency = 8;
                    StreamerMentionFrequency = 15;
                    break;
                case FrequencyProfile.Aggressive:
                    ReactionFrequency = 25;
                    EmoteFrequency = 15;
                    ComplimentFrequency = 20;
                    HypeFrequency = 20;
                    EngagementFrequency = 15;
                    QuestionFrequency = 20;
                    ContextualFrequency = 25;
                    MemeReferenceFrequency = 15;
                    SelfReferenceFrequency = 12;
                    StreamerMentionFrequency = 25;
                    break;
            }
        }
        
        public void SetTimingProfile(TimingProfile profile)
        {
            switch (profile)
            {
                case TimingProfile.Lurker:
                    MinDelaySeconds = 120;
                    MaxDelaySeconds = 600;
                    BurstProbability = 5;
                    break;
                case TimingProfile.Regular:
                    MinDelaySeconds = 45;
                    MaxDelaySeconds = 180;
                    BurstProbability = 15;
                    break;
                case TimingProfile.Enthusiast:
                    MinDelaySeconds = 20;
                    MaxDelaySeconds = 90;
                    BurstProbability = 30;
                    break;
            }
        }
    }
    
    public class PlatformChatSettings
    {
        public bool Enabled { get; set; } = true;
        public int MinDelayOverride { get; set; } = -1; // -1 means use global
        public int MaxDelayOverride { get; set; } = -1;
        public List<string> PlatformSpecificMessages { get; set; } = new List<string>();
        public List<string> PlatformSpecificReactions { get; set; } = new List<string>();
        public List<string> PlatformSpecificEmotes { get; set; } = new List<string>();
        public int PlatformEngagementPercentage { get; set; } = -1; // -1 means use global
        public bool EnablePlatformSpecificBehavior { get; set; } = true;
        
        public PlatformChatSettings Clone()
        {
            return new PlatformChatSettings
            {
                Enabled = this.Enabled,
                MinDelayOverride = this.MinDelayOverride,
                MaxDelayOverride = this.MaxDelayOverride,
                PlatformSpecificMessages = new List<string>(this.PlatformSpecificMessages),
                PlatformSpecificReactions = new List<string>(this.PlatformSpecificReactions),
                PlatformSpecificEmotes = new List<string>(this.PlatformSpecificEmotes),
                PlatformEngagementPercentage = this.PlatformEngagementPercentage,
                EnablePlatformSpecificBehavior = this.EnablePlatformSpecificBehavior
            };
        }
    }
    
    public class ViewerPersonalityProfile
    {
        public string Name { get; set; }
        public int Talkativeness { get; set; } = 5; // 1-10 scale
        public int Positivity { get; set; } = 5; // 1-10 scale
        public int Humor { get; set; } = 5; // 1-10 scale
        public int KnowledgeLevel { get; set; } = 5; // 1-10 scale
        public int EngagementStyle { get; set; } = 5; // 1-10 scale (reactive vs proactive)
        public List<string> PreferredMessageTypes { get; set; } = new List<string>();
        public List<string> AvoidedMessageTypes { get; set; } = new List<string>();
        public int TypingSpeedVariation { get; set; } = 20; // % variation from base speed
        public int MessageLengthPreference { get; set; } = 0; // -50 to +50 chars
        
        public ViewerPersonalityProfile Clone()
        {
            return new ViewerPersonalityProfile
            {
                Name = this.Name,
                Talkativeness = this.Talkativeness,
                Positivity = this.Positivity,
                Humor = this.Humor,
                KnowledgeLevel = this.KnowledgeLevel,
                EngagementStyle = this.EngagementStyle,
                PreferredMessageTypes = new List<string>(this.PreferredMessageTypes),
                AvoidedMessageTypes = new List<string>(this.AvoidedMessageTypes),
                TypingSpeedVariation = this.TypingSpeedVariation,
                MessageLengthPreference = this.MessageLengthPreference
            };
        }
    }
    
    // Enums for advanced configuration
    public enum TimingModel
    {
        NaturalVariation,    // Human-like random variation
        BurstFocused,        // More burst messaging
        SteadyPaced,         // Consistent timing
        Reactive,            // Responds to chat activity
        TimeBased            // Changes based on time of day
    }
    
    public enum TimeOfDayPattern
    {
        PeakHours,           // High activity (7-11 PM)
        OffPeak,             // Low activity (2-6 AM)
        Evening,             // Moderate evening activity
        Morning,             // Morning viewers
        AllDay               // Consistent all day
    }
    
    public enum TypingBehavior
    {
        HumanLike,           // Realistic typing with errors
        FastClean,           // Quick accurate typing
        SlowThoughtful,      // Deliberate slow typing
        Erratic,             // Random typing patterns
        Perfect              // No errors, consistent speed
    }
    
    public enum EmotionalState
    {
        Neutral,
        Excited,
        Bored,
        Confused,
        Frustrated,
        Amused,
        Impressed,
        Disappointed
    }
    
    public enum FrequencyProfile
    {
        Casual,              // Low engagement, mostly reactions
        Active,              // Balanced engagement
        Aggressive           // High engagement, frequent messaging
    }
    
    public enum TimingProfile
    {
        Lurker,              // Infrequent, long delays
        Regular,             // Standard timing
        Enthusiast           // Frequent, short delays
    }
}

