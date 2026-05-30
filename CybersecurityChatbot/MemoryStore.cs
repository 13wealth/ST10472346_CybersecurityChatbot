using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * MemoryStore is a class is used to store the chatbot's memory. 
     * It is used to store the chatbot's knowledge and experiences. 
     * It is used to store the chatbot's interactions with the user. 
     */
    public class MemoryStore
    {
        public string  UserName { get; set; } = "";                                                             //-Define a property to store the user's name, initialized to an empty string
        public string FavouriteTopic { get; set; } = "";                                                        //-Define a property to store the user's favourite topic, initialized to an empty string

        public void Store(string key, string value)
        {
            switch (key.ToLower())
            {
                case "username":
                    UserName = value;
                    break;

                case "favouritetopic":
                    FavouriteTopic = value;
                    break;
            }
        }

        public string Recall(string key)
        {
            switch (key.ToLower())
            {
                case "username":
                    return UserName;

                case "favouritetopic":
                    return FavouriteTopic;

                default:
                    return "";
            }
        }
    }
}