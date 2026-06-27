using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * This class handles all the reading and writing of the tasks.json file. 
     * It is responsible for loading the tasks from the file into memory and saving 
     *   the tasks from memory back to the file.
     * All file operations are kept here and nowhere else.
     */
    public class TaskStorageHelper
    {
        private const string FilePath = "tasks.json";                                                           // The path to the tasks.json file. It is a constant because it does not change.

        /*
         * Reads task.json and deserialises it into a List of CyberTask Objects
         * If the file does not exists, returns an empty list.
         */
        public List<CyberTask> LoadTasks()
        {
            try
            {
                if (!File.Exists(FilePath))                                                                     // Check if the file exists before trying to read it
                {
                    return new List<CyberTask>();                                                               // If the file does not exist, return an empty object
                }

                string fileContents = File.ReadAllText(FilePath);                                               // If the file exists, read the contents into a string
                List<CyberTask> json = JsonConvert.DeserializeObject<List<CyberTask>>(fileContents) ?? new List<CyberTask>();

                if (json.Count == 0)
                {
                    return new List<CyberTask>();                                                               // If no tasks were found, return an empty object
                }

                return json;                                                                                    // Else, return the list of tasks
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: " + ex.Message);
                return new List<CyberTask>();                                                                   // If there is an error, return an empty list
            }
        }


        /*
         * Serializes a List of CyberTask objects into a JSON string and writes it to tasks.json.
         * If the file does not exist, it will be created.
         */
        public void SaveTasks(List<CyberTask> tasks)
        {
            try
            {
                string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);                          // Serialize the object into a JSON string and apply indentation for readability
                File.WriteAllText(FilePath, json);                                                              // Write the JSON string to the file
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tasks: " + ex.Message);
            }
        }


        /*
         * Adds a new task to the list of tasks and saves it back to the file.
         * It first loads the current list of tasks, then adds the new task to the list, 
         *  and finally saves the updated list back to the file.
         */
        public void AddTask(string title, string description, string reminder)
        {
            try
            {
                List<CyberTask> tasks = LoadTasks();                                                            // Step 1: Call LoadTasks to get the current list of tasks

                /*
                 * Step 2: Work out what the new ID should be
                 * If there are no tasks in the list, set newID to 1
                 * If there are tasks in the list, set newID to the last task's ID + 1
                 */
                int newId;                                                                                      // Declares a variable to hold the new task ID

                if (tasks.Count == 0)
                {
                    newId = 1;
                }
                else
                {
                    newId = tasks[tasks.Count - 1].Id + 1;                                                      // Adds 1 to the last task's ID to get the new ID
                }

                /*
                 * Step 3: Create a new CyberTask object and set its properties
                 * The new task's ID is set to the newID calculated above
                 * The new task's Title, Description and Reminder are set to the values passed into the method
                 * The new task's IsCompleted property is set to false
                 * The new task's CreatedAt property is set to the current date and time
                 */
                CyberTask newTask = new CyberTask();

                newTask.Id = newId;
                newTask.Title = title;
                newTask.Description = description;
                newTask.Reminder = reminder;
                newTask.IsComplete = false;
                newTask.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                tasks.Add(newTask);                                                                             // Step 4: Add the new task to the list

                SaveTasks(tasks);                                                                               // Step 5: Call SaveTasks to save the updated list back to the file  
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error adding task: " + ex.Message);
            }
        }


        /*
         * Marks a task as completed by setting its IsCompleted property to true.
         * It first loads the current list of tasks, then finds the task with the matching ID,
         *  marks it as completed, and finally saves the updated list back to the file.
         */
        public void MarkAsCompleted(int id)
        {
            try
            {
                List<CyberTask> tasks = LoadTasks();                                                            // Step 1: Call LoadTasks to get the current list of tasks

                /*
                 * Step 2: Loop through the list to find the task with the matching id
                 * Check if the current task's ID matches the provided ID
                 * Step 3: Mark the task as completed
                 */
                foreach (CyberTask task in tasks)
                {
                    if (task.Id == id)
                    {
                        task.IsComplete = true;
                        break;                                                                                  // Exit the loop once the task is found and marked as completed
                    }
                }

                SaveTasks(tasks);                                                                               // Step 4: Call SaveTasks to save the updated list back to the file
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error marking task as completed: " + ex.Message);
            }
        }


        /*
         * Deletes a task from the list by its ID.
         * It first loads the current list of tasks, then finds the task with the matching ID,
         *  removes it from the list, and finally saves the updated list back to the file.
         */
        public void DeleteTask(int id)
        {
            try
            {
                List<CyberTask> tasks = LoadTasks();                                                           // Step 1: Call LoadTasks to get the current list of tasks

                /*
                 * Step 2: Loop through the list to find the task with the matching id
                 * Check if the current task's ID matches the provided ID
                 */
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (tasks[i].Id == id)
                    {
                        tasks.RemoveAt(i);                                                                      // Remove the task from the list
                        break;
                    }
                }

                SaveTasks(tasks);                                                                               //Step 3: Call SaveTasks to save the updated list back to the file
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting task: " + ex.Message);
            }
        }
    }
}