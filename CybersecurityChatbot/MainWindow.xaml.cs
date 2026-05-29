using System.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cybersecurity_Chatbot;                                                                                    //- Import the console namespace to access it properties and methods

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly KeywordResponder responder;                                                           //-Declare a private variable of type KeywordResponder to handle user input and provide responses based on keywords
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        /*
         * The constructor initializes the main window, sets up the UI, and loads any existing chat history.
         * It also plays a welcome sound and displays the bot's greeting and ASCII art logo.
         */
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UI.WelcomeMessage();                                                                                //-Static method from the console UI class to play the welcome sound

            foreach (var message in ChatHistoryStore.LoadHistory())
            {
                Messages.Add(message);
            }

            Messages.CollectionChanged += (_, __) => ScrollToLatestMessage();

            UI.BotGreeting(message =>
            {
                Messages.Add(new ChatMessage("Bot", message));
            });                                                                                                 //-Static method from the console UI class to display the bot's greeting in the console

            AsciiArtBlock.Text = Logo.GetAscii();                                                               //-Static method from the console Logo class to display the ASCII logo in the console

            responder = new KeywordResponder();                                                                 //-Initialize the responder to handle user input and provide responses based on keywords
        }

        private void HandleUserMessage()
        {
            string userMessage = MessageInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            var userChat = new ChatMessage("User", userMessage);
            Messages.Add(userChat);

            string botReply = responder.GetResponse(userMessage);
            var botChat = new ChatMessage("Bot", botReply);
            Messages.Add(botChat);

            MessageInput.Clear();

            ChatHistoryStore.SaveHistory(Messages.ToList());
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
            ChatScroll.ScrollToEnd();
        }
    }
}


