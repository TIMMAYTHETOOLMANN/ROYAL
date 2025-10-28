// filepath: c:\Users\timot\RiderProjects\Stream-Viewer-Chat-Bot\BotCore\ChatEngagementEngine.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace BotCore
{
    /// <summary>
    /// Hyperrealistic chat engagement engine with advanced behavioral modeling
    /// </summary>
    public class ChatEngagementEngine
    {
        private readonly Random _random = new Random();
        private CancellationTokenSource _cancellationToken;
        private bool _isEngaging = false;
        private string _lastMessage = "";
        private DateTime _lastMessageTime = DateTime.MinValue;
        
        // Enhanced fields for hyperrealistic behavior
        private readonly ChatEngagementConfig _config;
        private readonly ChatPlatformConfig _platformConfig;
        private readonly IPage _page;
        private ViewerPersonalityProfile _personality;
        private EmotionalState _currentEmotion;
        private List<string> _recentMessages = new List<string>();
        private List<string> _messageHistory = new List<string>();
        private int _messagesSentThisSession = 0;
        private DateTime _sessionStartTime = DateTime.UtcNow;

        public delegate void ChatLogHandler(string message);
        public event ChatLogHandler OnChatLog;

        // Constructor for legacy compatibility
        public ChatEngagementEngine()
        {
            _config = new ChatEngagementConfig();
            _platformConfig = ChatPlatformConfig.GetDefault();
            _page = null;
            _currentEmotion = EmotionalState.Neutral;
        }

        public ChatEngagementEngine(ChatEngagementConfig config, ChatPlatformConfig platformConfig, IPage page)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _platformConfig = platformConfig ?? throw new ArgumentNullException(nameof(platformConfig));
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _currentEmotion = _config.InitialEmotionalState;
        }

        // Legacy method signature for backward compatibility
        public async Task StartEngagementAsync(IPage page, string platform, CancellationToken cancellationToken)
        {
            if (_page == null && page != null)
            {
                // Use the legacy simple mode
                await StartSimpleEngagementAsync(page, platform, cancellationToken);
            }
            else
            {
                await StartEngagementAsync(cancellationToken);
            }
        }

        private async Task StartSimpleEngagementAsync(IPage page, string platform, CancellationToken cancellationToken)
        {
            if (_isEngaging) return;
            _isEngaging = true;
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            OnChatLog?.Invoke($"[CHAT ENGINE] Starting engagement for {platform}...");

            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    var delay = _random.Next(60000, 180000);
                    await Task.Delay(delay, _cancellationToken.Token);
                    OnChatLog?.Invoke($"[CHAT ENGINE] Chat engagement running (simplified mode)");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnChatLog?.Invoke($"[CHAT ENGINE] Error: {ex.Message}");
                    await Task.Delay(30000, _cancellationToken.Token);
                }
            }

            _isEngaging = false;
        }

        public async Task StartEngagementAsync(CancellationToken cancellationToken)
        {
            if (_isEngaging) return;
            _isEngaging = true;
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            OnChatLog?.Invoke($"[CHAT ENGINE] Starting hyperrealistic engagement (Mimicry Level: {_config.ViewerMimicryLevel}/10)");

            // Assign a random personality if enabled
            if (_config.EnablePersonalitySimulation && _config.PersonalityProfiles.Any())
            {
                var profiles = _config.PersonalityProfiles.Values.ToList();
                _personality = profiles[_random.Next(profiles.Count)];
                OnChatLog?.Invoke($"[CHAT ENGINE] Personality: {_personality.Name} (Talk:{_personality.Talkativeness}/10, Pos:{_personality.Positivity}/10)");
            }

            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    // Calculate delay based on configuration and personality
                    var delay = CalculateDelay();
                    await Task.Delay(delay, _cancellationToken.Token);

                    // Check if we should send a message
                    if (ShouldSendMessage())
                    {
                        // Select message type and message
                        var messageType = SelectMessageType();
                        var message = GetMessageByType(messageType);

                        if (!string.IsNullOrEmpty(message))
                        {
                            // Simulate typing and send the message
                            await SimulateTyping(message);
                            await SendMessage(message);

                            // Update message history and emotional state
                            _messageHistory.Add(message);
                            _messagesSentThisSession++;
                            _lastMessage = message;
                            _lastMessageTime = DateTime.UtcNow;
                            
                            if (_config.EnableMessageMemory && _messageHistory.Count > _config.MessageMemoryDepth)
                            {
                                _messageHistory.RemoveAt(0);
                            }

                            // Update emotional state
                            UpdateEmotionalState(messageType);

                            OnChatLog?.Invoke($"[CHAT ENGINE] Sent [{messageType}]: {message}");
                        }
                    }

                    // Monitor chat for social proofing and responses (if enabled)
                    if (_config.EnableSocialProofing || _config.EnableResponseBehavior)
                    {
                        await MonitorChat();
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnChatLog?.Invoke($"[CHAT ENGINE] Error: {ex.Message}");
                    await Task.Delay(30000, _cancellationToken.Token);
                }
            }

            OnChatLog?.Invoke($"[CHAT ENGINE] Session ended. Sent {_messagesSentThisSession} messages over {(DateTime.UtcNow - _sessionStartTime).TotalMinutes:F1} minutes");
            _isEngaging = false;
        }

        private int CalculateDelay()
        {
            int baseDelay = _random.Next(_config.MinDelaySeconds, _config.MaxDelaySeconds + 1);

            // Adjust delay based on personality talkativeness
            if (_personality != null)
            {
                // Talkativeness: 1 (least) to 10 (most)
                // Higher talkativeness = shorter delay
                double factor = 1.5 - (_personality.Talkativeness * 0.05);
                baseDelay = (int)(baseDelay * factor);
            }

            // Apply burst messaging if enabled and triggered
            if (_config.EnableBurstMessaging && _random.Next(100) < _config.BurstProbability)
            {
                baseDelay = _config.BurstDelaySeconds;
                OnChatLog?.Invoke("[CHAT ENGINE] Burst mode activated!");
            }

            // Apply time-of-day pattern
            if (_config.TimingBehavior == TimingModel.TimeBased)
            {
                var hour = DateTime.Now.Hour;
                if (_config.TimePattern == TimeOfDayPattern.PeakHours && (hour >= 19 && hour <= 23))
                {
                    baseDelay = (int)(baseDelay * 0.7); // More active during peak hours
                }
                else if (_config.TimePattern == TimeOfDayPattern.OffPeak && (hour >= 2 && hour <= 6))
                {
                    baseDelay = (int)(baseDelay * 1.5); // Less active during off-peak
                }
            }

            return Math.Max(baseDelay * 1000, 5000); // Minimum 5 seconds
        }

        private bool ShouldSendMessage()
        {
            // Check engagement percentage
            if (_random.Next(100) >= _config.EngagementPercentage)
                return false;

            // Check social proofing if enabled
            if (_config.EnableSocialProofing)
            {
                // Less likely to chat if few recent messages
                if (_recentMessages.Count < _config.SocialInfluenceThreshold)
                {
                    if (_random.Next(100) < 70) // 70% chance to skip
                        return false;
                }
            }

            // Personality-based decision
            if (_personality != null)
            {
                // Check engagement style (1=reactive, 10=proactive)
                var engagementThreshold = 100 - (_personality.EngagementStyle * 10);
                if (_random.Next(100) > engagementThreshold)
                    return false;
            }

            return true;
        }

        private MessageType SelectMessageType()
        {
            // Create a weighted list of message types based on frequencies
            var weightedOptions = new List<MessageType>();

            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Reaction, _config.ReactionFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Emote, _config.EmoteFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Compliment, _config.ComplimentFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Hype, _config.HypeFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Engagement, _config.EngagementFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Question, _config.QuestionFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.Contextual, _config.ContextualFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.MemeReference, _config.MemeReferenceFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.SelfReference, _config.SelfReferenceFrequency));
            weightedOptions.AddRange(Enumerable.Repeat(MessageType.StreamerMention, _config.StreamerMentionFrequency));

            if (_config.CustomMessages.Any())
            {
                weightedOptions.AddRange(Enumerable.Repeat(MessageType.Custom, 10));
            }

            // Adjust based on personality preferences
            if (_personality != null && _personality.PreferredMessageTypes.Any())
            {
                foreach (var preferredType in _personality.PreferredMessageTypes)
                {
                    if (Enum.TryParse<MessageType>(preferredType, true, out var msgType))
                    {
                        weightedOptions.AddRange(Enumerable.Repeat(msgType, 15)); // Boost preferred types
                    }
                }
            }

            if (!weightedOptions.Any())
                return MessageType.Reaction;

            return weightedOptions[_random.Next(weightedOptions.Count)];
        }

        private string GetMessageByType(MessageType messageType)
        {
            List<string> messageList = messageType switch
            {
                MessageType.Reaction => GetReactionMessages(),
                MessageType.Emote => GetEmoteMessages(),
                MessageType.Compliment => GetComplimentMessages(),
                MessageType.Hype => GetHypeMessages(),
                MessageType.Engagement => GetEngagementMessages(),
                MessageType.Question => GetQuestionMessages(),
                MessageType.Contextual => GetContextualMessages(),
                MessageType.MemeReference => GetMemeReferenceMessages(),
                MessageType.SelfReference => GetSelfReferenceMessages(),
                MessageType.StreamerMention => GetStreamerMentionMessages(),
                MessageType.Custom => _config.CustomMessages,
                _ => GetReactionMessages()
            };

            if (messageList == null || !messageList.Any())
                return null;

            // Avoid repeating recent messages
            if (_config.EnableMessageMemory)
            {
                var availableMessages = messageList.Where(m => !_messageHistory.Contains(m)).ToList();
                if (availableMessages.Any())
                    messageList = availableMessages;
            }

            var message = messageList[_random.Next(messageList.Count)];

            // Apply message randomization if enabled
            if (_config.EnableMessageRandomization)
            {
                message = RandomizeMessage(message);
            }

            // Adjust message length based on personality
            if (_personality != null && _personality.MessageLengthPreference != 0)
            {
                // Personality can prefer shorter or longer messages
                // This is a placeholder - actual implementation would need more sophisticated logic
            }

            // Ensure message length is within limits
            if (message.Length < _config.MinMessageLength)
            {
                message = message.PadRight(_config.MinMessageLength);
            }
            if (message.Length > _config.MaxMessageLength)
            {
                message = message.Substring(0, _config.MaxMessageLength);
            }

            return message.Trim();
        }

        private string RandomizeMessage(string message)
        {
            var randomFactor = _config.RandomizationFactor;

            if (_random.Next(100) < randomFactor)
            {
                // Randomly change case of some letters
                var chars = message.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (_random.Next(100) < 10)
                    {
                        chars[i] = char.IsLower(chars[i]) ? char.ToUpper(chars[i]) : char.ToLower(chars[i]);
                    }
                }
                message = new string(chars);
            }

            if (_random.Next(100) < randomFactor / 2)
            {
                // Add or remove exclamation marks
                if (message.EndsWith("!") && _random.Next(100) < 50)
                {
                    message = message.TrimEnd('!');
                }
                else if (_random.Next(100) < 30)
                {
                    message += "!";
                }
            }

            return message;
        }

        private async Task SimulateTyping(string message)
        {
            try
            {
                if (_page == null) return;

                // Find chat input
                var chatInput = await FindChatInput();
                if (chatInput == null)
                {
                    OnChatLog?.Invoke("[CHAT ENGINE] Chat input not found, skipping message");
                    return;
                }

                // Clear any existing text
                await chatInput.FillAsync("");

                if (_config.TypingStyle == TypingBehavior.Perfect)
                {
                    await chatInput.TypeAsync(message);
                    return;
                }

                // Calculate typing speed
                int baseCPS = _random.Next(_config.MinTypingSpeed, _config.MaxTypingSpeed + 1) * 5 / 60;
                if (_personality != null)
                {
                    baseCPS = baseCPS * (100 + _random.Next(-_personality.TypingSpeedVariation, _personality.TypingSpeedVariation + 1)) / 100;
                }

                int errorProbability = _config.TypingErrorProbability;

                // Type with realistic behavior
                foreach (char c in message)
                {
                    await chatInput.TypeAsync(c.ToString());

                    int delay = 1000 / Math.Max(baseCPS, 1);
                    await Task.Delay(delay + _random.Next(-10, 20)); // Add variation

                    // Simulate typing errors
                    if (_random.Next(100) < errorProbability)
                    {
                        char wrongChar = (char)('a' + _random.Next(26));
                        await chatInput.TypeAsync(wrongChar.ToString());
                        await Task.Delay(delay);

                        // Correct the error
                        if (_random.Next(100) < _config.CorrectionProbability)
                        {
                            await chatInput.PressAsync("Backspace");
                            await Task.Delay(delay / 2);
                        }
                    }
                }

                // Random pause before sending
                await Task.Delay(_random.Next(200, 800));
            }
            catch (Exception ex)
            {
                OnChatLog?.Invoke($"[CHAT ENGINE] Typing error: {ex.Message}");
            }
        }

        private async Task SendMessage(string message)
        {
            try
            {
                if (_page == null) return;

                var chatInput = await FindChatInput();
                if (chatInput != null)
                {
                    await chatInput.PressAsync("Enter");
                    await Task.Delay(_random.Next(500, 1500));
                }
            }
            catch (Exception ex)
            {
                OnChatLog?.Invoke($"[CHAT ENGINE] Send error: {ex.Message}");
            }
        }

        private async Task<IElementHandle> FindChatInput()
        {
            if (_page == null) return null;

            foreach (var selector in _platformConfig.ChatInputSelectors ?? Array.Empty<string>())
            {
                try
                {
                    var element = await _page.QuerySelectorAsync(selector);
                    if (element != null)
                        return element;
                }
                catch { }
            }
            return null;
        }

        private async Task MonitorChat()
        {
            try
            {
                if (_page == null) return;

                var messages = await _page.EvaluateAsync<string[]>(@"
                    () => {
                        const selectors = ['.chat-message', '.message', '.chat-line', '[data-a-target=""chat-line-message""]'];
                        for (let selector of selectors) {
                            const elements = document.querySelectorAll(selector);
                            if (elements.length) {
                                return Array.from(elements).map(el => el.textContent).slice(-10);
                            }
                        }
                        return [];
                    }
                ");

                _recentMessages = messages.ToList();

                // Check for streamer messages if response behavior is enabled
                if (_config.EnableResponseBehavior && messages.Any())
                {
                    if (_random.Next(100) < _config.ResponseProbability)
                    {
                        OnChatLog?.Invoke("[CHAT ENGINE] Detected chat activity, may respond soon");
                    }
                }
            }
            catch (Exception ex)
            {
                OnChatLog?.Invoke($"[CHAT ENGINE] Monitor error: {ex.Message}");
            }
        }

        private void UpdateEmotionalState(MessageType messageType)
        {
            if (!_config.EnableEmotionalModeling) return;

            if (_random.Next(100) < _config.EmotionalShiftProbability)
            {
                var emotions = Enum.GetValues(typeof(EmotionalState));
                _currentEmotion = (EmotionalState)emotions.GetValue(_random.Next(emotions.Length));
            }

            // Message type influences emotion
            switch (messageType)
            {
                case MessageType.Hype:
                case MessageType.Compliment:
                    _currentEmotion = EmotionalState.Excited;
                    break;
                case MessageType.Question:
                    _currentEmotion = EmotionalState.Confused;
                    break;
                case MessageType.MemeReference:
                    _currentEmotion = EmotionalState.Amused;
                    break;
            }
        }

        // Message generation methods
        private List<string> GetReactionMessages()
        {
            var messages = new List<string>
            {
                "lol", "LOL", "haha", "hahaha", "😂", "🤣", "lmao", "rofl", "wtf", "omg", "wow",
                "nice", "cool", "awesome", "great", "sick", "fire", "🔥", "💯", "ok", "yeah", "yep"
            };
            messages.AddRange(_config.CustomReactions);
            return messages;
        }

        private List<string> GetEmoteMessages()
        {
            var messages = new List<string>
            {
                "😂", "🤣", "🔥", "💯", "❤️", "👍", "👀", "🎉", "🙏", "😍", "😎", "😭",
                "LUL", "KEKW", "Pog", "PogChamp", "Poggers", "OMEGALUL", "monkaS"
            };
            messages.AddRange(_config.CustomEmotes);
            return messages;
        }

        private List<string> GetComplimentMessages()
        {
            var messages = new List<string>
            {
                "great stream", "awesome content", "love this", "good stuff", "well played",
                "gg", "nice play", "good game", "great job", "keep it up"
            };
            messages.AddRange(_config.CustomCompliments);
            return messages;
        }

        private List<string> GetHypeMessages()
        {
            var messages = new List<string>
            {
                "lets go", "let's go!", "LETS GO", "pog", "poggers", "NO WAY", "INSANE",
                "CLUTCH", "ez", "too ez", "gg ez", "destroyed"
            };
            messages.AddRange(_config.CustomHypePhrases);
            return messages;
        }

        private List<string> GetEngagementMessages()
        {
            var messages = new List<string>
            {
                "how's it going", "first time here", "what's up", "hey everyone",
                "good vibes", "nice community", "loving the energy"
            };
            return messages;
        }

        private List<string> GetQuestionMessages()
        {
            var messages = new List<string>
            {
                "what game is this?", "how long you been streaming?", "what's the plan?",
                "when did you start?", "what rank are you?", "first playthrough?"
            };
            messages.AddRange(_config.CustomQuestions);
            return messages;
        }

        private List<string> GetContextualMessages()
        {
            var messages = new List<string>
            {
                "that was close", "almost had it", "unlucky", "so close", "next time",
                "good try", "you got this", "comeback time"
            };
            messages.AddRange(_config.CustomContextualPhrases);
            return messages;
        }

        private List<string> GetMemeReferenceMessages()
        {
            var messages = new List<string>
            {
                "based", "cringe", "ratio", "L", "W", "copium", "hopium", "touch grass"
            };
            return messages;
        }

        private List<string> GetSelfReferenceMessages()
        {
            var messages = new List<string>
            {
                "back", "I'm back", "been here a while", "still watching", "good stream so far"
            };
            return messages;
        }

        private List<string> GetStreamerMentionMessages()
        {
            var messages = new List<string>
            {
                "nice", "wp", "well played", "good stuff", "keep going", "you got this"
            };
            return messages;
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();
            _isEngaging = false;
        }
    }

    public class ChatPlatformConfig
    {
        public string[] ChatInputSelectors { get; set; }
        public string[] SendButtonSelectors { get; set; }
        public string[] ChatContainerSelectors { get; set; }
        public string[] MessageSelectors { get; set; }

        public static ChatPlatformConfig GetDefault()
        {
            return new ChatPlatformConfig
            {
                ChatInputSelectors = new[]
                {
                    "[data-a-target='chat-input']",
                    "textarea[placeholder*='chat']",
                    ".chat-input textarea",
                    "#chat-input"
                },
                SendButtonSelectors = new[]
                {
                    "[data-a-target='chat-send-button']",
                    ".send-button",
                    "button[type='submit']"
                },
                ChatContainerSelectors = new[]
                {
                    ".chat-scrollable-area__message-container",
                    ".chat-list",
                    "#chat-messages"
                },
                MessageSelectors = new[]
                {
                    "[data-a-target='chat-line-message']",
                    ".chat-message",
                    ".message"
                }
            };
        }
    }

    public enum MessageType
    {
        Reaction,
        Emote,
        Compliment,
        Hype,
        Engagement,
        Question,
        Contextual,
        MemeReference,
        SelfReference,
        StreamerMention,
        Custom
    }
}

