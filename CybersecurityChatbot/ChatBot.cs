using System;
using System.Collections.Generic;
using System.Text;


namespace CybersecurityChatbot
{
    /*
     * This class hold the core logic and functionality of the chatbot.
     * It handles user interactions, process input and generate responses.
     */
    public class ChatBot
    {
        private KeywordResponder _keywords;
        private SentimentDetector _sentiment;
        private MemoryStore _memory;

        public ChatBot()
        {
            _keywords = new KeywordResponder();                                                                 //-Initialize the responder to handle user input and provide responses based on keywords
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();

        }
        public string ProcessInput(string userInput)
        {
            Sentiment sentiment = _sentiment.Detect(userInput);                                                 //-Detect the sentiment of the user input using Detector() and store it in sentiment
            String sentimentResponse = _sentiment.GetSentimentResponse(sentiment);                              //-Generate a response based on the detected sentiment
            String keywordsResponse = _keywords.GetResponse(userInput);                                         //-Get a response based on the detected keywords in the user input
            
            return  sentimentResponse + "\n" + keywordsResponse;                                                 //-Generate a response based on the detected sentiment
        }
    }
}
