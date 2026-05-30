using System;
using System.Collections.Generic;
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
        private enum ChatFlowState
        {
            AskingName,
            AskingTopic,
            Ready
        }

        private ChatBot _chatbot;                                                                               //-Generate responses based on user input and chatbot logic
        private ChatFlowState _chatFlowState = ChatFlowState.AskingName;

        /*
         * The constructor initializes the main window, sets up the UI, and loads any existing chat history.
         * It also plays a welcome sound and displays the bot's greeting and ASCII art logo.
         */
        public MainWindow()
        {
            InitializeComponent();

            UI.WelcomeMessage();                                                                                //-Static method from the console UI class to play the welcome sound
            UI.BotGreeting(message =>
            {
                AddBotMessage(message);
            });                                                                                                 //-Static method from the console UI class to display the bot's greeting in the console

            AsciiArtBlock.Text = Logo.GetAscii();                                                               //-Static method from the console Logo class to display the ASCII logo in the console        

            _chatbot = new ChatBot();                                                                           //-Initialise the chatbot instance to handle user interactions and generate responses
            AddBotMessage("What should I call you?");
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
         * It validates input, displays the user bubble, runs chatbot processing, and then displays the bot bubble.
         */
        private void HandleUserMessage()
        {
            string userMessage = MessageInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            AddUserMessage(userMessage);

            if (_chatFlowState == ChatFlowState.AskingName)
            {
                _chatbot.InitialiseMemory(userMessage, "");
                _chatFlowState = ChatFlowState.AskingTopic;
                AddBotMessage($"Thanks {userMessage}! And what's your favourite cybersecurity topic?");
            }
            else if (_chatFlowState == ChatFlowState.AskingTopic)
            {
                string currentName = _chatbot.GetStoredValue("username");
                _chatbot.InitialiseMemory(currentName, userMessage);
                _chatFlowState = ChatFlowState.Ready;
                //AddBotMessage($"Thanks {currentName}. We can start chatting now.");

                string botReply = _chatbot.ProcessInput(userMessage);
                AddBotMessage(botReply);
            }
            else
            {
                string botReply = _chatbot.ProcessInput(userMessage);
                AddBotMessage(botReply);
            }

            MessageInput.Clear();

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

        private void AddUserMessage(string message)
        {
            ChatMessagesHost.Children.Add(CreateMessageBubble("You", message, true));
            ScrollToLatestMessage();
        }

        private void AddBotMessage(string message)
        {
            ChatMessagesHost.Children.Add(CreateMessageBubble("Bot", message, false));
            ScrollToLatestMessage();
        }

        private FrameworkElement CreateMessageBubble(string author, string message, bool isUser)
        {
            var container = new Grid { Margin = new Thickness(0, 8, 0, 8) };

            var bubble = new Border
            {
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = CreateBrush(isUser ? "#12B886" : "#163045"),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 460
            };

            var content = new StackPanel();
            var authorText = new TextBlock
            {
                Text = author,
                Foreground = CreateBrush(isUser ? "#EFFFFB" : "#6EE7FF"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            var messageText = new TextBlock
            {
                Text = message,
                Foreground = CreateBrush(isUser ? "#071018" : "#FFFFFF"),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 460
            };

            content.Children.Add(authorText);
            content.Children.Add(messageText);
            bubble.Child = content;
            container.Children.Add(bubble);

            return container;
        }

        private static SolidColorBrush CreateBrush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        }
    }
}


