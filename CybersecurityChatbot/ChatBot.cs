using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * This class holds the core logic and functionality of the chatbot.
     * It handles user interactions, processes input, and generates responses.
     */
    public class ChatBot
    {
        private readonly KeywordResponder _keywords;
        private readonly SentimentDetector _sentiment;
        private readonly MemoryStore _memory;
        private bool _hasShownInitialPrompt;
        private bool _hasShownPersonalisedIntro;
        private const string CurrentTopicKey = "currenttopic";
        private static readonly string[] TopicKeywords =
        {
            "password",
            "phishing",
            "privacy",
            "scam",
            "malware",
            "vpn",
            "twofactor",
            "ransomware",
            "firewall"
        };

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
            _hasShownInitialPrompt = false;
        }

        public string ProcessInput(string userInput)
        {
            if (!_hasShownInitialPrompt)
            {
                _hasShownInitialPrompt = true;
                return "What should I call you?";
            }

            if (string.IsNullOrWhiteSpace(_memory.Recall("username")))
            {
                InitialiseMemory(userInput, string.Empty);
                return $"Nice to meet you {userInput}! And what's your favourite cybersecurity topic?";
            }

            if (string.IsNullOrWhiteSpace(_memory.Recall("favouritetopic")))
            {
                string currentName = _memory.Recall("username");
                InitialiseMemory(currentName, userInput);

                return CombineResponses
                (
                    BuildPersonalisedIntro(),
                    BuildNormalResponse(userInput)
                );
            }

            return BuildNormalResponse(userInput);
        }

        private string BuildNormalResponse(string userInput)
        {
            string normalizedInput = userInput?.ToLowerInvariant() ?? string.Empty;
            bool isFollowUpRequest = normalizedInput.Contains("tell me more") ||
                                     normalizedInput.Contains("more details") ||
                                     normalizedInput == "more";

            string keywordsResponse;

            if (isFollowUpRequest)
            {
                string currentTopic = _memory.Recall(CurrentTopicKey);
                if (string.IsNullOrWhiteSpace(currentTopic))
                {
                    currentTopic = _memory.Recall("favouritetopic");
                }

                if (!string.IsNullOrWhiteSpace(currentTopic))
                {
                    keywordsResponse = _keywords.GetResponse(currentTopic);
                }
                else
                {
                    keywordsResponse = "Sorry, I don't have information on that topic. Please try asking about something else.";
                }
            }
            else
            {
                keywordsResponse = _keywords.GetResponse(userInput ?? string.Empty);

                string matchedTopic = GetMatchingTopic(normalizedInput);
                if (!string.IsNullOrWhiteSpace(matchedTopic))
                {
                    _memory.Store(CurrentTopicKey, matchedTopic);
                }
            }

            Sentiment sentiment = _sentiment.Detect(userInput ?? string.Empty);
            string sentimentResponse = _sentiment.GetSentimentResponse(sentiment);
            string intro = BuildPersonalisedIntro();

            return CombineResponses(intro, sentimentResponse, keywordsResponse);
        }

        private static string GetMatchingTopic(string normalizedInput)
        {
            foreach (string keyword in TopicKeywords)
            {
                if (normalizedInput.Contains(keyword))
                {
                    return keyword;
                }
            }

            return string.Empty;
        }

        public void InitialiseMemory(string userName, string favouriteTopic)
        {
            _memory.Store("username", userName);
            _memory.Store("favouritetopic", favouriteTopic);
            _hasShownPersonalisedIntro = false;
        }

        public string GetStoredValue(string key)
        {
            return _memory.Recall(key);
        }

        private string BuildPersonalisedIntro()
        {
            if (_hasShownPersonalisedIntro)
            {
                return string.Empty;
            }

            string userName = _memory.Recall("username");
            string favouriteTopic = _memory.Recall("favouritetopic");

            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(favouriteTopic))
            {
                _hasShownPersonalisedIntro = true;
                return $"Thank you {userName}. As someone interested in {favouriteTopic},";
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                _hasShownPersonalisedIntro = true;
                return $"Thank you {userName}.";
            }

            return string.Empty;
        }

        private static string CombineResponses(params string[] parts)
        {
            List<string> nonEmptyParts = parts.Where(part => !string.IsNullOrWhiteSpace(part)).ToList();
            return string.Join("\n\n", nonEmptyParts);
        }
    }
}
