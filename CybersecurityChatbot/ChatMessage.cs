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

        // Accept nullable text and guard against null to satisfy nullable reference checks.
        public ChatMessage(string role, string? text)
        {
            Role = role;
            Text = text ?? string.Empty;
            Timestamp = DateTime.Now;
        }
    }
}
