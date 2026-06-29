using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /*
     * This class represents a single quiz question.
     * It holds the question text, the answer options, the correct answer,
     * an explanation shown after answering, and a flag for true/false questions.
     */
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = "";
        public string Explanation { get; set; } = "";
        public bool IsTrueFalse { get; set; } = false;
    }
}