using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cybersecurity_Chatbot
{
    public class Logo
    {
        public static void AsciiLogo()
        {
    
    string[] logo =                                                                                             //-Store the logo as an array of strings, One string = one line of the logo
    {
        " ",
        "███╗   ██╗███████╗████████╗     ███████╗ █████╗ ███████╗███████╗",
        "████╗  ██║██╔════╝╚══██╔══╝     ██╔════╝██╔══██╗██╔════╝██╔════╝",
        "██╔██╗ ██║█████╗     ██║        ███████╗███████║█████╗  █████╗  ",
        "██║╚██╗██║██╔══╝     ██║        ╚════██║██╔══██║██╔══╝  ██╔══╝  ",
        "██║ ╚████║███████╗   ██║        ███████║██║  ██║██║     ███████╗",
        "╚═╝  ╚═══╝╚══════╝   ╚═╝        ╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝",
        " ",
        "           Cybersecurity Awareness Assistant - NET SAFE"
     };

            foreach (string line in logo)                                                                       //-Goes through the logo, line by line, animations happen per line
            {
                string output = "";                                                                             //-Starts a line with an empty string

                foreach (char c in line)
                {
                    output += c;                                                                                //-Adds the next character to the output string
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("\r" + output);                                                               //-Once finish writting, go back to the start of the line (Typing animation)
                    Thread.Sleep(1);                                                                            //-Waits for 50 milliseconds before adding the next character, creating an animation effect
                }
                Console.WriteLine();                                                                            //-After finishing one line, move to the next line
            }
            Console.ResetColor(); 
        }
    }
}

