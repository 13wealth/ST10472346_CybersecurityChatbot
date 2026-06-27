using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * TaskManager handles the full conversation flow for the Task Assistant feature.
     * It guides the user through adding, viewing, completing, and deleting tasks using a state system 
     * It is called by ChatBot.cs when the user types a task-related command
     */
    public class TaskManager
    {
        /*
         * The TaskState enum represents the different states of the task flow.
         * It is used to track where the user is in the task flow and what input is expected next.
         */
        private enum TaskStep
        {
            None,                                                                                           // Not currently in a task flow
            AwaitingTitle,                                                                                  // Waiting for the user to enter a task title
            AwaitingDescription,                                                                            // Waiting for the user to enter a description
            AwaitingRemindYesNo,                                                                            // Waiting for Yes or No to "do you want a reminder?"
            AwaitingReminder                                                                                // Waiting for the actual reminder details
        }

        // This variable tracks which step the conversation is currently on
        // It starts at None because no task flow is active when the app loads
        private TaskStep _currentStep = TaskStep.None;

        // These hold the task details while the user is busy filling them in
        private string _pendingTitle = "";
        private string _pendingDescription = "";

        // This is the helper class that does all the file reading and writing
        private TaskStorageHelper _storage = new TaskStorageHelper();


        // ── Is the task flow currently active? ───────────────────────────────────

        /*
         * ChatBot calls this to check if the user is in the middle of adding a task.
         * If true, all input must come here instead of the normal chatbot logic.
         */
        public bool IsActive()
        {
            return _currentStep != TaskStep.None;
        }


        // ── Is this a task-related command? ──────────────────────────────────────

        /*
         * ChatBot calls this to check if the user typed a task command.
         * We convert the input to lowercase so it works regardless of capitalisation.
         * Example: "Add Task", "ADD TASK", and "add task" all match.
         */
        public bool IsTaskCommand(string? input)
        {
            string lower = (input ?? string.Empty).ToLower();

            if (lower.Contains("add task")) return true;
            if (lower.Contains("new task")) return true;
            if (lower.Contains("view tasks")) return true;
            if (lower.Contains("show tasks")) return true;
            if (lower.Contains("list tasks")) return true;
            if (lower.Contains("my tasks")) return true;
            if (lower.Contains("complete task")) return true;
            if (lower.Contains("mark complete")) return true;
            if (lower.Contains("delete task")) return true;
            if (lower.Contains("remove task")) return true;

            return false;
        }


        // ── Main entry point ─────────────────────────────────────────────────────

        /*
         * ChatBot calls HandleInput() with the user's message.
         * If a task flow is already active, we continue it.
         * If not, we check what command was typed and start the right action.
         */
        public string HandleInput(string? input)
        {
            input ??= string.Empty;

            // If the user is mid-flow (e.g. busy adding a task), continue that flow
            if (_currentStep != TaskStep.None)
            {
                return ContinueAddingTask(input);
            }

            // Otherwise, check which command was typed and respond accordingly
            string lower = input.ToLower();

            if (lower.Contains("add task") || lower.Contains("new task"))
            {
                return StartAddingTask();
            }

            if (lower.Contains("view tasks") || lower.Contains("show tasks") ||
                lower.Contains("list tasks") || lower.Contains("my tasks"))
            {
                return ViewTasks();
            }

            if (lower.Contains("complete task") || lower.Contains("mark complete"))
            {
                return CompleteTask(input);
            }

            if (lower.Contains("delete task") || lower.Contains("remove task"))
            {
                return DeleteTask(input);
            }

            // Fallback — should not normally reach here
            return "I did not understand that task command. Try typing \"add task\" or \"view tasks\".";
        }


        // ── Step 1: Start the add task flow ──────────────────────────────────────

        /*
         * This is called when the user types "add task".
         * We set the step to AwaitingTitle so the next message is treated as the title.
         */
        private string StartAddingTask()
        {
            _currentStep = TaskStep.AwaitingTitle;
            return "Sure! What would you like to call this task? (Enter a title)";
        }


        // ── Steps 2 to 5: Continue the add task conversation ─────────────────────

        /*
         * This method is called for every message while a task is being added.
         * It uses a switch statement to check which step we are on.
         * Each case collects one piece of information from the user.
         * 
         * A switch statement is a cleaner way to write multiple if/else checks
         * when you are comparing the same variable each time.
         */
        private string ContinueAddingTask(string input)
        {
            switch (_currentStep)
            {
                // Step 2: The user just entered the title
                case TaskStep.AwaitingTitle:
                    _pendingTitle = input.Trim();                   // Save the title
                    _currentStep = TaskStep.AwaitingDescription;   // Move to the next step
                    return "Got it — \"" + _pendingTitle + "\". Now give a short description for this task.";

                // Step 3: The user just entered the description
                case TaskStep.AwaitingDescription:
                    _pendingDescription = input.Trim();             // Save the description
                    _currentStep = TaskStep.AwaitingRemindYesNo; // Move to the next step
                    return "Would you like a reminder for this task? (Yes / No)";

                // Step 4: The user answered Yes or No to the reminder question
                case TaskStep.AwaitingRemindYesNo:
                    if (input.ToLower().Contains("yes"))
                    {
                        // User wants a reminder — ask for the details
                        _currentStep = TaskStep.AwaitingReminder;
                        return "When should I remind you? (e.g. \"Remind me in 7 days\" or a date like \"2026-07-01\")";
                    }
                    else
                    {
                        // User does not want a reminder — save the task now with no reminder
                        _storage.AddTask(_pendingTitle, _pendingDescription, "");
                        _currentStep = TaskStep.None;               // Reset the step tracker
                        return "✅ Task added: \"" + _pendingTitle + "\". No reminder set.\n\nType \"view tasks\" to see all your tasks.";
                    }

                // Step 5: The user just entered the reminder details
                case TaskStep.AwaitingReminder:
                    string reminder = input.Trim();                 // Save the reminder text

                    // Now we have everything — save the task to the file
                    _storage.AddTask(_pendingTitle, _pendingDescription, reminder);
                    _currentStep = TaskStep.None;                   // Reset the step tracker
                    return "✅ Task added: \"" + _pendingTitle + "\".\n\nGot it! I'll remind you — " + reminder + ".\n\nType \"view tasks\" to see all your tasks.";

                // Safety fallback — something unexpected happened, so reset
                default:
                    _currentStep = TaskStep.None;
                    return "Something went wrong. Type \"add task\" to try again.";
            }
        }


        // ── View all tasks ────────────────────────────────────────────────────────

        /*
         * Loads all tasks from the file and formats them for display in the chat.
         * Each task shows its ID, title, status, description, reminder and date added.
         */
        private string ViewTasks()
        {
            List<CyberTask> tasks = _storage.LoadTasks();

            // If there are no tasks yet, tell the user
            if (tasks.Count == 0)
            {
                return "You have no tasks yet. Type \"add task\" to create your first one.";
            }

            // Build the message line by line
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📋 Your Cybersecurity Tasks:\n");

            // Loop through each task and display its details
            for (int i = 0; i < tasks.Count; i++)
            {
                CyberTask task = tasks[i];

                // Show a tick for completed tasks, a square for pending ones
                string status = "🔲 Pending";
                if (task.IsComplete == true)
                {
                    status = "✅ Done";
                }

                // If no reminder was set, show "None"
                string reminder = "None";
                if (task.Reminder != "")
                {
                    reminder = task.Reminder;
                }

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


        // ── Complete a task ───────────────────────────────────────────────────────

        /*
         * The user types something like "complete task 2".
         * We extract the number from the message and mark that task as done.
         */
        private string CompleteTask(string input)
        {
            int id = GetNumberFromInput(input);

            if (id == -1)
            {
                return "Please tell me which task number to complete.\nExample: \"complete task 2\"";
            }

            _storage.MarkAsCompleted(id);
            return "✅ Task " + id + " marked as complete. Great work on your cybersecurity!";
        }


        // ── Delete a task ─────────────────────────────────────────────────────────

        /*
         * The user types something like "delete task 3".
         * We extract the number from the message and remove that task from the file.
         */
        private string DeleteTask(string input)
        {
            int id = GetNumberFromInput(input);

            if (id == -1)
            {
                return "Please tell me which task number to delete.\nExample: \"delete task 3\"";
            }

            _storage.DeleteTask(id);
            return "🗑️ Task " + id + " has been deleted.";
        }


        // ── Helper: extract a number from the user's message ─────────────────────

        /*
         * This method splits the user's message into individual words,
         * then checks each word to see if it is a number.
         * Example: "delete task 3" → splits into ["delete", "task", "3"] → returns 3
         * If no number is found, it returns -1 as a signal that the input was invalid.
         */
        private int GetNumberFromInput(string input)
        {
            string[] words = input.Split(' ');

            foreach (string word in words)
            {
                int number;
                bool isNumber = int.TryParse(word, out number);

                if (isNumber)
                {
                    return number;
                }
            }

            return -1; // No number was found
        }
    }
}