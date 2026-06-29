using Cybersecurity_Chatbot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private ChatBot _chatbot;
        private TaskManager _taskManager = new TaskManager();
        private QuizManager _quizManager = new QuizManager();
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
            });                                                                         // Display the bot's greetin message in the chat window

            AsciiArtBlock.Text = Logo.GetAscii();                                       // Display the ASCII art logo in the designated TextBlock

            _chatbot = new ChatBot();                                                   // Initialize the chatbot instance

            LoadTasksIntoList();                                                        // Load any existing tasks from the tasks.json file into the list box

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
            {
                await Task.Delay(3000);                                                 // Wait for Welcome message to complete
                AppendBotMessage(_chatbot.ProcessInput(""));
            }));                                                                        // Start the onboarding process after a short delay
        }


        // ── UI Event Handlers ─────────────────────────────────────────────────
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)                // Handles the Enter key press in the message input box
        {
            if (e.Key == Key.Enter)
            {
                HandleUserMessage();
                e.Handled = true;
            }
        }

        private void OpenChatButton(object sender, RoutedEventArgs e)
        {
            OpenChatPanel();
        }


        // ── UI Display Handlers ─────────────────────────────────────────────────
        private void AppendUserMessage(string message)
        {
            Messages.Add(new ChatMessage("User", message));
            ScrollToLatestMessage();
        }

        private async void AppendBotMessage(string message)
        {
            var typing = new ChatMessage("Bot", "__typing__");                          // Show typing indicator as a temporary message
            Messages.Add(typing);
            ScrollToLatestMessage();

            int baseMs = 700;
            int perCharMs = 25;
            int delay = Math.Min(3000, baseMs + (Math.Max(0, message?.Length ?? 0) * perCharMs));

            await Task.Delay(delay);                                                    // Wait for the typing indicator to simulate a realistic response time

            var lastTyping = Messages.LastOrDefault(m =>
                string.Equals(m.Role, "Bot", StringComparison.OrdinalIgnoreCase) &&
                m.Text == "__typing__");

            if (lastTyping != null)                                                     // Remove the typing indicator before adding the actual bot message
            {
                Messages.Remove(lastTyping);
            }

            Messages.Add(new ChatMessage("Bot", message));                              // Add the real bot message
            ScrollToLatestMessage();
        }

        private void ScrollToLatestMessage()                                            // Scrolls the chat window to the latest message after a new message is added
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ChatScroll.ScrollToEnd();
            }));
        }


        // ── UI Animation Handlers ─────────────────────────────────────────────────
        private void OpenChatPanel()                                                // Opens the chat panel with a slide-in animation
        {
            if (ChatPanelBorder.Visibility == Visibility.Visible)
                return;

            ChatColumn.Width = new GridLength(1.75, GridUnitType.Star);
            ChatPanelBorder.Visibility = Visibility.Visible;

            ChatPanelBorder.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400)));
            ChatSlideTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(180, 0, TimeSpan.FromMilliseconds(400)));
        }

        private void CloseChatButton_Click(object sender, RoutedEventArgs e)        // Closes the chat panel with a slide-out animation
        {
            ChatPanelBorder.Visibility = Visibility.Collapsed;
            ChatPanelBorder.Opacity = 0;
            ChatSlideTransform.X = 180;
            ChatColumn.Width = new GridLength(0);
        }


        // ── UI Conversation Handlers ─────────────────────────────────────────────────
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

            if (botReply.StartsWith("ONBOARDING_COMPLETE:"))                          // Check if onboarding just finished
            {
                
                string topic = botReply.Replace("ONBOARDING_COMPLETE:", "");          // Extract the topic from the bot's reply
                string favTopicMessage = _chatbot.GetFavouriteTopic(topic);           // Bubble 1 — topic response (Thank you {name}... + explanation)
                AppendBotMessage(favTopicMessage);

                int baseMs = 700;
                int perCharMs = 25;
                int delay = Math.Min(3000, baseMs + (Math.Max(0, favTopicMessage?.Length ?? 0) * perCharMs));

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
                {
                    await Task.Delay(delay);
                    AppendBotMessage(_chatbot.GetMenuPrompt());
                }));                                                                    // Bubble 2 — menu prompt (What would you like to do next? + options)
            }
            
            else if (botReply.StartsWith("TASK_ADDED:"))                                // Check if a task was just added
            {
                string message = botReply.Replace("TASK_ADDED:", "");
                AppendBotMessage(message);
                LoadTasksIntoList();
            }
            else
            {
                AppendBotMessage(botReply);                                             // Regular bot response
            }

            MessageInput.Clear();
        }


        // ── UI Task Assistant Handlers ─────────────────────────────────────────────────
        /*
         * Reads tasks.json and populates the TaskListBox on screen.
         * Called on startup so saved tasks always appear when the app opens.
         * Also called after every add, complete, or delete to keep the list up to date.
         */
        private void LoadTasksIntoList()
        {
            List<CyberTask> tasks = _taskManager.GetAllTasks();
            TaskListBox.ItemsSource = null;                                             // Clear first to force a refresh
            TaskListBox.ItemsSource = tasks;                                            // Bind the updated list to the GUI
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

            if (string.IsNullOrWhiteSpace(title))                                       // Validate that the title is not empty
            {
                AppendBotMessage("Please enter a task title before adding.");
                return;
            }

            string confirmation = _taskManager.AddTask(
                                                        title, 
                                                        description, 
                                                        reminder
            );                                                                          // Adds the task and get a confirmation message 
            AppendBotMessage(confirmation);

            TaskTitleInput.Text = "";                                                   // Clear the input fields ready for the next task
            TaskDescriptionInput.Text = "";                                             // Clear the input fields ready for the next task 
            TaskReminderInput.Text = "";                                                // Clear the input fields ready for the next task

            LoadTasksIntoList();                                                        // Refresh the list so the new task appears immediately
        }

        /*
         * Called when the user clicks Mark Complete.
         * Gets the selected task from the list and marks it as done in the file.
         */
        private void MarkCompleteButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask? selectedTask = TaskListBox.SelectedItem as CyberTask;            // Get the currently selected task from the list box

            if (selectedTask == null)                                                   // Check if no task is selected
            {
                AppendBotMessage("Please select a task from the list first.");          // If not, show a warning message
                return;
            }

            _taskManager.MarkAsComplete(selectedTask.Id); // Mark the selected task as complete in the tasks.json file
            AppendBotMessage($"{selectedTask.Title} marked as complete!"); // Show a confirmation message to the user

            LoadTasksIntoList();                                                        // Refresh the list so the updated task status appears immediately
        }

        /*
         * Called when the user clicks Delete.
         * Gets the selected task from the list and removes it permanently.
         */
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask? selectedTask = TaskListBox.SelectedItem as CyberTask;

            if (selectedTask == null)
            {
                AppendBotMessage("Please select a task from the list first.");
                return;
            }

            _taskManager.DeleteTask(selectedTask.Id);
            AppendBotMessage($"{selectedTask.Title} has been deleted.");

            
            LoadTasksIntoList();                                                        // Refresh the list so the deleted task disappears immediately
        }

        // ── UI Quiz Handlers ─────────────────────────────────────────────────
        /*
         * Opens the quiz panel with a slide-in animation.
         * Resets the quiz and loads the first question.
         */
        private void OpenQuizButton_Click(object sender, RoutedEventArgs e)
        {
            _quizManager.ResetQuiz();                                                   // Reset so the quiz always starts fresh

            QuizColumn.Width = new GridLength(1.75, GridUnitType.Star);                 // Show the panel with the same animation as the chat panel
            QuizPanelBorder.Visibility = Visibility.Visible;
            QuizPanelBorder.BeginAnimation(
                                            OpacityProperty,
                                            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
             );
            QuizSlideTransform.BeginAnimation(
                                                TranslateTransform.XProperty,
                                                new DoubleAnimation(180, 0, TimeSpan.FromMilliseconds(400))
             );

            LoadCurrentQuestion();                                                      // Loads the first question from QuizManager
        }

        /*
         * Closes the quiz panel.
         */
        private void CloseQuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizPanelBorder.Visibility = Visibility.Collapsed;
            QuizPanelBorder.Opacity = 0;
            QuizSlideTransform.X = 180;
            QuizColumn.Width = new GridLength(0);
        }

        /*
         * Loads the current question from QuizManager and displays it on screen.
         * Switches between multiple choice and true/false layouts automatically.
         */
        private void LoadCurrentQuestion()
        {
            QuizQuestion question = _quizManager.GetCurrentQuestion();                  // Get the current question from QuizManager
            QuizProgressText.Text = _quizManager.GetQuestionProgress();                 // Updates progress and score display
            QuizScoreText.Text = _quizManager.GetCurrentScore();                        // Show the question text
            QuestionText.Text = question.Question;                                      // Display the question text in the designated TextBlock

            /*
             * Reset the UI for the new question:
             * - Hide feedback and next button
             * - Show submit button
             * - Clear all radio buttons to ensure no previous selection is carried over
             */
            FeedbackBorder.Visibility = Visibility.Collapsed; 
            NextQuestionButton.Visibility = Visibility.Collapsed;
            SubmitAnswerButton.Visibility = Visibility.Visible;
            OptionA.IsChecked = false;                                                  // Clear all radio buttons to ensure no previous selection is carried over
            OptionB.IsChecked = false;
            OptionC.IsChecked = false;
            OptionD.IsChecked = false;
            OptionTrue.IsChecked = false;                                               // Clear True/False selection
            OptionFalse.IsChecked = false;

            if (question.IsTrueFalse)
            {
                MultipleChoicePanel.Visibility = Visibility.Collapsed;                  // Show True/False buttons, hide multiple choice
                TrueFalsePanel.Visibility = Visibility.Visible;
            }
            else
            {
                MultipleChoicePanel.Visibility = Visibility.Visible;                    // Show multiple choice buttons, hide True/False
                TrueFalsePanel.Visibility = Visibility.Collapsed;
                OptionA.Content = question.Options[0];                                  // Set the text for each option from the question's options list
                OptionB.Content = question.Options[1];
                OptionC.Content = question.Options[2];
                OptionD.Content = question.Options[3];
            }
        }

        /*
         * Called when the user clicks Submit Answer.
         * Works out which option is selected, submits it to QuizManager,
         * and displays the feedback.
         */
        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            QuizQuestion question = _quizManager.GetCurrentQuestion();
            string selectedAnswer = "";

            if (question.IsTrueFalse)                                                   // Work out which radio button the user selected
            {
                if (OptionTrue.IsChecked == true) selectedAnswer = "True";
                if (OptionFalse.IsChecked == true) selectedAnswer = "False";
            }
            else
            {
                if (OptionA.IsChecked == true) selectedAnswer = "A";
                if (OptionB.IsChecked == true) selectedAnswer = "B";
                if (OptionC.IsChecked == true) selectedAnswer = "C";
                if (OptionD.IsChecked == true) selectedAnswer = "D";
            }

            if (string.IsNullOrEmpty(selectedAnswer))                                   // Make sure the user actually selected something
            {
                FeedbackBorder.Visibility = Visibility.Visible;
                FeedbackText.Text = "⚠️ Please select an answer before submitting.";
                return;
            }

            bool isCorrect = _quizManager.SubmitAnswer(selectedAnswer);                 // Submit the answer to QuizManager and get back true or false

            FeedbackBorder.Visibility = Visibility.Visible;                             // Show the feedback panel
            FeedbackText.Text = _quizManager.GetFeedback(isCorrect);
            QuizScoreText.Text = _quizManager.GetCurrentScore();                        // Update the score display    
            SubmitAnswerButton.Visibility = Visibility.Collapsed;                       // Hide Submit, show Next (or results if finished)

            if (_quizManager.IsFinished())
            {
                NextQuestionButton.Content = "See Results ➡";                           // Show the results screen after a short delay via Next button
                NextQuestionButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextQuestionButton.Visibility = Visibility.Visible;
            }
        }

        /*
         * Called when the user clicks Next Question.
         * Either loads the next question or shows the results screen.
         */
        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_quizManager.IsFinished())
            {
                QuestionPanel.Visibility = Visibility.Collapsed;                        // Hide the question panel
                ResultsPanel.Visibility = Visibility.Visible;                           // Show the results panel
                FeedbackBorder.Visibility = Visibility.Collapsed;                       // Hide feedback panel
                NextQuestionButton.Visibility = Visibility.Collapsed;                   // Hide Next button
                SubmitAnswerButton.Visibility = Visibility.Collapsed;                   // Hide Submit button
                PlayAgainButton.Visibility = Visibility.Visible;                        // Show Play Again button
                FinalScoreText.Text = _quizManager.GetFinalScore();                     // Display the final score
                FinalMessageText.Text = _quizManager.GetFinalMessage();                 // Display the final message based on performance
                QuizProgressText.Text = "Quiz Complete!";                               // Update progress text to indicate completion
            }
            else
            {
                LoadCurrentQuestion();                                                  // Load the next question
            }
        }

        /*
         * Called when the user clicks Play Again.
         * Resets the quiz and starts from question 1.
         */
        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            _quizManager.ResetQuiz();

            ResultsPanel.Visibility = Visibility.Collapsed;
            QuestionPanel.Visibility = Visibility.Visible;
            PlayAgainButton.Visibility = Visibility.Collapsed;

            LoadCurrentQuestion();
        }
    }
}