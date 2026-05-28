using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles loading and saving the chat history to a JSON file.
    /// </summary>
    public static class ChatHistoryStore
    {
        // Define path in AppData to not litter workspace
        private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CybersecurityChatbot");
        private static readonly string filePath = Path.Combine(directoryPath, "chat-history.json");

        /// <summary>
        /// Saves a list of ChatMessages to the JSON file.
        /// </summary>
        public static void SaveHistory(List<ChatMessage> messages)
        {
            try
            {
                // Ensure the directory exists before saving
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Serialize the message list to JSON and write to file
                string jsonString = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                // Simple logging for 2nd year project
                Console.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of ChatMessages from the JSON file.
        /// </summary>
        public static List<ChatMessage> LoadHistory()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var messages = JsonSerializer.Deserialize<List<ChatMessage>>(jsonString);
                    return messages ?? new List<ChatMessage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading history: {ex.Message}");
            }

            // Return empty list if file doesn't exist or an error occurs
            return new List<ChatMessage>();
        }
    }
}
