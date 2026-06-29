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
        private int _onboardingStep = 0;                                                // Track onboarding step: 0=intro, 1=name, 2=topic, 3+=done
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
        private readonly TaskManager _taskManager = new TaskManager();                  // Task manager instance for handling tasks
        private readonly ActivityLogger _activityLogger = new ActivityLogger();         // Activity logger instance for logging actions
        private bool _showingFullLog = false;                                           // Track if we're viewing full log
        private enum TaskStep
        {
            None,
            AwaitingTitle,                                                               // The bot is waiting for the user to provide a title for the task.
            AwaitingDescription,
            AwaitingRemindYesNo,
            AwaitingReminder
        }                                                                                // Enum to track the current step in the add_task creation flow (states)
        private TaskStep _taskStep = TaskStep.None;
        private string _pendingTitle = "";
        private string _pendingDescription = "";
        private Dictionary<string, Func<string, string>> _intentHandlers;               // Dictionary mapping intent names to their handler functions


        // ── NLP Intent fields ─────────────────────────────────────────────────────

        /*
         * These dictionaries map intent names to lists of phrases.
         * If the user's input contains ANY of these phrases, the intent is detected.
         * Using a Dictionary makes it easy to add more phrases later.
         */
        private static readonly Dictionary<string, List<string>> IntentPhrases = new Dictionary<string, List<string>>                                        // Creates an empty dictionary to hold intent names and their associated phrases
        {
            {
                "add_task", new List<string>                                            // Intent: user wants to add a task
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
                
                "set_reminder", new List<string>                                        // Intent: user wants to set a reminder
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
                "start_quiz", new List<string>                                          // Intent: user wants to take the quiz
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
                "show_log", new List<string>                                            // Intent: user wants to view the activity log
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
                    "show more",
                    "view log"
                }
            },
            {         
                "view_tasks", new List<string>                                          // Intent: user wants to view their tasks
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


        /*
         * The constructor initializes the chatbot's components.
         * It sets up the keyword responder, sentiment detector, intents and memory store.
         * It also initializes flags for onboarding and personalised intro.
         */
        public ChatBot()
        {
            _keywords = new KeywordResponder();                                             // Initialize the keyword responder for handling topic-based responses
            _sentiment = new SentimentDetector();                                           // Initialize the sentiment detector for analyzing user input sentiment
            _memory = new MemoryStore();
            _hasShownInitialPrompt = false;
            _intentHandlers = new Dictionary<string, Func<string, string>>                  // Creates an empty dictionary to hold key: intent_names and value: handler function/methods
            {
                { "add_task",     HandleAddTaskIntent },                                    // Needs user input so we pass it directly to the handler
                { "set_reminder", HandleSetReminderIntent }, 
                { "view_tasks",   _ =>ChatViewTasks() },                                    // Does not need user input so we ignore it with _
                { "show_log",     _ => HandleShowLogIntent() },
                { "start_quiz",   _ => "OPEN_QUIZ: Let's test your cybersecurity knowledge!" }, 
            };
        }

        /*
         * ProcessInput handles the full conversation flow step by step.
         * Step 1: Onboarding — ask for name
         * Step 2: Onboarding — ask for favourite topic
         * Step 3: Signal MainWindow that onboarding is done
         * Step 4: Task flow continuation if mid-flow
         * Step 5: NLP intent detection — runs BEFORE keyword/sentiment logic
         * Step 6: Fall through to existing keyword and sentiment logic
         */
        public string ProcessInput(string userInput)
        {
            try
            {
                if (userInput == null)                                                      // Check if userInput is null
                {
                    userInput = string.Empty;                                               // If null, set it to an empty string to avoid exceptions
                }
                
                string lower = userInput.ToLower();                                         // Convert the user input to lowercase for case-insensitive matching
                string intent = DetectIntent(lower);

                if (_onboardingStep == 0)                                                   // Onboarding Step 1: Show the first prompt asking for the user's name
                {
                    _onboardingStep++;                                                      // Increment to step 1 for the next input
                    return "Onboarding (1 of 2)\n\n" +
                           "What should I call you?";
                }

                if (_onboardingStep == 1)                                                   // Onboarding Step 2: Store the name and ask for the favourite topic
                {
                    InitialiseMemory(userInput, string.Empty);                              // Store the username in memory with an empty favourite topic
                    _onboardingStep++;
                    return "Onboarding (2 of 2)\n\n" +
                           $"Nice to meet you, {userInput}!\n\n" +
                           "What is your favourite cybersecurity topic?\n" +
                           "(e.g. passwords, phishing, malware, vpn, firewall)";
                }

                if (_onboardingStep == 2)                                                   // Onboarding Step 3: Store the favourite topic and signal MainWindow
                {
                    string currentName = _memory.Recall("username");                        // Retrieve the stored username

                    InitialiseMemory(currentName, userInput);                               // Store the favourite topic in memory
                    _onboardingStep++;
                    return "ONBOARDING_COMPLETE:" + userInput;                              // Signal MainWindow that onboarding is complete and pass the favourite topic
                }

                /*
                 * Step 4: Task flow takes priority — check BEFORE NLP intent detection
                 * Tracks states if task creation is in progress
                 * This helps the bots memory to know what to expect next from the user
                 * Without this, the bot would treat the next input from "add task" as a 
                 *  normal input and not know that it is part of the task creation flow/.
                 */
                if (_taskStep != TaskStep.None)                                             
                {
                    return ContinueTaskFlow(userInput);                                     
                }

                /*
                 * Step 5: NLP intent detection — runs BEFORE keyword/sentiment logic
                 * NLP intent detection — only runs when NOT mid-flow
                 * If an intent is detected, we look it up in the _intentHandlers dictionary.
                 * If a handler exists, we call it and return its response.
                 */
                if (_intentHandlers.TryGetValue(intent, out var handler))                   // Checks if key exists and gives value: out handler
                {
                    _activityLogger.Log(
                                            "NLP recognised intent: '" +
                                            intent +
                                            "' from: '" +
                                            userInput +
                                            "'"
                    );
                    return handler(userInput);
                }

                if (lower.Contains("complete task"))
                {
                    return ChatCompleteTask(userInput);
                }

                if (lower.Contains("delete task"))
                {
                    return ChatDeleteTask(userInput);
                }

                return BuildNormalResponse(userInput);                                  // Step 6: Else fall through to existing keyword and sentiment logic
            }
            catch (Exception ex)                                                        // Log the exception for debugging purposes
            {
                _activityLogger.Log("Error in ProcessInput: " + ex.Message);
                return "Oops! Something went wrong while processing your input. " +
                       "Please try again.";
            }
        }


        /*
         * DetectIntent() loops through every intent group in IntentPhrases.
         * For each group it checks if the input contains any of the listed phrases.
         * Returns the intent name (e.g. "add_task") or empty string if no match.
         */
        private string DetectIntent(string lowerInput)
        {
            foreach (KeyValuePair<string, List<string>> intent in IntentPhrases)        // Loop through dictionary first
            {
                string intentName = intent.Key;
                List<string> phrases = intent.Value;

                foreach (string phrase in phrases)                                      // Then check each phrase in the group
                {
                    if (lowerInput.Contains(phrase))
                    {
                        return intentName;                                              // Return as soon as we find a match
                    }
                }
            }

            return string.Empty;                                                        // Else return empty string if no intent matched
        }

        // ── NPL phrases handlers ───────────────────────────────────────────────

        /*
         * Handles the "add task" intent.
         * Extracts a task title from the user's message using string manipulation.
         */
        private string HandleAddTaskIntent(string userInput)
        {
            List<string> extractionPhrases = new List<string>
            {
                "add a task to",
                "add task to",
                "create a task to",
                "create task to",
                "i need to",
                "i want to",
                "set up",
                "enable"
            };

            string extractedTitle = ExtractFromPhrases(userInput, extractionPhrases);

            if (string.IsNullOrWhiteSpace(extractedTitle))
            {
                _taskStep = TaskStep.AwaitingTitle;
                return "Sure! What would you like to call this task? (Enter a title)";
            }

            _taskManager.AddTask(extractedTitle, "", "");
            _activityLogger.Log($"Task added: {extractedTitle}.");

            return $"TASK_ADDED: Task added: {extractedTitle}.\n\n" +
                   "Would you like to set a reminder for this task? (Yes / No)";
        }


        /*
         * Handles the "set reminder" intent.
         * Extracts what to be reminded about and when from the user's message.
         */
        private string HandleSetReminderIntent(string userInput)
        {
            string extractedTitle = ExtractFromPhrases(
                                                        userInput, 
                                                        IntentPhrases["set_reminder"]
                                                       );                               // Extracts the reminder title from the user's input using ExtractFromPhrases

            /*if (string.IsNullOrWhiteSpace(extractedTitle))
                return "What would you like me to remind you about?";
            */
            string[] timeWords = { 
                                    "tomorrow", 
                                    "today", 
                                    "tonight", 
                                    "in ", 
                                    "next ", 
                                    "on " 
                                };                                                      // Array of time-related words to look for in the extracted title
            string taskTitle = extractedTitle;
            string reminder = "No specific time set";                                   // Default reminder if no time-related words are found

            foreach (string timeWord in timeWords)                                      // Loop through the timeWords array to find a match
            {
                int index = extractedTitle.ToLower().IndexOf(timeWord);
                if (index == -1) continue;                                              // If the time word is not found, continue to the next iteration

                taskTitle = extractedTitle.Substring(0, index).Trim();
                reminder = extractedTitle.Substring(index).Trim();
                break;
            }

            _taskManager.AddTask(taskTitle, "", reminder);                              // Add the task with the extracted title and reminder
            _activityLogger.Log($"Reminder set: {taskTitle}");

            return $"TASK_ADDED: Reminder set for {taskTitle}" +
                   (reminder == "No specific time set" ? "." : " — " + reminder + ".") +
                   "\n\nI have also added this to your task list.";
        }


        /*
         * Helper method that extracts text after a matched phrase from the user's input.
         */
        private string ExtractFromPhrases(string userInput, List<string> phrases)
        {
            string lower = userInput.ToLower();

            foreach (string phrase in phrases)
            {
                if (lower.Contains(phrase))
                    return ExtractAfter(userInput, phrase);
            }

            return string.Empty;
        }


        /*
         * Handles the "show log" intent.
         * First call: shows recent activity (last 10 entries) with Show More option
         * Second call (via "show more"): shows full activity log
         */
        private string HandleShowLogIntent()
        {
            if (_showingFullLog)
            {
                string fullLog = _activityLogger.GetFullLog();                          // First call: show full log
                _showingFullLog = false;                                                // Reset flag for next call
                return "Here's your complete activity history:\n\n" + fullLog;
            }

            string recentLog = _activityLogger.GetRecentLog(10);                        // Show recent log (last 10 entries)

            StringBuilder log = new StringBuilder();
            log.AppendLine("Here's a summary of recent actions:\n");
            log.Append(recentLog);

            if (_activityLogger.GetCount() > 10)                                        // If there are more than 10 entries, show "Show More" option
            {
                log.AppendLine("\n\n[Show More] Type 'show more' to see full history");
                _showingFullLog = true;                                                 // Set flag: next "show more" will show full log
            }

            return log.ToString();                                                      // Return the recent log summary
        }

        /*
         * ExtractAfter() is a helper that returns the text that comes after
         * a given trigger phrase in the user's input.
         *
         * Example:
         *   input   = "Add a task to enable two-factor authentication"
         *   trigger = "add a task to"
         *   returns = "enable two-factor authentication"
         */
        private string ExtractAfter(string input, string trigger)
        {
            int index = input.IndexOf(
                                        trigger, 
                                        StringComparison.OrdinalIgnoreCase
                                     );                                                 // Find the index of the trigger phrase in the input

            if (index == -1)                                                            // Check if the trigger phrase was not found
                return string.Empty;                                                    // If not found, return an empty string
            return input.Substring(index + trigger.Length).Trim();                      // Move past the trigger phrase and trim any leading spaces
        }


        /*
         * GetFavouriteTopic() retrieves the user's favourite topic from memory.
         * If the user input is null, it defaults to an empty string.
         * It combines a personalised intro with the normal response for the topic.
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


        // ── Task flow methods ───────────────────────────────────────────────

        /*
         * ContinueTaskFlow() manages the multi-step task creation process.
         * It tracks the current step and prompts the user for the next piece of information.
         */
        private string ContinueTaskFlow(string input)
        {
            switch (_taskStep)
            {
                case TaskStep.AwaitingTitle:
                        _pendingTitle = input.Trim();
                        _taskStep = TaskStep.AwaitingDescription;                       // Set state to AwaitingDescription
                    return $"Got it — '{_pendingTitle}'. " +
                           "Now give a short description.";

                case TaskStep.AwaitingDescription:
                        _pendingDescription = input.Trim();
                        _taskStep = TaskStep.AwaitingRemindYesNo;
                    return "Would you like a reminder for this task? (Yes / No)";

                case TaskStep.AwaitingRemindYesNo:
                    if (input.ToLower().Contains("yes"))                                // If the user wants a reminder, prompt for the reminder time
                    {
                        _taskStep = TaskStep.AwaitingReminder;                          // Set state to AwaitingReminder
                    return "When should I remind you? " +
                               "(e.g. 'Remind me in 7 days')";
                    }

                        _taskManager.AddTask(
                                                _pendingTitle, 
                                                _pendingDescription, 
                                                ""
                                            );                                          // If no reminder, add the task without a reminder
                        _activityLogger.Log($"Task added: '{_pendingTitle}'");          // Log the task addition
                        _taskStep = TaskStep.None;                                      // Reset state to None after task creation
                    return $"TASK_ADDED: Task added: {_pendingTitle}. " +
                           "No reminder set." +
                           "\n\nType 'view tasks' to see all your tasks.";

                case TaskStep.AwaitingReminder:
                        string reminder = input.Trim();                                 // Get the reminder time from the user's input
                        _taskManager.AddTask(
                                                _pendingTitle, 
                                                _pendingDescription, reminder
                                            );
                        _activityLogger.Log($"Task added: {_pendingTitle}");            // Log the task addition with reminder
                    _taskStep = TaskStep.None;
                    return $"TASK_ADDED: Task added: {_pendingTitle}.\n\n" +
                            $"Got it! I'll remind you — {reminder}.\n\n" +
                            "Type 'view tasks' to see all your tasks.";

                default:
                    _taskStep = TaskStep.None;
                    return "Something went wrong. " +
                           "Type 'add task' to try again.";
            }
        }

        /*
         * ViewTasks() retrieves all tasks and formats them for display in the chat.
         * It shows task ID, title, description, status, reminder and creation date.
         */
        private string ChatViewTasks()
        {
            List<CyberTask> tasks = _taskManager.GetAllTasks();                         // Retrieve all tasks from the task manager

            if (tasks.Count == 0)                                                       // Check if there are tasks to display
                return "You have no tasks yet. " +
                       "Type 'add task' to create your first one.";

            StringBuilder taskList = new StringBuilder();                               // Use StringBuilder for efficient string concatenation
            taskList.AppendLine("Your Cybersecurity Tasks:\n");

            foreach (CyberTask task in tasks)                                           // Loop through each task and format its details for display
            {
                string status = task.IsComplete ? "Done" : "Pending";                   // If task is complete, set Done else set Pending
                string reminder = task.Reminder == "" ? "None" : task.Reminder;         // If no reminder is set, display None

                taskList.AppendLine($"[{task.Id}] {task.Title}  —  {status}");
                taskList.AppendLine($"     📝 {task.Description}");
                taskList.AppendLine($"     ⏰ Reminder : {reminder}");
                taskList.AppendLine($"     🕐 Added    : {task.CreatedAt}");
                taskList.AppendLine();
            }

            taskList.AppendLine("──────────────────────────────");
            taskList.AppendLine("To complete : 'complete task 1'");
            taskList.AppendLine("To delete   : 'delete task 2'");

            return taskList.ToString();
        }

        /*
         * CompleteTaskFromChat() marks a task as complete based on user input.
         * It extracts the task ID from the input and updates the task status.
         */
        private string ChatCompleteTask(string input)
        {
            string[] words = input.Split(' ');                                          // Split the input into words to find the task ID

            foreach (string word in words)
            {
                int id;
                if (int.TryParse(word, out id))
                {
                    _activityLogger.Log(
                                          $"Task marked complete: Task {id} + " +
                                          ""
                                       );                                               // Log the task completion action

                    _taskManager.MarkAsComplete(id);
                    return $"Task {id} + marked as complete. " +
                           "Great work on your cybersecurity!";
                }
            }

            return "Please tell me which task number to complete." +
                   "\nExample: complete task 2";
        }

        /*
         * DeleteTaskFromChat() removes a task based on user input.
         * It extracts the task ID from the input and deletes the task.
         */
        private string ChatDeleteTask(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int id;

                if (int.TryParse(word, out id))
                {
                    _activityLogger.Log("Task deleted: 'Task " + id + "'");

                    _taskManager.DeleteTask(id);
                    return $"Task {id} has been deleted.";
                }
            }

            return "Please tell me which task number to delete." +
                   "\nExample: \"delete task 3\"";
        }


        // ── Normal response methods ───────────────────────────────────────────────

        private string BuildNormalResponse(string userInput)
        {
            string normalizedInput = userInput?.ToLowerInvariant();                     // Normalize the input to lowercase for consistent processing

            bool isFollowUpRequest = normalizedInput.Contains("tell me more") ||
                                     normalizedInput.Contains("more details") ||
                                     normalizedInput == "more";
            string keywordsResponse;                                                    // Variable to hold the response from the keyword responder

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
                keywordsResponse = _keywords.GetResponse(userInput);

                string matchedTopic = GetMatchingTopic(normalizedInput);
                if (!string.IsNullOrWhiteSpace(matchedTopic))
                {
                    _memory.Store(CurrentTopicKey, matchedTopic);
                    _activityLogger.Log(
                                            $"Keyword matched: {matchedTopic}" +
                                            " - response delivered");
                }
            }

            Sentiment sentiment = _sentiment.Detect(userInput);                             // Detect the sentiment of the user's input
            string sentimentResponse = _sentiment.GetSentimentResponse(sentiment);          // Get a response based on the detected sentiment
            string intro = BuildPersonalisedIntro();                                        // Build a personalised intro if applicable

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

        /*
         * InitialiseMemory() stores the user's name and favourite topic in memory.
         */
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

        /*
         * BuildPersonalisedIntro() constructs a personalised introduction message
         * based on the user's stored name and favourite topic.
         * It ensures the intro is only shown once per session.
         */
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
