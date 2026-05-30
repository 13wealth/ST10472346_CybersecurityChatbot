using System;
using System.Windows;
using System.Windows.Controls;

namespace CybersecurityChatbot
{
    public class ChatRoleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BotTemplate { get; set; }
        public DataTemplate? UserTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message)
            {
                if (string.Equals(message.Role, "User", StringComparison.OrdinalIgnoreCase))
                {
                    return UserTemplate;
                }

                return BotTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
