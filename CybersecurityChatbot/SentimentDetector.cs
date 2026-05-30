using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated,
        Happy,
        Sad
    }

    /*
     * This class is responsible for analyzing the sentiment of user input and chatbot responses.
     * It can be used to detect negative sentiments that may indicate frustration or dissatisfaction.
     * The implementation can be enhanced with natural language processing techniques.
     */
    class SentimentDetector
    {
        private Dictionary<Sentiment, List<string>> _sentimentKeys;                                             //- A dictionary that maps keywords to their corresponding sentiment categories

        public SentimentDetector()
        {
            _sentimentKeys = new Dictionary<Sentiment, List<string>>()
            {
                {
                    Sentiment.Worried,                                                                          //- Defines a sentiment category for worried feelings and associates it with a list of keywords
                    new List<string>                                                                            //- Creates a list of keywords associated with the defined sentiment
                    {
                        "worried","scared","afraid","anxious","nervous","unsafe","concerned","uneasy",
                        "uneasy","panicking","terrified","threatened","vulnerable","compromised",
                        "breached","hacked","paranoid","suspicious","alarmed"
                    }
                },
                {
                    Sentiment.Curious,
                    new List<string>
                    {
                        "curious","wondering","interested","want to know","how does","what is","tell me about",
                        "explain","learn", "understand", "how do","why does", "what are", "can you show", 
                        "give me an example", "explore"
                    }
                },
                {
                    Sentiment.Frustrated,
                    new List<string>
                    {
                        "frustrated","annoyed","confused","don't understand", "ugh", "useless", "not working", 
                        "still don't get it", "makes no sense","ridiculous", "pointless", "stupid", 
                        "this is hard", "not helpful", "broken"
                    }
                },
                {
                    Sentiment.Happy,
                    new List<string>
                    {
                        "great","thanks","helpful","awesome","love it", "perfect", "excellent", "amazing", 
                        "appreciate", "thank you", "glad","satisfied", "works great", "exactly what I needed", 
                        "well done", "brilliant"
                    }
                },
                {
                    Sentiment.Sad,
                    new List<string>
                    {
                        "sad","depressed", "miserable", "heartbroken", "down", "gloomy", "blue", "melancholy",
                        "sad", "unhappy", "disappointed", "upset", "let down", "discouraged","hopeless", 
                        "feeling down", "not okay", "struggling"
                    }
                }
            };
        }

        public Sentiment Detect(string userInput)
        {
            foreach (var sentiment in _sentimentKeys.Keys)                                                      //- Iterates through each sentiment category in the dictionary, if found it picks the catergory
            {
                foreach (var keyword in _sentimentKeys[sentiment])                                              //- Iterates through the list of keywords associated with the current sentiment category, if found it picks the keyword
                {
                    if (userInput.ToLower().Contains(keyword))                                                  //- Checks if the user input contains any of the keywords associated with the current sentiment category
                    {
                        return sentiment;                                                                       //- If a keyword is found, returns the corresponding sentiment category
                    }
                }
            }
            return Sentiment.Neutral;                                                                           //- If no keywords are found, returns a default sentiment of Neutral
        }

        /*
         * Generates a response based on the detected sentiment. 
         * It uses a switch statement to return an appropriate message for each sentiment category
         */
        public string GetSentimentResponse(Sentiment sentiment)
        {
            switch (sentiment)
            {
                case Sentiment.Worried:
                return "I understand that you're feeling worried. Let's go through this together.";

                case Sentiment.Curious:
                    return "It's great that you're curious! I'll help you understand this better.";

                case Sentiment.Frustrated:
                    return "I hear your frustration. Cybersecurity can be confusing sometimes, but we'll work through it.";

                case Sentiment.Happy:
                    return "That's great to hear! I'm glad you're feeling positive.";

                case Sentiment.Sad:
                    return "I'm sorry you're feeling this way. I'm here to help you with anything you need.";

                default:
                    return "";
            }
        }
    }
}
