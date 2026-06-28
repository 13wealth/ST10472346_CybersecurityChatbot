using System;
using System.IO;
using System.Media;

namespace Cybersecurity_Chatbot
{
    /**
     * UI class handles all user interactions, including displaying messages, menus, and prompts.
     * It also manages the welcome sound and validates user input for continuing the conversation.
     * Changed the class from internal to public to allow access from the WPF application
     */
    public class UI
    {
        private static SoundPlayer welcomePlayer;

        public static void BotGreeting(Action<string> output)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            string greeting =
                                "\n\n" +
                                "******************************\n" +
                                "   W   E   L   C   O   M   E  \n" +
                                "******************************\n\n" +
                                "Hello human... I am a Cybersecurity Awareness Bot, here to help you stay safe online.\n";

            Console.ResetColor();
            output(greeting);
        }

        public static void GetUserName()
        {
            TypeText("Bot: What should I call you? \nYou: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "User";
            }
            StateSharing.Name = name;
            Console.WriteLine();
            TypeText($"Bot: Nice to meet you, {name}!\n");
            Console.WriteLine();
        }

        public static void OnboardingIntro()
        {
            TypeText("Bot: Let me get to know you with two quick questions.\n");
            Console.WriteLine();
        }

        public static void GetFavoriteTopic()
        {
            TypeText("Bot: What is your favorite cybersecurity topic?\nYou: ");
            string topic = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = "cybersecurity";
            }
            StateSharing.FavoriteTopic = topic;
            Console.WriteLine();
            TypeText($"Bot: Thank you {StateSharing.Name}, as someone interested in {topic}, here's what I can help you with...\n");
            Console.WriteLine();
        }

        public static void WelcomeMessage()
        {
            if (!OperatingSystem.IsWindows())                                                                   //-Checks if the operating system is Windows, as SoundPlayer is only supported on Windows.
            {
                return;                                                                                         //-If it is not, skip the sound playback to avoid errors.
            }

            try
            {
                string soundFile = File.Exists("welcome.wav") ? "welcome.wav" : "assets\\welcome.wav";
                welcomePlayer = new SoundPlayer(soundFile);                                                     //-Attempts to play the welcome sound when the chatbot starts
                welcomePlayer.Play();                                                                           //-Plays the welcome sound asynchronously in the background
            }
            catch (Exception a)
            {
                Console.WriteLine("Bot: Unable to play welcome sound," + a.Message);                            //-If the sound file is missing or cannot be played, it will catch the exception and display an error message
            }
        }

        public static void Menu()
        {
            Console.WriteLine();                                                                                //-Adds a blank line for spacing
            string border = new string('*', 64);                                                                //-Creates a string of 64 asterisks to use as a border for the menu display

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(border);
            Console.WriteLine("        C   H   A   T   B   O   T  -  M   E   N   U     ");
            Console.WriteLine(border);

            Console.WriteLine();                                                                                //-Adds a blank line for spacing

            //-This will highlight only this line
            Console.ForegroundColor = ConsoleColor.Green;
            TypeText("Choose a topic:\n");

            //-Return the section colour for the rest of the items in the border 
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1) Phishing emails");
            Console.WriteLine("2) Passwords");
            Console.WriteLine("3) Suspicious links");
            Console.WriteLine("4) Exit");

            Console.WriteLine(border);
            Console.ResetColor();                                                                               //-Resets the console color to default after displaying the menu
        }

        public static void TypeText(string text, int delay = 40)
        {
            foreach (char a in text)                                                                            //-Iterates through each character in the input text
            {
                Console.Write(a);                                                                               //-Writes the current character to the console without a newline
                System.Threading.Thread.Sleep(delay);                                                           //-Delay between each character to create typing effect
            }
        }
    }
}