using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Media;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading.Tasks;
using Cybersecurity_Chatbot;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private ChatBot _chatbot;
        private TaskManager _taskManager = new TaskManager();
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        /*
         * The constructor initializes the main window, sets up the UI, and loads any existing chat history.
         * It also plays a welcome sound and displays the bot's greeting and ASCII art logo.
         */
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            UI.WelcomeMessage();
            UI.BotGreeting(message =>
            {
                AppendBotMessage(message);
            });

            AsciiArtBlock.Text = Logo.GetAscii();

            _chatbot = new ChatBot();

            // Load any saved tasks from tasks.json into the list when the app opens
            // This satisfies the READ operation — tasks must appear on startup
            LoadTasksIntoList();

            AppendBotMessage(_chatbot.ProcessInput(""));
        }


        /*********** UI EVENT HANDLERS ************/

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleUserMessage();
                e.Handled = true;
            }
        }

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            OpenChatPanel();
        }


        /*********** UI DISPLAY HANDLERS ***********/

        private void AppendUserMessage(string message)
        {
            Messages.Add(new ChatMessage("User", message));
            ScrollToLatestMessage();
        }

        private async void AppendBotMessage(string message)
        {
            // Show typing indicator as a temporary message
            var typing = new ChatMessage("Bot", "__typing__");
            Messages.Add(typing);
            ScrollToLatestMessage();

            // Estimate typing duration based on message length (min 700ms, capped at 3000ms)
            int baseMs = 700;
            int perCharMs = 25;
            int delay = Math.Min(3000, baseMs + (Math.Max(0, message?.Length ?? 0) * perCharMs));

            await Task.Delay(delay);

            // Remove the typing indicator
            var lastTyping = Messages.LastOrDefault(m =>
                string.Equals(m.Role, "Bot", StringComparison.OrdinalIgnoreCase) &&
                m.Text == "__typing__");

            if (lastTyping != null)
            {
                Messages.Remove(lastTyping);
            }

            // Add the real bot message
            Messages.Add(new ChatMessage("Bot", message));
            ScrollToLatestMessage();
        }

        private void ScrollToLatestMessage()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ChatScroll.ScrollToEnd();
            }));
        }


        /*********** UI ANIMATION HANDLERS ***********/

        private void OpenChatPanel()
        {
            if (ChatPanelBorder.Visibility == Visibility.Visible)
                return;

            ChatColumn.Width = new GridLength(1.75, GridUnitType.Star);
            ChatPanelBorder.Visibility = Visibility.Visible;

            ChatPanelBorder.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400)));
            ChatSlideTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(180, 0, TimeSpan.FromMilliseconds(400)));
        }

        private void CloseChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanelBorder.Visibility = Visibility.Collapsed;
            ChatPanelBorder.Opacity = 0;
            ChatSlideTransform.X = 180;
            ChatColumn.Width = new GridLength(0);
        }


        /*********** CONVERSATION HANDLERS ***********/

        /*
         * Handles a single user submission from the input box.
         * Validates input, displays the user bubble, processes through chatbot,
         * then displays the bot response bubble.
         */
        private void HandleUserMessage()
        {
            string userMessage = MessageInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            AppendUserMessage(userMessage);

            string botReply = _chatbot.ProcessInput(userMessage);

            // Check if onboarding just finished
            if (botReply.StartsWith("ONBOARDING_COMPLETE:"))
            {
                // Extract the topic the user typed
                string topic = botReply.Replace("ONBOARDING_COMPLETE:", "");

                // Bubble 1 — topic response
                AppendBotMessage(_chatbot.GetFavouriteTopic(topic));

                // Bubble 2 — menu
                AppendBotMessage(_chatbot.GetMenuPrompt());
            }
            else
            {
                AppendBotMessage(botReply);
            }

            MessageInput.Clear();
        }


        /*********** TASK ASSISTANT GUI HANDLERS ***********/

        /*
         * Reads tasks.json and populates the TaskListBox on screen.
         * Called on startup so saved tasks always appear when the app opens.
         * Also called after every add, complete, or delete to keep the list up to date.
         */
        private void LoadTasksIntoList()
        {
            List<CyberTask> tasks = _taskManager.GetAllTasks();
            TaskListBox.ItemsSource = null;     // Clear first to force a refresh
            TaskListBox.ItemsSource = tasks;    // Bind the updated list to the GUI
        }

        /*
         * Called when the user clicks Add Task.
         * Reads the three input fields, adds the task, then refreshes the list.
         */
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();
            string description = TaskDescriptionInput.Text.Trim();
            string reminder = TaskReminderInput.Text.Trim();

            // Title is required — warn the user if it is empty
            if (string.IsNullOrWhiteSpace(title))
            {
                AppendBotMessage("⚠️ Please enter a task title before adding.");
                return;
            }

            // Add the task and show the confirmation in the chat
            string confirmation = _taskManager.AddTask(title, description, reminder);
            AppendBotMessage(confirmation);

            // Clear the input fields ready for the next task
            TaskTitleInput.Text = "";
            TaskDescriptionInput.Text = "";
            TaskReminderInput.Text = "";

            // Refresh the list so the new task appears immediately
            LoadTasksIntoList();
        }

        /*
         * Called when the user clicks Mark Complete.
         * Gets the selected task from the list and marks it as done in the file.
         */
        private void MarkCompleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Cast the selected item in the ListBox to a CyberTask object
            CyberTask selectedTask = TaskListBox.SelectedItem as CyberTask;

            // Nothing selected — ask the user to pick one first
            if (selectedTask == null)
            {
                AppendBotMessage("⚠️ Please select a task from the list first.");
                return;
            }

            _taskManager.MarkAsComplete(selectedTask.Id);
            AppendBotMessage("✅ \"" + selectedTask.Title + "\" marked as complete!");

            // Refresh the list so the tick icon updates on screen immediately
            LoadTasksIntoList();
        }

        /*
         * Called when the user clicks Delete.
         * Gets the selected task from the list and removes it permanently.
         */
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask selectedTask = TaskListBox.SelectedItem as CyberTask;

            if (selectedTask == null)
            {
                AppendBotMessage("⚠️ Please select a task from the list first.");
                return;
            }

            _taskManager.DeleteTask(selectedTask.Id);
            AppendBotMessage("🗑️ \"" + selectedTask.Title + "\" has been deleted.");

            // Refresh the list so the deleted task disappears immediately
            LoadTasksIntoList();
        }
    }
}