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
        private bool _hasShownPersonalisedIntro;

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
        }

        public string GetInitialPrompt()
        {
            return "What should I call you?";
        }

        public string ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(_memory.Recall("username")))
            {
                InitialiseMemory(userInput, string.Empty);
                return $"Thanks {userInput}! And what's your favourite cybersecurity topic?";
            }

            if (string.IsNullOrWhiteSpace(_memory.Recall("favouritetopic")))
            {
                string currentName = _memory.Recall("username");
                InitialiseMemory(currentName, userInput);

                return CombineResponses(
                    $"Thanks {currentName}. We can start chatting now.",
                    BuildPersonalisedIntro(),
                    BuildNormalResponse(userInput));
            }

            return BuildNormalResponse(userInput);
        }

        private string BuildNormalResponse(string userInput)
        {
            Sentiment sentiment = _sentiment.Detect(userInput);
            string sentimentResponse = _sentiment.GetSentimentResponse(sentiment);
            string keywordsResponse = _keywords.GetResponse(userInput);
            string intro = BuildPersonalisedIntro();

            return CombineResponses(intro, sentimentResponse, keywordsResponse);
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
