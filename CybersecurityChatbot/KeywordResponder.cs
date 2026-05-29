using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    /*
     * This class reads user input and responds with predefined answers based on keywords.
     * It uses a dictionary to map keywords to responses.
     * It also has a method to check if a keyword is present in the user input and return the corresponding response.
     */
    class KeywordResponder
    {
        /*
         * Declaration of the dictionary that will hold keywords and responses
         * Encapsulation: Only KeywordResponder can access variable name _responses
         * KeyType: Is a string representing the keyword to look for in user input (string)
         * ValueType: Is a list of strings representing possible responses for that keyword (List<string>)                                                  
         */
        private Dictionary<string, List<string>> _responses; 
        
        /*
         * Random (built-in C#) method that randomly selects a response from the list using a keyword
         * Ensures the key does not return sane response all the time
         * Creates a new object and stores the reponse in _random
         */
        private Random _random = new Random(); 

        public KeywordResponder()
        {
            _responses = new Dictionary<string, List<string>>()                                                 //-Creates an object for the dictionary and stores the keywords and responses in it
            {
                {
                    "password",
                    new List<string>()
                    {
                        "Creating a strong password means mixing uppercase and lowercase letters, numbers and special characters so it's much harder for anyone to guess or crack it.",
                        "Using the same password across multiple accounts is risky because if one account gets compromised, all your other accounts become vulnerable too.",
                        "A password manager is a really handy tool that securely stores all your passwords in one place, so you only need to remember one master password."
                    }
                },

                {
                    "phishing",
                    new List<string>()
                    {
                        "Phishing emails are designed to look like they're coming from a bank or a well-known company, but their real goal is to steal your personal information.",
                        "If you receive a message with a link that seems even slightly suspicious, it's always safer to avoid clicking it and go directly to the official website instead.",
                        "Before you share any personal or financial information, take a moment to double-check who the sender actually is, because scammers are very good at faking identities."
                    }
                },

                {
                    "privacy",
                    new List<string>()
                    {
                        "It's a good habit to check your privacy settings on social media and apps every few months, since platforms often update their policies without letting you know.",
                        "Posting sensitive details like your home address, phone number or daily routine publicly online can put you at serious risk, so it's best to keep that information private.",
                        "Some apps request access to your contacts, location or camera even when they don't really need it, so always think twice before granting those permissions."
                    }
                },

                {
                    "scam",
                    new List<string>()
                    {
                        "A lot of online scams work by creating a sense of urgency, like telling you that your account will be closed or that you've won a prize, just to pressure you into acting without thinking.",
                        "You should never send money to someone you haven't verified in person, especially if the request came out of nowhere through a message or email.",
                        "If an offer online looks unbelievably good, whether it's a job, a deal or a prize, take a step back and research it carefully before you engage with it."
                    }
                },

                {
                    "malware",
                    new List<string>()
                    {
                        "Having a reliable antivirus program installed on your device gives you an important layer of protection against malicious software that can steal your data or damage your system.",
                        "Downloading files from websites you don't fully trust is one of the most common ways people accidentally install malware on their devices.",
                        "Keeping your operating system and apps up to date is really important because those updates often include security patches that fix vulnerabilities hackers love to exploit."
                    }
                },

                {
                    "vpn",
                    new List<string>()
                    {
                        "A VPN or Virtual Private Network, encrypts your internet connection so that anyone trying to intercept your traffic, like on a public Wi-Fi network, can't read what you're sending or receiving.",
                        "Using a VPN when you're connected to a coffee shop or airport Wi-Fi is a smart move because those networks are often unsecured and easy targets for hackers.",
                        "While a VPN does a great job of protecting your privacy online, it doesn't make you completely anonymous, so you should still practice safe browsing habits alongside using one."
                    }
                },

                {
                    "twofactor",
                    new List<string>()
                    {
                        "Two-factor authentication, often called 2FA, adds a second layer of security to your accounts by requiring not just your password but also a code sent to your phone or email.",
                        "Even if someone manages to get hold of your password, two-factor authentication makes it much harder for them to actually log into your account without also having access to your device.",
                        "You can set up two-factor authentication on most major platforms like Gmail, Facebook and your bank and it only takes a few minutes but makes a huge difference to your security."
                    }
                },

                {
                    "ransomware",
                    new List<string>()
                    {
                        "Ransomware is a type of malicious software that locks you out of your own files and then demands a payment, usually in cryptocurrency, before the attacker will give you access back.",
                        "Regularly backing up your important files to an external drive or a secure cloud service means that even if ransomware hits, you won't lose everything and won't need to pay the ransom.",
                        "Ransomware often sneaks onto your device through email attachments or fake software downloads, so being cautious about what you open or install is one of the best ways to stay protected."
                    }
                },

                {
                    "firewall",
                    new List<string>()
                    {
                        "A firewall acts like a security guard for your device or network, monitoring incoming and outgoing traffic and blocking anything that looks suspicious or unauthorised.",
                        "Most operating systems come with a built-in firewall and it's really important to make sure it's always turned on because turning it off leaves your device much more exposed to attacks.",
                        "Businesses often use more advanced firewalls to protect their entire network, but even at home, having a basic firewall enabled on your router and computer goes a long way in keeping threats out."
                    }
                }
            };
        }

        /*
         * Gets the keyword from the input and searches/iterates the dictionery for a response.
         * If a keyword is found, Random() selects one of the responses associated with that keyword and returns it.
         * If no keywords are found, it returns a default message.
         */
        public string GetResponse(string userInput)
        {
            foreach (var keyword in _responses.Keys)
            {
                if (userInput.ToLower().Contains(keyword))
                {
                    var keyWordFound = _responses[keyword];
                    int randomReply = _random.Next(keyWordFound.Count);
                    return keyWordFound[randomReply];
                }
            }
            return "Sorry, I don't have information on that topic. Please try asking about something else.";    //-else statement if no keywords are found in the user input
        }
    }
}
