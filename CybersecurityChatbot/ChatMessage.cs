using System;

namespace CybersecurityChatbot
{
    /*
     * This class represents a chat message in the chatbot application.
     * Each message has role, test and timestamp properties.
     * ChatMessage object is created when a user sends a message or when the chatbot responds. 
     */
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatMessage(string role, string text)
        {
            Role = role;
            Text = text;
            Timestamp = DateTime.Now;
        }
    }
}
