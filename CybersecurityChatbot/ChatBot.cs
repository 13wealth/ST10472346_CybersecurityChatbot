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
        // ── Existing fields ───────────────────────────────────────────────────────
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

        // TaskManager handles the four CRUD operations
        private readonly TaskManager _taskManager = new TaskManager();

        // ── Task chat flow fields ─────────────────────────────────────────────────

        /*
         * TaskStep tracks which step of the add-task conversation the user is on.
         * It works the same way as the onboarding steps above.
         * None means no task flow is currently active.
         */
        private enum TaskStep
        {
            None,
            AwaitingTitle,
            AwaitingDescription,
            AwaitingRemindYesNo,
            AwaitingReminder
        }

        private TaskStep _taskStep = TaskStep.None; // Current step in the task flow
        private string _pendingTitle = "";          // Temporarily holds the title
        private string _pendingDescription = "";          // Temporarily holds the description


        // ── Constructor ───────────────────────────────────────────────────────────

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
            _hasShownInitialPrompt = false;
        }


        // ── ProcessInput ──────────────────────────────────────────────────────────

        /*
         * ProcessInput handles the full conversation flow step by step.
         * Step 1: Ask for name            (onboarding 1 of 2)
         * Step 2: Ask for favourite topic  (onboarding 2 of 2)
         * Step 3: Signal MainWindow that onboarding is done
         * Step 4: Handle task commands and normal responses after onboarding
         */
        public string ProcessInput(string userInput)
        {
            // Onboarding Step 1: Show the first prompt asking for the user's name
            if (!_hasShownInitialPrompt)
            {
                _hasShownInitialPrompt = true;
                return "⚙️ Onboarding (1 of 2)\n\n" +
                       "What should I call you?";
            }

            // Onboarding Step 2: Store the name and ask for the favourite topic
            if (string.IsNullOrWhiteSpace(_memory.Recall("username")))
            {
                InitialiseMemory(userInput, string.Empty);
                return "⚙️ Onboarding (2 of 2)\n\n" +
                       $"Nice to meet you, {userInput}!\n\n" +
                       "What is your favourite cybersecurity topic?\n" +
                       "(e.g. passwords, phishing, malware, vpn, firewall)";
            }

            // Onboarding Step 3: Store the favourite topic and signal MainWindow
            // MainWindow checks for "ONBOARDING_COMPLETE:" to know onboarding finished
            if (string.IsNullOrWhiteSpace(_memory.Recall("favouritetopic")))
            {
                string currentName = _memory.Recall("username");
                string safeInput = userInput ?? string.Empty;

                InitialiseMemory(currentName, safeInput);

                // MainWindow will call GetFavouriteTopic() and GetMenuPrompt() separately
                return "ONBOARDING_COMPLETE:" + safeInput;
            }

            // ── Onboarding complete — task and normal flow from here ──────────────

            // If the user is mid-flow adding a task via chat, continue that flow
            if (_taskStep != TaskStep.None)
            {
                return ContinueTaskFlow(userInput ?? string.Empty);
            }

            // Check if the user typed a task command
            string lower = (userInput ?? string.Empty).ToLower();

            if (lower.Contains("add task") || lower.Contains("new task"))
            {
                _taskStep = TaskStep.AwaitingTitle;
                return "Sure! What would you like to call this task? (Enter a title)";
            }

            if (lower.Contains("view tasks") || lower.Contains("show tasks") ||
                lower.Contains("list tasks") || lower.Contains("my tasks"))
            {
                return ViewTasksForChat();
            }

            if (lower.Contains("complete task"))
            {
                return CompleteTaskFromChat(userInput ?? string.Empty);
            }

            if (lower.Contains("delete task"))
            {
                return DeleteTaskFromChat(userInput ?? string.Empty);
            }

            // Default — send to normal keyword response
            return BuildNormalResponse(userInput ?? string.Empty);
        }


        // ── Onboarding response methods ───────────────────────────────────────────

        /*
         * Called by MainWindow after onboarding completes.
         * Returns the topic response as the first bubble after onboarding.
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
         * Called by MainWindow after GetFavouriteTopic.
         * Returns the menu as a second separate bubble after onboarding.
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


        // ── Task chat flow methods ────────────────────────────────────────────────

        /*
         * Handles each step of the add-task conversation.
         * Called when the user is mid-flow — e.g. they typed "add task"
         * and we are now collecting the title, description, and reminder one step at a time.
         */
        private string ContinueTaskFlow(string input)
        {
            switch (_taskStep)
            {
                // Step 1: User just entered the title
                case TaskStep.AwaitingTitle:
                    _pendingTitle = input.Trim();
                    _taskStep = TaskStep.AwaitingDescription;
                    return "Got it — \"" + _pendingTitle + "\". Now give a short description.";

                // Step 2: User just entered the description
                case TaskStep.AwaitingDescription:
                    _pendingDescription = input.Trim();
                    _taskStep = TaskStep.AwaitingRemindYesNo;
                    return "Would you like a reminder for this task? (Yes / No)";

                // Step 3: User answered Yes or No to the reminder question
                case TaskStep.AwaitingRemindYesNo:
                    if (input.ToLower().Contains("yes"))
                    {
                        _taskStep = TaskStep.AwaitingReminder;
                        return "When should I remind you? (e.g. \"Remind me in 7 days\")";
                    }
                    else
                    {
                        // No reminder — save the task with an empty reminder
                        _taskManager.AddTask(_pendingTitle, _pendingDescription, "");
                        _taskStep = TaskStep.None;
                        // Signal MainWindow to refresh the UI by prefixing the response
                        return "TASK_ADDED:✅ Task added: \"" + _pendingTitle + "\". No reminder set.\n\n" +
                               "Type \"view tasks\" to see all your tasks.";
                    }

                // Step 4: User entered the reminder details
                case TaskStep.AwaitingReminder:
                    string reminder = input.Trim();
                    _taskManager.AddTask(_pendingTitle, _pendingDescription, reminder);
                    _taskStep = TaskStep.None;
                    // Signal MainWindow to refresh the UI by prefixing the response
                    return "TASK_ADDED:✅ Task added: \"" + _pendingTitle + "\".\n\n" +
                           "Got it! I'll remind you — " + reminder + ".\n\n" +
                           "Type \"view tasks\" to see all your tasks.";

                // Safety fallback
                default:
                    _taskStep = TaskStep.None;
                    return "Something went wrong. Type \"add task\" to try again.";
            }
        }

        /*
         * Loads all tasks and formats them for display in the chat.
         */
        private string ViewTasksForChat()
        {
            List<CyberTask> tasks = _taskManager.GetAllTasks();

            if (tasks.Count == 0)
                return "You have no tasks yet. Type \"add task\" to create your first one.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 Your Cybersecurity Tasks:\n");

            for (int i = 0; i < tasks.Count; i++)
            {
                CyberTask task = tasks[i];

                string status = task.IsComplete ? "✅ Done" : "🔲 Pending";
                string reminder = (task.Reminder == "") ? "None" : task.Reminder;

                sb.AppendLine("[" + task.Id + "] " + task.Title + "  —  " + status);
                sb.AppendLine("     📝 " + task.Description);
                sb.AppendLine("     ⏰ Reminder : " + reminder);
                sb.AppendLine("     🕐 Added    : " + task.CreatedAt);
                sb.AppendLine();
            }

            sb.AppendLine("──────────────────────────────");
            sb.AppendLine("To complete : \"complete task 1\"");
            sb.AppendLine("To delete   : \"delete task 2\"");

            return sb.ToString();
        }

        /*
         * Extracts the task ID from the chat message and marks it complete.
         * Example: "complete task 2" → finds "2" → marks task 2 as done.
         */
        private string CompleteTaskFromChat(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int id;
                if (int.TryParse(word, out id))
                {
                    _taskManager.MarkAsComplete(id);
                    return "✅ Task " + id + " marked as complete. Great work on your cybersecurity!";
                }
            }

            return "Please tell me which task number to complete.\nExample: \"complete task 2\"";
        }

        /*
         * Extracts the task ID from the chat message and deletes it.
         * Example: "delete task 3" → finds "3" → removes task 3.
         */
        private string DeleteTaskFromChat(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int id;
                if (int.TryParse(word, out id))
                {
                    _taskManager.DeleteTask(id);
                    return "🗑️ Task " + id + " has been deleted.";
                }
            }

            return "Please tell me which task number to delete.\nExample: \"delete task 3\"";
        }


        // ── Normal response methods ───────────────────────────────────────────────

        /*
         * Builds a normal response based on the user input.
         * Checks for follow-up requests, gets keyword responses,
         * detects sentiment, and combines everything into one response.
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
                    keywordsResponse = "Sorry, I don't have information on that topic. " +
                                       "Please try asking about something else.";
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
                return string.Empty;

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