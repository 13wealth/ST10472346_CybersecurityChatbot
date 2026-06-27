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

        // The list of answer options — e.g. "A) Reply with your password"
        // For True/False questions this list will be empty
        public List<string> Options { get; set; } = new List<string>();

        // The correct answer — either "A", "B", "C", "D", "True", or "False"
        public string CorrectAnswer { get; set; } = "";

        // Shown to the user after they submit their answer
        public string Explanation { get; set; } = "";

        // If true, the question is a True/False question
        // If false, it is a multiple choice question with A B C D options
        public bool IsTrueFalse { get; set; } = false;
    }
}