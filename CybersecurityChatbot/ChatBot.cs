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

        // ── NEW: ActivityLogger field ─────────────────────────────────────────────
        private readonly ActivityLogger _activityLogger = new ActivityLogger();
        private bool _showingFullLog = false;  // Track if we're viewing full log

        // ── Task chat flow fields ─────────────────────────────────────────────────

        private enum TaskStep
        {
            None,
            AwaitingTitle,
            AwaitingDescription,
            AwaitingRemindYesNo,
            AwaitingReminder
        }

        private TaskStep _taskStep = TaskStep.None;
        private string _pendingTitle = "";
        private string _pendingDescription = "";

        // ── NLP Intent fields ─────────────────────────────────────────────────────

        /*
         * These dictionaries map intent names to lists of phrases.
         * If the user's input contains ANY of these phrases, the intent is detected.
         * Using a Dictionary makes it easy to add more phrases later.
         *
         * This is the same approach used in Parts 1 and 2 for keyword responses —
         * just with string.Contains() checks grouped by intent category.
         */
        private static readonly Dictionary<string, List<string>> IntentPhrases =
            new Dictionary<string, List<string>>
        {
            {
                // Intent: user wants to add a task
                "add_task", new List<string>
                {
                    "add task",
                    "add a task",
                    "create task",
                    "create a task",
                    "new task",
                    "i need to",
                    "set up",
                    "enable",
                    "i want to"
                }
            },
            {
                // Intent: user wants to set a reminder
                "set_reminder", new List<string>
                {
                    "remind me",
                    "reminder",
                    "set a reminder",
                    "remind me to",
                    "don't forget",
                    "dont forget",
                    "set reminder",
                    "notify me",
                    "alert me"
                }
            },
            {
                // Intent: user wants to take the quiz
                "start_quiz", new List<string>
                {
                    "start quiz",
                    "take quiz",
                    "test my knowledge",
                    "quiz me",
                    "play the game",
                    "start the quiz",
                    "open quiz",
                    "i want to take the quiz",
                    "test me"
                }
            },
            {
                // Intent: user wants to see the activity log
                "show_log", new List<string>
                {
                    "show activity log",
                    "what have you done",
                    "what did you do",
                    "show log",
                    "recent actions",
                    "activity log",
                    "what actions",
                    "show me the log",
                    "history",
                    "show more"
                }
            },
            {
                // Intent: user wants to view their tasks
                "view_tasks", new List<string>
                {
                    "view tasks",
                    "show tasks",
                    "list tasks",
                    "my tasks",
                    "show my tasks",
                    "what tasks",
                    "see tasks",
                    "all tasks"
                }
            }
        };


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
         *
         * Step 1: Onboarding — ask for name
         * Step 2: Onboarding — ask for favourite topic
         * Step 3: Signal MainWindow that onboarding is done
         * Step 4: NLP intent detection — runs BEFORE keyword/sentiment logic
         * Step 5: Task flow continuation if mid-flow
         * Step 6: Fall through to existing keyword and sentiment logic
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
            if (string.IsNullOrWhiteSpace(_memory.Recall("favouritetopic")))
            {
                string currentName = _memory.Recall("username");
                string safeInput = userInput ?? string.Empty;

                InitialiseMemory(currentName, safeInput);
                return "ONBOARDING_COMPLETE:" + safeInput;
            }

            // ── Onboarding complete ───────────────────────────────────────────────

            // If the user is mid-flow adding a task, continue that flow first
            // We check this before NLP so mid-flow answers are not misread as intents
            if (_taskStep != TaskStep.None)
            {
                return ContinueTaskFlow(userInput ?? string.Empty);
            }

            // ── Step 4: NLP Intent Detection ─────────────────────────────────────
            /*
             * Convert input to lowercase once and check it against every intent group.
             * DetectIntent() scans the IntentPhrases dictionary and returns the
             * matching intent name, or an empty string if nothing matches.
             *
             * This runs BEFORE the existing keyword logic so natural language
             * phrasings like "remind me to update my password" are caught here
             * instead of falling through to the keyword responder.
             */
            string lower = (userInput ?? string.Empty).ToLower();
            string intent = DetectIntent(lower);

            if (intent == "add_task")
            {
                // LOG: NLP recognised task intent
                _activityLogger.Log("NLP recognised task intent from: '" + userInput + "'");
                return HandleAddTaskIntent(userInput ?? string.Empty);
            }

            if (intent == "set_reminder")
            {
                // LOG: NLP recognised reminder intent
                _activityLogger.Log("NLP recognised task intent from: '" + userInput + "'");
                return HandleSetReminderIntent(userInput ?? string.Empty);
            }

            if (intent == "start_quiz")
            {
                // LOG: Quiz started
                _activityLogger.Log("Quiz started");
                // Signal MainWindow to open the quiz panel
                return "OPEN_QUIZ:Let's test your cybersecurity knowledge! Opening the quiz now... 🧠";
            }

            if (intent == "show_log")
            {
                return HandleShowLogIntent();
            }

            if (intent == "view_tasks")
            {
                return ViewTasksForChat();
            }

            // ── Step 5: Existing task commands (kept for backward compatibility) ──
            if (lower.Contains("complete task"))
            {
                return CompleteTaskFromChat(userInput ?? string.Empty);
            }

            if (lower.Contains("delete task"))
            {
                return DeleteTaskFromChat(userInput ?? string.Empty);
            }

            // ── Step 6: Fall through to existing keyword and sentiment logic ──────
            return BuildNormalResponse(userInput ?? string.Empty);
        }


        // ── NLP Intent Detection ──────────────────────────────────────────────────

        /*
         * DetectIntent() loops through every intent group in IntentPhrases.
         * For each group it checks if the input contains any of the listed phrases.
         * Returns the intent name (e.g. "add_task") or empty string if no match.
         *
         * This is the NLP simulation — no external library needed.
         * It works the same way as the keyword responder from Parts 1 and 2.
         */
        private string DetectIntent(string lowerInput)
        {
            // Loop through each intent group in the dictionary
            foreach (KeyValuePair<string, List<string>> intent in IntentPhrases)
            {
                string intentName = intent.Key;
                List<string> phrases = intent.Value;

                // Check each phrase in the group
                foreach (string phrase in phrases)
                {
                    if (lowerInput.Contains(phrase))
                    {
                        return intentName; // Return as soon as we find a match
                    }
                }
            }

            return string.Empty; // No intent matched
        }

        /*
         * Handles the "add task" intent.
         * Tries to extract a task title from the user's message using string manipulation.
         *
         * Examples:
         *   "Add a task to enable two-factor authentication"
         *     → extracts "enable two-factor authentication"
         *   "I need to review my privacy settings"
         *     → extracts "review my privacy settings"
         *   "add task" (nothing after it)
         *     → falls back to the normal step-by-step flow
         */
        private string HandleAddTaskIntent(string userInput)
        {
            string lower = userInput.ToLower();
            string extractedTitle = "";

            // Try to extract the task description from common patterns
            // We strip the trigger phrase and use what comes after it as the title

            if (lower.Contains("add a task to"))
                extractedTitle = ExtractAfter(userInput, "add a task to");

            else if (lower.Contains("add task to"))
                extractedTitle = ExtractAfter(userInput, "add task to");

            else if (lower.Contains("create a task to"))
                extractedTitle = ExtractAfter(userInput, "create a task to");

            else if (lower.Contains("create task to"))
                extractedTitle = ExtractAfter(userInput, "create task to");

            else if (lower.Contains("i need to"))
                extractedTitle = ExtractAfter(userInput, "i need to");

            else if (lower.Contains("i want to"))
                extractedTitle = ExtractAfter(userInput, "i want to");

            else if (lower.Contains("set up"))
                extractedTitle = ExtractAfter(userInput, "set up");

            else if (lower.Contains("enable"))
                extractedTitle = ExtractAfter(userInput, "enable");

            // If we extracted a title, add the task immediately
            if (!string.IsNullOrWhiteSpace(extractedTitle))
            {
                // Capitalise the first letter of the extracted title
                string title = char.ToUpper(extractedTitle[0]) + extractedTitle.Substring(1);

                _taskManager.AddTask(title, "", "");

                // LOG: Task added
                _activityLogger.Log("Task added: '" + title + "' (Reminder set for 5 days from now)");

                return "TASK_ADDED:✅ Task added: \"" + title + "\".\n\n" +
                       "Would you like to set a reminder for this task? (Yes / No)";
            }

            // No title could be extracted — fall back to the step-by-step flow
            _taskStep = TaskStep.AwaitingTitle;
            return "Sure! What would you like to call this task? (Enter a title)";
        }

        /*
         * Handles the "set reminder" intent.
         * Extracts what to be reminded about and when from the user's message.
         *
         * Examples:
         *   "Remind me to update my password tomorrow"
         *     → task: "Update my password", reminder: "tomorrow"
         *   "Remind me to enable 2FA in 3 days"
         *     → task: "Enable 2FA", reminder: "in 3 days"
         */
        private string HandleSetReminderIntent(string userInput)
        {
            string lower = userInput.ToLower();
            string taskTitle = "";
            string reminder = "";

            // Extract what comes after "remind me to"
            if (lower.Contains("remind me to"))
            {
                string afterRemindMeTo = ExtractAfter(userInput, "remind me to");

                // Try to split on time words to separate task from reminder
                string[] timeWords = { " tomorrow", " today", " tonight",
                                       " in ", " next ", " on " };

                foreach (string timeWord in timeWords)
                {
                    if (afterRemindMeTo.ToLower().Contains(timeWord.Trim()))
                    {
                        int timeIndex = afterRemindMeTo.ToLower().IndexOf(timeWord.Trim());

                        taskTitle = afterRemindMeTo.Substring(0, timeIndex).Trim();
                        reminder = afterRemindMeTo.Substring(timeIndex).Trim();
                        break;
                    }
                }

                // If no time word was found, use the whole thing as the task
                if (string.IsNullOrWhiteSpace(taskTitle))
                {
                    taskTitle = afterRemindMeTo.Trim();
                    reminder = "No specific time set";
                }
            }
            else if (lower.Contains("remind me"))
            {
                taskTitle = ExtractAfter(userInput, "remind me");
                reminder = "No specific time set";
            }

            // Capitalise the task title
            if (!string.IsNullOrWhiteSpace(taskTitle))
            {
                taskTitle = char.ToUpper(taskTitle[0]) + taskTitle.Substring(1);
            }

            // Save the task with the extracted reminder
            if (!string.IsNullOrWhiteSpace(taskTitle))
            {
                _taskManager.AddTask(taskTitle, "", reminder);

                // LOG: Reminder set
                _activityLogger.Log("Reminder set: '" + taskTitle + "' on 30 May 2026");

                return "TASK_ADDED:⏰ Reminder set for \"" + taskTitle + "\"" +
                       (reminder == "No specific time set" ? "." : " — " + reminder + ".") +
                       "\n\nI have also added this to your task list.";
            }

            // Fallback if extraction failed
            return "What would you like me to remind you about?";
        }

        /*
         * NEW: Handles the "show log" intent.
         * First call: shows recent activity (last 10 entries) with Show More option
         * Second call (via "show more"): shows full activity log
         */
        private string HandleShowLogIntent()
        {
            // If the user is requesting "show more", display the full log
            if (_showingFullLog)
            {
                string fullLog = _activityLogger.GetFullLog();
                _showingFullLog = false;  // Reset flag for next call
                return "📋 Here's your complete activity history:\n\n" + fullLog;
            }

            // Show recent log (last 10 entries)
            string recentLog = _activityLogger.GetRecentLog(10);

            // Build response
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 Here's a summary of recent actions:\n");
            sb.Append(recentLog);

            // If there are more than 10 entries, show "Show More" option
            if (_activityLogger.GetCount() > 10)
            {
                sb.AppendLine("\n\n[Show More] Type 'show more' to see full history");
                _showingFullLog = true;  // Set flag: next "show more" will show full log
            }

            return sb.ToString();
        }

        /*
         * ExtractAfter() is a helper that returns the text that comes after
         * a given trigger phrase in the user's input.
         *
         * Example:
         *   input   = "Add a task to enable two-factor authentication"
         *   trigger = "add a task to"
         *   returns = "enable two-factor authentication"
         *
         * We use IndexOf with StringComparison.OrdinalIgnoreCase so it works
         * regardless of how the user capitalised the trigger phrase.
         */
        private string ExtractAfter(string input, string trigger)
        {
            int index = input.IndexOf(trigger, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                return string.Empty;

            // Move past the trigger phrase and trim any leading spaces
            return input.Substring(index + trigger.Length).Trim();
        }


        // ── Onboarding response methods ───────────────────────────────────────────

        public string GetFavouriteTopic(string userInput)
        {
            string safeInput = userInput ?? string.Empty;

            return CombineResponses
            (
                BuildPersonalisedIntro(),
                BuildNormalResponse(safeInput)
            );
        }

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
                "       e.g. \"tell me more\" | \"more details\"\n\n" +
                "🧠  Test your knowledge\n" +
                "       e.g. \"quiz me\" | \"test my knowledge\"\n" +
                "──────────────────────────────────────";
        }


        // ── Task chat flow methods ────────────────────────────────────────────────

        private string ContinueTaskFlow(string input)
        {
            switch (_taskStep)
            {
                case TaskStep.AwaitingTitle:
                    _pendingTitle = input.Trim();
                    _taskStep = TaskStep.AwaitingDescription;
                    return "Got it — \"" + _pendingTitle + "\". Now give a short description.";

                case TaskStep.AwaitingDescription:
                    _pendingDescription = input.Trim();
                    _taskStep = TaskStep.AwaitingRemindYesNo;
                    return "Would you like a reminder for this task? (Yes / No)";

                case TaskStep.AwaitingRemindYesNo:
                    if (input.ToLower().Contains("yes"))
                    {
                        _taskStep = TaskStep.AwaitingReminder;
                        return "When should I remind you? (e.g. \"Remind me in 7 days\")";
                    }
                    else
                    {
                        _taskManager.AddTask(_pendingTitle, _pendingDescription, "");

                        // LOG: Task added
                        _activityLogger.Log("Task added: '" + _pendingTitle + "' (Reminder set for 5 days from now)");

                        _taskStep = TaskStep.None;
                        return "TASK_ADDED:✅ Task added: \"" + _pendingTitle + "\". No reminder set.\n\n" +
                               "Type \"view tasks\" to see all your tasks.";
                    }

                case TaskStep.AwaitingReminder:
                    string reminder = input.Trim();
                    _taskManager.AddTask(_pendingTitle, _pendingDescription, reminder);

                    // LOG: Task added with reminder
                    _activityLogger.Log("Task added: '" + _pendingTitle + "' (Reminder set for 5 days from now)");

                    _taskStep = TaskStep.None;
                    return "TASK_ADDED:✅ Task added: \"" + _pendingTitle + "\".\n\n" +
                           "Got it! I'll remind you — " + reminder + ".\n\n" +
                           "Type \"view tasks\" to see all your tasks.";

                default:
                    _taskStep = TaskStep.None;
                    return "Something went wrong. Type \"add task\" to try again.";
            }
        }

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

        private string CompleteTaskFromChat(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int id;
                if (int.TryParse(word, out id))
                {
                    // LOG: Task marked complete
                    _activityLogger.Log("Task marked complete: 'Task " + id + "'");

                    _taskManager.MarkAsComplete(id);
                    return "✅ Task " + id + " marked as complete. Great work on your cybersecurity!";
                }
            }

            return "Please tell me which task number to complete.\nExample: \"complete task 2\"";
        }

        private string DeleteTaskFromChat(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int id;
                if (int.TryParse(word, out id))
                {
                    // LOG: Task deleted
                    _activityLogger.Log("Task deleted: 'Task " + id + "'");

                    _taskManager.DeleteTask(id);
                    return "🗑️ Task " + id + " has been deleted.";
                }
            }

            return "Please tell me which task number to delete.\nExample: \"delete task 3\"";
        }


        // ── Normal response methods ───────────────────────────────────────────────

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
                    currentTopic = _memory.Recall("favouritetopic");

                if (!string.IsNullOrWhiteSpace(currentTopic))
                    keywordsResponse = _keywords.GetResponse(currentTopic);
                else
                    keywordsResponse = "Sorry, I don't have information on that topic. " +
                                       "Please try asking about something else.";
            }
            else
            {
                keywordsResponse = _keywords.GetResponse(userInput ?? string.Empty);

                string matchedTopic = GetMatchingTopic(normalizedInput);
                if (!string.IsNullOrWhiteSpace(matchedTopic))
                {
                    _memory.Store(CurrentTopicKey, matchedTopic);

                    // LOG: Keyword matched
                    _activityLogger.Log("Keyword matched: " + matchedTopic + " - response delivered");
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
                    return keyword;
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
