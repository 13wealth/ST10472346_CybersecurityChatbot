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
        // The storage helper that handles all file reading and writing
        private TaskStorageHelper _storage;

        /*
         * Constructor — creates the TaskStorageHelper when TaskManager is created.
         */
        public TaskManager()
        {
            _storage = new TaskStorageHelper();
        }

        /*
         * AddTask() — calls storage to save the task and returns a confirmation message.
         */
        public string AddTask(string title, string description, string reminder)
        {
            _storage.AddTask(title, description, reminder);

            // ActivityLogger will be added in Task 4
            return "✅ Task added: \"" + title + "\"";
        }

        /*
         * GetAllTasks() — loads and returns the full list of tasks from the file.
         */
        public List<CyberTask> GetAllTasks()
        {
            return _storage.LoadTasks();
        }

        /*
         * MarkAsComplete() — marks the task with the given ID as done.
         */
        public void MarkAsComplete(int id)
        {
            _storage.MarkAsCompleted(id);

            // ActivityLogger will be added in Task 4
        }

        /*
         * DeleteTask() — removes the task with the given ID.
         */
        public void DeleteTask(int id)
        {
            _storage.DeleteTask(id);

            // ActivityLogger will be added in Task 4
        }
    }
}