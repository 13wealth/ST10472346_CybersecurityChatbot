using System;
using System.Windows;
using System.Windows.Controls;


namespace CybersecurityChatbot
{
    /*
     * Class selects appropriete template for a chat based on role
     * Example: user bubbles are on the right, bot bubbles are on the left
     */
    public class ChatRoleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BotTemplate { get; set; }                                   // Declares template for bot messages
        public DataTemplate UserTemplate { get; set; }                                  // Declares emplate for user messages

        public ChatRoleTemplateSelector()
        {
            BotTemplate = new DataTemplate();                                           // Initializes the BotTemplate property with a new DataTemplate instance
            UserTemplate = new DataTemplate();                                          // Initializes the UserTemplate property with a new DataTemplate instance
        }

        public override DataTemplate SelectTemplate(
                                                     object item, 
                                                     DependencyObject container
                                                   )
        {
            if (item is ChatMessage message)                                            // Checks if the item is of type ChatMessage
            {
                if (string.Equals(
                                    message.Role, 
                                    "User", 
                                    StringComparison.OrdinalIgnoreCase
                                 ))                                                     // Compares the Role property of the message with "User" (case-insensitive)
                {
                    return UserTemplate;                                                // If the role is "User", return the UserTemplate
                }

                return BotTemplate;                                                     // Else return the BotTemplate for any other role (e.g., "Bot")
            }

            DataTemplate baseTemplate = base.SelectTemplate(item, container);
            if (baseTemplate != null)
            {
                return baseTemplate;
            }

            return new DataTemplate();
        }
    }
}