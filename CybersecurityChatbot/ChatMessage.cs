using System;

namespace CybersecurityChatbot
{
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatMessage()
        {
        }

        public ChatMessage(string role, string text)
        {
            Role = role;
            Text = text;
            Timestamp = DateTime.Now;
        }
    }
}
