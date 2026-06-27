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
using Cybersecurity_Chatbot;                                                                                    //- Import the console namespace to access it properties and methods

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        private ChatBot _chatbot;                                                                               //-Generate responses based on user input and chatbot logic

        /*
         * The constructor initializes the main window, sets up the UI, and loads any existing chat history.
         * It also plays a welcome sound and displays the bot's greeting and ASCII art logo.
         */
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            UI.WelcomeMessage();                                                                                //-Static method from the console UI class to play the welcome sound
            UI.BotGreeting(message =>
            {
                AppendBotMessage(message);
            });                                                                                                 //-Static method from the console UI class to display the bot's greeting in the console

            AsciiArtBlock.Text = Logo.GetAscii();                                                               //-Static method from the console Logo class to display the ASCII logo in the console        

            _chatbot = new ChatBot();                                                                           //-Initialise the chatbot instance to handle user interactions and generate responses
            AppendBotMessage(_chatbot.ProcessInput(""));                                                        //- Start the conversation by processing an empty input to trigger the initial prompt from the chatbot
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

            // Estimate typing duration based on message length (min 700ms, capped)
            int baseMs = 700;
            int perCharMs = 25;
            int delay = Math.Min(3000, baseMs + (Math.Max(0, message?.Length ?? 0) * perCharMs));

            await Task.Delay(delay);

            // Remove the typing indicator (the most recent typing entry)
            var lastTyping = Messages.LastOrDefault(m => string.Equals(m.Role, "Bot", StringComparison.OrdinalIgnoreCase) && m.Text == "__typing__");
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


        //───CONVERSATION HANDLERS──────────────────────────────────────────────────────────────────────────────────

        /*
         * Handles a single user submission from the input box.
         * It validates input, displays the user bubble, runs chatbot processing and then displays the bot bubble.
         * The flow of the conversation is managed based on the current state of the chat flow (asking for name, topic, or ready).
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
    }
}