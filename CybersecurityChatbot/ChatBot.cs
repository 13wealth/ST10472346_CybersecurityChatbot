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
        private readonly TaskManager _taskManager = new TaskManager();                                          // Initialize the TaskManager to handle task-related commands and interactions

        public ChatBot()
        {
            _keywords = new KeywordResponder();                                                                 // Initialize the KeywordResponder to handle keyword-based responses
            _sentiment = new SentimentDetector();                                                               // Initialize the SentimentDetector to analyze user sentiment
            _memory = new MemoryStore();                                                                        // Initialize the MemoryStore to store user data and preferences
            _hasShownInitialPrompt = false;                                                                     // Flag to track if the initial prompt has been shown
        }

        /*
 * ProcessInput handles the conversation flow step by step.
 * Step 1: Ask for name (onboarding 1 of 2)
 * Step 2: Ask for favourite topic (onboarding 2 of 2)
 * Step 3: Signal that onboarding is done so MainWindow can call GetTopicResponse and GetMenuPrompt
 * Step 4: Handle task commands and normal responses after onboarding
 */
        public string ProcessInput(string userInput)
        {
            // Onboarding Step 1: Show the first prompt asking for the user's name
            if (!_hasShownInitialPrompt)
            {
                _hasShownInitialPrompt = true;
                return "⚙️ Onboarding (1 of 2)\n\nWhat should I call you?";
            }

            // Onboarding Step 2: Store the name and ask for the favourite topic
            if (string.IsNullOrWhiteSpace(_memory.Recall("username")))
            {
                InitialiseMemory(userInput, string.Empty);
                return $"Nice to meet you, {userInput}!\n\n⚙️ Onboarding (2 of 2)\n\nWhat is your favourite cybersecurity topic?\n(e.g. passwords, phishing, malware, vpn, firewall)";
            }

            // Onboarding Step 3: Store the favourite topic and signal MainWindow
            // MainWindow will call GetTopicResponse() and GetMenuPrompt() separately
            if (string.IsNullOrWhiteSpace(_memory.Recall("favouritetopic")))
            {
                string currentName = _memory.Recall("username");
                string safeInput = userInput ?? string.Empty;

                InitialiseMemory(currentName, safeInput);

                // Return this signal so MainWindow knows onboarding just finished
                return "ONBOARDING_COMPLETE:" + safeInput;
            }

            // Onboarding is done — handle task commands from here
            if (_taskManager.IsActive())
            {
                return _taskManager.HandleInput(userInput);
            }

            if (_taskManager.IsTaskCommand(userInput ?? string.Empty))
            {
                return _taskManager.HandleInput(userInput);
            }

            // Default — send to normal keyword response
            return BuildNormalResponse(userInput ?? string.Empty);
        }


        /*
         * Called by MainWindow after onboarding completes.
         * Builds the topic response using the favourite topic the user just entered.
         * This is the first of the two separate bubbles shown after onboarding.
         */
        public string GetFavouriteTopic(string userInput)
        {
            string safeInput = userInput ?? string.Empty;

            return CombineResponses
            (
                BuildPersonalisedIntro(),
                BuildNormalResponse(safeInput)
            );
        }


        /*
         * Called by MainWindow after GetTopicResponse.
         * Returns the menu as a separate bubble so it is visually distinct
         * from the topic response above it.
         * This is the second of the two separate bubbles shown after onboarding.
         */
        public string GetMenuPrompt()
        {
            return
                "──────────────────────────────────────\n" +
                "Here is what I can help you with:\n\n" +
                "💬  Ask me anything about cybersecurity\n" +
                "       e.g. \"Tell me about phishing\"\n\n" +
                "📋  Manage your cybersecurity tasks\n" +
                "       e.g. \"add task\" | \"view tasks\"\n" +
                "            \"complete task 1\" | \"delete task 2\"\n\n" +
                "🔎  Dive deeper into any topic\n" +
                "       e.g. \"tell me more\" | \"more details\"\n" +
                "──────────────────────────────────────";
        }


        /*
         * Builds a normal response based on the user input.
         * It checks for follow-up requests, retrieves responses from the keyword responder,
         * detects sentiment, and combines all parts into a final response.
         */
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
                    keywordsResponse = "Sorry, I don't have information on that topic." +
                                       " Please try asking about something else.";
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