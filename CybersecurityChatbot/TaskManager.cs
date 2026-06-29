using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /*
     * TaskManager sits between the GUI and TaskStorageHelper.
     * It contains the business logic for managing tasks.
     * The GUI calls TaskManager, and TaskManager calls TaskStorageHelper.
     */
    public class TaskManager
    {
        private TaskStorageHelper _storage;                                             // The storage helper that handles all file reading and writing

        public TaskManager()                                                            // Constructor — creates the TaskStorageHelper when TaskManager is created.
        {
            _storage = new TaskStorageHelper();
        }

        public string AddTask(string title, string description, string reminder)        // Calls storage to save the task and returns a confirmation message.
        {
            _storage.AddTask(title, description, reminder);
            return "Task added: \"" + title + "\"";                                  // ActivityLogger will be added in Task 4
        }

        public List<CyberTask> GetAllTasks()                                            // Loads and returns the full list of tasks from the file.
        {
            return _storage.LoadTasks();
        }

        public void MarkAsComplete(int id)                                              // Marks the task with the given ID as done.
        {
            _storage.MarkAsCompleted(id);
        }

        public void DeleteTask(int id)                                                  // Removes the task with the given ID.
        {
            _storage.DeleteTask(id);
        }
    }
}