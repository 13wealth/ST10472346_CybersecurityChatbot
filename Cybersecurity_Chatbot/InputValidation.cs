using System;

namespace Cybersecurity_Chatbot
{
    internal class InputValidation
    {
        /**     
         * Normalises user input by trimming whitespace, converting to lowercase and mapping menu options to topic names.
         * If the input is empty or whitespace, returns an empty string.
         * If the input matches a menu option, returns the corresponding topic name or "exit".
         * For any other input, returns the normalised string for further processing.
         */
        public static string InputHandler(string input)
        {
            if (string.IsNullOrWhiteSpace(input))                                                               //-If user types nothing, return empty string instead of crashing
            {
                return string.Empty;
            }

            string menu = input.Trim().ToLowerInvariant();                                                      //-Trim whitespace and convert to lowercase for consistent processing

            switch (menu)
            {
                case "1":
                    return "phishing";                                                                          //-Program expects "phishing" so when user types "1", convert it to "phishing"
                case "2":
                    return "passwords";                                                                                 
                case "3":
                    return "suspicious links";                                                                        
                case "4":
                case "quit":
                    return "exit";
                default:
                    return menu;
            }
        }
    }
}
