using System;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Represents a single message in the chat history.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Identifies who sent the message (e.g., "User" or "Bot").
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The time the message was sent.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public ChatMessage()
        {
            // Default constructor required for JSON serialization
        }

        public ChatMessage(string role, string text)
        {
            Role = role;
            Text = text;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Role}: {Text}";
        }
    }
}
