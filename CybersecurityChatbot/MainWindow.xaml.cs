using System.Collections.Generic;
using System.Text;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cybersecurity_Chatbot;                                                                                              //- Import the console namespace to access it properties and methods

namespace CybersecurityChatbot
{
    /*
     * MainWindow.xaml.cs
     * 
     * This file contains the code-behind for the MainWindow of the Cybersecurity Chatbot application.
     * It manages the chat interface, including displaying messages, handling user input, and maintaining chat history.
     * 
     * Key functionalities:
     * - Load existing chat history from a JSON file on startup.
     * - Display chat messages in a scrollable TextBox.
     * - Allow users to send messages via a TextBox and a Send button (or pressing Enter).
     * - Append new messages to the chat history and update the JSON store accordingly.
     * - Show and hide the speech bubble when hovering over the bot image.
     * 
     * Note: The actual chatbot response logic is not implemented in this version; it uses a placeholder response.
     */
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();                                                                                        
            UI.WelcomeMessage();                                                                                          //Static method from the console UI class to play the welcome sound
            ChatHistoryBox.Text = "";                                                                                     //Initialize the chat history box as an empty string (Resets history on each startup
            
            UI.BotGreeting(message =>
            {
                ChatHistoryBox.Text += message + Environment.NewLine;
            });                                                                                                           //Static method from the console UI class to display the bot's greeting in the console

            AsciiArtBlock.Text = Logo.GetAscii();                                                                         //Static method from the console Logo class to display the ASCII logo in the console

            /*_messages = ChatHistoryStore.LoadHistory();                                                                   // 1. Load existing history from JSON store
            RefreshChatBox();*/                                                                                             // 2. Display the loaded history in the UI
        }   


        // Internal list to keep track of messages
        private List<ChatMessage> _messages;
        private void ShowSpeechBubble()
        {
            ChatBubble.Visibility = Visibility.Visible;
            InputPanel.Visibility = Visibility.Visible;
            BubbleColumn.Width = new GridLength(400);
        }

        private void HideSpeechBubble()
        {
            ChatBubble.Visibility = Visibility.Collapsed;
            InputPanel.Visibility = Visibility.Collapsed;
            BubbleColumn.Width = new GridLength(0);
        }

        private void CloseChatButton_Click(object sender, RoutedEventArgs e)
        {
            HideSpeechBubble();
        }

        private void Bot_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowSpeechBubble();
        }
       

        /// <summary>
        /// Handles the Send button click event.
        /// </summary>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Allows the user to press 'Enter' inside the TextBox to send a message.
        /// </summary>
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        /// <summary>
        /// Appends the user message to history, saves it, and adds a placeholder bot response.
        /// </summary>
        private void SendMessage()
        {
            string text = MessageInput.Text.Trim();

            // Don't send empty messages or the placeholder text
            if (string.IsNullOrEmpty(text) || text == "Type message here...")
                return;

            // 1. Create a new message from the User
            var userMessage = new ChatMessage("User", text);
            _messages.Add(userMessage);

            // 2. Append directly to UI
            ChatHistoryBox.AppendText(userMessage.ToString() + "\n");

            // 3. Clear the input box
            MessageInput.Text = "";

            // 4. Temporarily add a placeholder Bot response (until actual logic is wired)
            var botResponse = new ChatMessage("Bot", "[Placeholder] Message received. I will analyze this shortly.");
            _messages.Add(botResponse);
            ChatHistoryBox.AppendText(botResponse.ToString() + "\n\n");

            // Scroll to the bottom of the chat
            ChatScroll.ScrollToBottom();

            // 5. Update the local JSON file with the new history
            ChatHistoryStore.SaveHistory(_messages);
        }

        /// <summary>
        /// Helper method to reload the entire chat box from memory.
        /// </summary>
        private void RefreshChatBox()
        {
            ChatHistoryBox.Clear();
            foreach (var msg in _messages)
            {
                ChatHistoryBox.AppendText(msg.ToString() + "\n");
            }
            ChatHistoryBox.AppendText("\n");
            ChatScroll.ScrollToBottom();
        }
    }
}