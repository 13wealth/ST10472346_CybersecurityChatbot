using System;
using System.Collections.Generic;
using System.Text;


namespace CybersecurityChatbot
{
    /*
     * QuizManager stores all the quiz questions and tracks the quiz state.
     * It knows which question we are on, what the score is,
     * and whether the quiz has finished.
     *
     * The GUI calls these methods one at a time as the user moves through the quiz.
     */
    public class QuizManager
    { 
        private List<QuizQuestion> _questions;                                          // The full list of quiz questions
        private int _currentIndex = 0;                                                  // Tracks which question we are currently on (starts at 0)
        private int _score = 0;                                                         // Tracks how many correct answers the user has given

        /*
         * The constructor creates all the quiz questions and stores them in the list.
         * Questions cover: phishing, password safety, safe browsing,
         * social engineering, two-factor authentication, malware, privacy, backup.
         */
        public QuizManager()
        {
            _questions = new List<QuizQuestion>();


            // ── Phishing (2 questions) ────────────────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new List<string>
                {
                    "A) Reply with your password",
                    "B) Delete the email",
                    "C) Report the email as phishing",
                    "D) Ignore it"
                },
                CorrectAnswer = "C",
                Explanation = "Reporting phishing emails helps protect others and prevents scams from spreading.",
                IsTrueFalse = false
            });

            _questions.Add(new QuizQuestion
            {
                Question = "Phishing emails always contain spelling mistakes and are easy to identify.",
                Options = new List<string>(),
                CorrectAnswer = "False",
                Explanation = "Modern phishing emails can look very professional and convincing. Always verify the sender.",
                IsTrueFalse = true
            });


            // ── Password Safety (2 questions) ─────────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following is the strongest password?",
                Options = new List<string>
                {
                    "A) password123",
                    "B) John1990",
                    "C) qwerty",
                    "D) T#9mK!vL2@pX"
                },
                CorrectAnswer = "D",
                Explanation = "A strong password uses a mix of uppercase, lowercase, numbers and special characters with no personal information.",
                IsTrueFalse = false
            });

            _questions.Add(new QuizQuestion
            {
                Question = "It is safe to use the same password for all your accounts.",
                Options = new List<string>(),
                CorrectAnswer = "False",
                Explanation = "If one account is compromised, all your other accounts become vulnerable. Use a unique password for each account.",
                IsTrueFalse = true
            });


            // ── Safe Browsing (2 questions) ───────────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "What does HTTPS in a website URL indicate?",
                Options = new List<string>
                {
                    "A) The website is free to use",
                    "B) The website is encrypted and more secure",
                    "C) The website is government approved",
                    "D) The website loads faster"
                },
                CorrectAnswer = "B",
                Explanation = "HTTPS means the connection between your browser and the website is encrypted, protecting your data in transit.",
                IsTrueFalse = false
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What should you avoid doing on public Wi-Fi?",
                Options = new List<string>
                {
                    "A) Browsing news websites",
                    "B) Accessing your bank account",
                    "C) Watching videos",
                    "D) Searching on Google"
                },
                CorrectAnswer = "B",
                Explanation = "Public Wi-Fi is often unencrypted. Avoid accessing sensitive accounts like banking unless you use a VPN.",
                IsTrueFalse = false
            });


            // ── Social Engineering (2 questions) ──────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "Social engineering attacks rely on tricking people rather than hacking systems.",
                Options = new List<string>(),
                CorrectAnswer = "True",
                Explanation = "Social engineering exploits human psychology — trust, fear, or urgency — to get people to reveal information.",
                IsTrueFalse = true
            });

            _questions.Add(new QuizQuestion
            {
                Question = "Which scenario is an example of social engineering?",
                Options = new List<string>
                {
                    "A) A hacker guessing your password",
                    "B) A virus infecting your computer",
                    "C) Someone calling pretending to be IT support to get your login details",
                    "D) An app crashing due to a bug"
                },
                CorrectAnswer = "C",
                Explanation = "Pretexting — pretending to be someone trustworthy — is a classic social engineering technique.",
                IsTrueFalse = false
            });


            // ── Two-Factor Authentication (1 question) ────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "What is the purpose of two-factor authentication (2FA)?",
                Options = new List<string>
                {
                    "A) To make logging in faster",
                    "B) To add an extra layer of security beyond just a password",
                    "C) To replace your password entirely",
                    "D) To allow multiple users to share one account"
                },
                CorrectAnswer = "B",
                Explanation = "2FA requires a second verification step — like a code sent to your phone — making it much harder for attackers to access your account.",
                IsTrueFalse = false
            });


            // ── Malware and Ransomware (2 questions) ──────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "Ransomware encrypts your files and demands payment to restore access.",
                Options = new List<string>(),
                CorrectAnswer = "True",
                Explanation = "Ransomware locks your files and asks for a ransom, usually in cryptocurrency. Backups are the best defence.",
                IsTrueFalse = true
            });

            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following can help protect your device from malware?",
                Options = new List<string>
                {
                    "A) Disabling your firewall",
                    "B) Downloading software from any website",
                    "C) Keeping your operating system and antivirus updated",
                    "D) Using the same password everywhere"
                },
                CorrectAnswer = "C",
                Explanation = "Keeping software updated patches known vulnerabilities that malware exploits to infect your device.",
                IsTrueFalse = false
            });


            // ── Privacy Settings (1 question) ─────────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "Why is it important to review your social media privacy settings?",
                Options = new List<string>
                {
                    "A) To get more followers",
                    "B) To limit who can see your personal information",
                    "C) To improve your internet speed",
                    "D) To block advertisements"
                },
                CorrectAnswer = "B",
                Explanation = "Limiting who can see your posts and personal details reduces the risk of identity theft and targeted attacks.",
                IsTrueFalse = false
            });


            // ── Data Backup (1 question) ──────────────────────────────────────────

            _questions.Add(new QuizQuestion
            {
                Question = "What is the recommended backup strategy known as the 3-2-1 rule?",
                Options = new List<string>
                {
                    "A) 3 passwords, 2 devices, 1 account",
                    "B) 3 copies of data, on 2 different media, with 1 stored offsite",
                    "C) Back up 3 times a day, 2 days a week, 1 month a year",
                    "D) 3 cloud services, 2 hard drives, 1 USB"
                },
                CorrectAnswer = "B",
                Explanation = "The 3-2-1 rule ensures you always have a safe copy of your data even if one storage method fails or is attacked.",
                IsTrueFalse = false
            });
        }


        // ── Quiz methods ──────────────────────────────────────────────────────────

        /*
         * Returns the question the user is currently on.
         * The GUI calls this to display the question text and options.
         */
        public QuizQuestion GetCurrentQuestion()
        {
            return _questions[_currentIndex];
        }

        /*
         * Checks the user's answer against the correct answer.
         * Increments the score if correct and moves to the next question.
         * Returns true if the answer was correct, false if not.
         */
        public bool SubmitAnswer(string answer)
        {
            bool isCorrect = answer.Trim().ToUpper() == _questions[_currentIndex].CorrectAnswer.ToUpper();

            if (isCorrect)
            {
                _score++;
            }

            _currentIndex++;

            return isCorrect;
        }

        /*
         * Returns the explanation for the current question.
         * Called by the GUI after the user submits an answer.
         * The correct parameter tells us whether to prefix with Correct or Incorrect.
         */
        public string GetFeedback(bool correct)
        {
            // _currentIndex has already moved forward in SubmitAnswer
            // so we look at the previous question for the explanation
            string explanation = _questions[_currentIndex - 1].Explanation;

            if (correct)
            {
                return "✅ Correct!\n\n" + explanation;
            }
            else
            {
                string correctAnswer = _questions[_currentIndex - 1].CorrectAnswer;
                return "❌ Incorrect. The correct answer was: " + correctAnswer + "\n\n" + explanation;
            }
        }

        /*
         * Returns true when the user has answered all questions.
         * The GUI uses this to decide whether to show Next Question or the results screen.
         */
        public bool IsFinished()
        {
            return _currentIndex >= _questions.Count;
        }

        /*
         * Returns the final score as a readable string.
         * Example: "You scored 8 out of 13"
         */
        public string GetFinalScore()
        {
            return "You scored " + _score + " out of " + _questions.Count;
        }

        /*
         * Returns an encouraging message based on how well the user did.
         * The threshold is 70% — scoring 70% or above gives a positive message.
         */
        public string GetFinalMessage()
        {
            double percentage = (double)_score / _questions.Count * 100;                // Work out the percentage the user scored

            if (percentage >= 70)
            {
                return "Great job! You have a solid understanding of cybersecurity.";
            }
            else
            {
                return "Keep learning! Review the topics above and try again to improve your score.";
            }
        }

        /*
         * Resets the quiz back to the beginning.
         * Called when the user clicks Play Again.
         */
        public void ResetQuiz()
        {
            _currentIndex = 0;
            _score = 0;
        }

        /*
         * Returns the current question number for display.
         * Example: "Question 3 of 13"
         */
        public string GetQuestionProgress()
        {
            return "Question " + (_currentIndex + 1) + " of " + _questions.Count;
        }

        /*
         * Returns the current score during the quiz.
         * Example: "Score: 4"
         */
        public string GetCurrentScore()
        {
            return "Score: " + _score;
        }
    }
}