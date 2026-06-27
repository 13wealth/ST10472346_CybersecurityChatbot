using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * This class creates an object that represents a cybersecurity task.
     * This object is serialised to JSON and saved to tasks.json file.
     */
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;                                                          // Tell C# that this property can be null, avoids nullable warnings
        public string Description { get; set; } = string.Empty;
        public string Reminder { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}