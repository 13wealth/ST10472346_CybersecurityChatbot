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
            AppendBotMessage(_chatbot.GetInitialPrompt());
        }

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            OpenChatPanel();
        }

        private void CloseChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanelBorder.Visibility = Visibility.Collapsed;
            ChatPanelBorder.Opacity = 0;
            ChatSlideTransform.X = 180;
            ChatColumn.Width = new GridLength(0);
        }

        private void OpenChatPanel()
        {
            if (ChatPanelBorder.Visibility == Visibility.Visible)
                return;

            ChatColumn.Width = new GridLength(1.75, GridUnitType.Star);
            ChatPanelBorder.Visibility = Visibility.Visible;

            ChatPanelBorder.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400)));
            ChatSlideTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(180, 0, TimeSpan.FromMilliseconds(400)));
        }


        /*
         * Handles a single user submission from the input box.
         * It validates input, displays the user bubble, runs chatbot processing and then displays the bot bubble.
         * The flow of the conversation is managed based on the current state of the chat flow (asking for name, topic, or ready).
         */
        private void HandleUserMessage()
        {
            string userMessage = MessageInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))                                                         //- Validate that the user has entered a message before processing
                return;

            AppendUserMessage(userMessage);

            string botReply = _chatbot.ProcessInput(userMessage);
            AppendBotMessage(botReply);                                                                         //- Process the user input through the chatbot and display the response

            MessageInput.Clear();                                                                               //- Clear the input box after processing the message
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleUserMessage();
                e.Handled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserMessage();
        }

        private void ScrollToLatestMessage()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ChatScroll.ScrollToEnd();
            }));
        }

        private void AppendUserMessage(string message)
        {
            Messages.Add(new ChatMessage("User", message));
            ScrollToLatestMessage();
        }

        private void AppendBotMessage(string message)
        {
            Messages.Add(new ChatMessage("Bot", message));
            ScrollToLatestMessage();
        }
    }
}


