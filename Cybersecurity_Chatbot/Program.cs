using System;

namespace Cybersecurity_Chatbot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logo.AsciiLogo();                                                                                   //-Display the logo of the chatbot
            UI.WelcomeMessage();                                                                                //-Start welcome sound in the background
            UI.BotGreeting(Console.WriteLine);                                                                  //-Display the bot's greeting
            UI.OnboardingIntro();                                                                               //-Display onboarding intro bubble
            UI.GetUserName();                                                                                   //-Ask for user's name
            UI.GetFavoriteTopic();                                                                              //-Ask for favorite topic and show thank you/menu intro

            while (true)
            {
                UI.Menu();
                Console.Write($"{StateSharing.Name}: ");                                                        //-Prompt the user for input
                string input = Console.ReadLine();

                string choice = InputValidation.InputHandler(input);                                            //-Validate and process the user input to determine the topic

                if (choice == "exit")                                                                           //-Early exit condition
                {
                    ResponseSystem.ExitMessage();
                    break;
                }

                bool valid = ResponseSystem.HandleTopic(choice);                                                //-If input is not "exit" try to treat is as a topic and assign to 'valid'

                if (valid)                                                                                      //-If it was valid and handled as a topic, prompt if they want to continue to another topic
                {
                    bool proceed = ResponseSystem.AnotherTopic();                                               //-Call ContinueTopic() to prompt user if they want to continue, assign results to 'proceed'
                    if (!proceed)                                                                               //-If 'no' or 'n' is entered, exit the chatbot
                    {
                        ResponseSystem.GoodbyeMessage();
                        break;
                    }
                }

                if (!valid)
                    valid = ResponseSystem.HandleGeneral(choice);                                               //-If the input doesn't match a topic, try handling it as a general conversational input  
                if (!valid)
                    ResponseSystem.InvalidOption();                                                             //-Else handle it as an invalid option
            }
        }
    }
}



