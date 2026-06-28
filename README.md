# Cybersecurity Chatbot - Complete Edition (Parts 1, 2 & 3)

**A comprehensive Windows Presentation Foundation (WPF) chatbot application that delivers personalized cybersecurity awareness training through interactive conversations, task management and knowledge assessments.**

This project spans **three development phases**:
- **Part 1:** Console-based chatbot with keyword matching and sentiment analysis
- **Part 2:** WPF desktop GUI with user onboarding and personalized responses
- **Part 3:** Enhanced features including task management, reminders, quiz functionality and activity logging

---

## Student Information

**Name:** Smith Mbele 

**Student Number:** ST10472346

---

## Project Overview

This is a comprehensive Windows Presentation Foundation (WPF) desktop application that engages users in intelligent conversations about cybersecurity. 

The chatbot learns the user's name and favorite cybersecurity topic during onboarding, then provides context-aware responses using sentiment analysis and keyword matching. 

In Part 3, the application expands to include task management, reminders, a cybersecurity quiz system and detailed activity logging.

---

## Features Implemented (Parts 1, 2 & 3)

### Part 1: Console-Based Foundation
- ✅ **Console User Interface** – Terminal-based interactive chatbot
- ✅ **Keyword Matching System** – Recognizes cybersecurity topics (phishing, passwords, malware, VPN, etc.)
- ✅ **Sentiment Analysis** – Detects user emotions (happy, sad, neutral, angry) and responds empathetically
- ✅ **Pre-programmed Responses** – Curated responses for each cybersecurity topic
- ✅ **User Engagement** – Maintains conversation flow with appropriate follow-up prompts

### Part 2: WPF Desktop Interface
- ✅ **WPF Desktop Interface** – Modern, responsive chat UI with bubble-based message display
- ✅ **User Onboarding Flow** – Asks for user's name and favorite cybersecurity topic on first interaction
- ✅ **Memory System** – Stores and recalls user information across the session
- ✅ **Personalized Responses** – Tailors greeting and responses based on user preferences
- ✅ **Sentiment Detection** – Analyzes user input sentiment and responds empathetically
- ✅ **Keyword Responder** – Matches cybersecurity keywords and provides relevant answers
- ✅ **One-Shot Personalized Introduction** – Shows custom intro message only once after onboarding completes
- ✅ **Responsive Chat Layout** – Chat panel resizes and adapts to window size
- ✅ **Animated Welcome Avatar** – Spinning and blinking bot avatar with SVG-like design
- ✅ **Welcome Sound** – Plays greeting.wav on app startup
- ✅ **Collapsible Chat Panel** – Hide/show chat interface with smooth animations
- ✅ **Clean Separation of Concerns** – UI logic in MainWindow, business logic in ChatBot class
- ✅ **XAML Data Binding** – Chat messages rendered via ItemsControl and DataTemplateSelector
- ✅ **GitHub Actions CI/CD** – Automated build and test pipeline on push

### Part 3: Task Management, Quiz & Activity Tracking
- ✅ **Task Manager (CRUD Operations)** – Add, view, complete and delete cybersecurity tasks
- ✅ **Persistent Task Storage** – Tasks saved to `tasks.json` for persistence across sessions
- ✅ **Reminder System** – Set and manage reminders for tasks with user-defined timeframes
- ✅ **Natural Language Processing (NLP)** – Detects user intent from conversational input
- ✅ **Intent Recognition** – Identifies requests like "add task", "remind me", "take quiz", "show log"
- ✅ **Task Chat Integration** – Multi-step task creation flow with title, description and reminders
- ✅ **Quiz System** – Interactive cybersecurity knowledge assessment with multiple-choice questions
- ✅ **Quiz Score Tracking** – Displays results and provides feedback after quiz completion
- ✅ **Activity Logger** – Logs all user actions (task management, quiz attempts, keyword matches)
- ✅ **Activity Log Viewer** – View recent actions (last 10) with option to see full history
- ✅ **Help/Menu System** – Contextual menu showing available commands and features
- ✅ **Multi-step Dialog Flows** – Guided conversations for task creation and reminder setup
- ✅ **Error Handling** – Graceful recovery from invalid inputs and edge cases
- ✅ **State Management** – Tracks conversation state (onboarding, task flow, quiz mode)

---

## Prerequisites

- **Visual Studio 2022** (Community edition or higher)
- **.NET 8.0** or **.NET 10.0** SDK installed
- **Newtonsoft.Json NuGet package** (for JSON serialization)
- **Windows** (7, 10, 11) – Required for WPF and SoundPlayer
- **Git** (for cloning)

---

## Clone and Run Instructions

### Step 1: Clone the Repository

```powershell
git clone https://github.com/YourUsername/ST10472346_CybersecurityChatbot.git
cd CybersecurityChatbot
```

### Step 2: Install NuGet Dependencies

The Newtonsoft.Json package is required for JSON serialization (used for task persistence):

**Option A: Install via Visual Studio Package Manager**

1. Open the solution in Visual Studio 2022
2. Go to **Tools → NuGet Package Manager → Package Manager Console**
3. Run the command:
   ```powershell
   Install-Package Newtonsoft.Json
   ```

**Option B: Install via dotnet CLI**

```powershell
cd CybersecurityChatbot
dotnet add package Newtonsoft.Json
```

### Step 3: Automatic Task File Creation

The `tasks.json` file is **automatically created** when you add your first task in the application. 

No manual setup is required — the file will be generated in the `CybersecurityChatbot` directory with the following structure:

```json
{
  "tasks": [
    {
      "id": 1,
      "title": "Task Title",
      "description": "Task Description",
      "isComplete": false,
      "reminder": "Reminder details",
      "createdAt": "2026-05-30 14:30:00"
    }
  ]
}
```

### Step 4: Set Up Audio File

Place the `greeting.wav` file in the following location:

```
CybersecurityChatbot/assets/greeting.wav
```

If the file is missing, the app will still run but the welcome sound will not play. 

Create the `assets` folder if it doesn't exist.

### Step 5: Open the Project

```powershell
# Option A: Open in Visual Studio 2022
start CybersecurityChatbot\CybersecurityChatbot.sln

# Option B: Build and run from command line
cd CybersecurityChatbot
dotnet restore
dotnet build
dotnet run
```

### Step 6: Run the Application

From within Visual Studio, press `F5` or select **Debug → Start Debugging**.

Or from the terminal:

```powershell
cd CybersecurityChatbot
dotnet run
```

---

## Build and Test

### Build

```powershell
dotnet build CybersecurityChatbot/CybersecurityChatbot.csproj
```

### Run Tests

```powershell
dotnet test CybersecurityChatbot/CybersecurityChatbot.csproj
```

---

## Screenshots

### Running Application

![Main Window](CybersecurityChatbot/assets/screenshot-main.png)

*The chatbot displaying the welcome screen with ASCII art avatar and "Open Chat" button.*

### Chat Interface

![Chat Panel](CybersecurityChatbot/assets/screenshot-chat.png)

*Onboarding flow showing name capture, topic selection and personalized greeting.*

Part three screenshot CybersecurityChatbot/assets/Part3_Screenshot.png
---

## Video Demonstration

**YouTube Link (Unlisted):**  
https://youtu.be/vFwU3Uj0hvw

*Watch the complete demonstration of the Cybersecurity Chatbot including onboarding, task management, quiz functionality and activity logging.*

---

## Release History

### Release 3.0 – Part 3: Task Management, Quiz & Activity Logging
**Date:** Jun 2026  
**Features:**
- Complete task management system (CRUD operations)
- Natural Language Processing for intent detection
- Interactive cybersecurity quiz with scoring
- Activity logging and history viewer
- Reminder system with task associations
- Enhanced multi-step conversation flows
- Persistent storage via tasks.json

**GitHub Release:** [v3.0](https://github.com/13wealth/ST10472346_CybersecurityChatbot/releases/tag/v3.0)

### Release 2.0 – Part 2: WPF Desktop Interface
**Date:** April 2026  
**Features:**
- Complete WPF desktop GUI with chat bubbles
- User onboarding flow (name and favorite topic)
- Personalized greetings and responses
- Animated bot avatar with sound effects
- Collapsible chat panel with animations
- GitHub Actions CI/CD pipeline

**GitHub Release:** [v2.0](https://github.com/13wealth/ST10472346_CybersecurityChatbot/releases/tag/v2.0)

### Release 1.0 – Part 1: Console Chatbot Foundation
**Date:** March 2026  
**Features:**
- Console-based interactive chatbot
- Keyword matching for cybersecurity topics
- Sentiment analysis and empathetic responses
- Topic explanations (phishing, passwords, malware, VPN, firewall, ransomware)
- Conversation flow management

**GitHub Release:** [v1.0](https://github.com/13wealth/ST10472346_CybersecurityChatbot/releases/tag/v1.0)

---

### Workflow Status

✅ **Build & Test:** Passing

![GitHub Actions Status](CybersecurityChatbot/assets/github-actions-status.png)

**Workflow:** `.github/workflows/ci.yml`

The CI pipeline automatically runs on every push and pull request:

1. **Restore** – Restore NuGet packages
2. **Build** – Compile the project
3. **Test** – Execute any unit tests

---

## Project Structure

```
ST10472346_CybersecurityChatbot/
├── README.md
├── CybersecurityChatbot/
│   ├── CybersecurityChatbot.sln
│   ├── CybersecurityChatbot.csproj
│   ├── App.xaml                      # Application configuration
│   ├── App.xaml.cs                   # Application code-behind
│   ├── MainWindow.xaml               # UI layout and data binding
│   ├── MainWindow.xaml.cs            # UI event handling
│   ├── AssemblyInfo.cs               # Assembly metadata
│   ├── icon.ico                      # Application icon
│   │
│   ├── ChatBot.cs                    # Core conversation logic (Parts 1-3)
│   ├── ChatMessage.cs                # Message model for binding
│   ├── ChatRoleTemplateSelector.cs   # Template selector for bubble rendering
│   ├── MemoryStore.cs                # User information persistence
│   ├── KeywordResponder.cs           # Keyword-based response system
│   ├── SentimentDetector.cs          # Sentiment analysis
│   │
│   ├── TaskManager.cs                # CRUD operations for tasks (Part 3)
│   ├── TaskStorageHelper.cs          # JSON serialization for tasks (Part 3)
│   ├── CyberTask.cs                  # Task model definition (Part 3)
│   ├── QuizManager.cs                # Quiz logic and scoring (Part 3)
│   ├── QuizQuestion.cs               # Quiz question model (Part 3)
│   ├── ActivityLogger.cs             # Activity logging system (Part 3)
│   │
│   ├── UI.cs                         # UI utilities and typing effect
│   ├── Logo.cs                       # ASCII art generation
│   ├── SpinBehavior.cs               # Animation behavior for avatar
│   │
│   ├── assets/
│   │   ├── welcome.wav               # Welcome sound (greeting audio)
│   │   ├── chat-bg.jpg               # Chat background image
│   │   ├── icon.ico                  # Application icon
│   │   ├── Part3_Screenshot.png      # Part 3 GUI screenshot
│   │   └── ci-results.jpg            # GitHub Actions CI results
│   │
│   ├── bin/                          # Compiled binaries
│   ├── obj/                          # Build artifacts
│   └── Properties/
│       └── AssemblyInfo.cs           # Assembly information
│
├── Cybersecurity_Chatbot/            # Original Part 1 Console Project (Archive)
│   ├── Cybersecurity_Chatbot.csproj
│   ├── Program.cs
│   ├── ResponseSystem.cs
│   ├── InputValidation.cs
│   ├── StateSharing.cs
│   ├── UI.cs
│   ├── Logo.cs
│   ├── welcome.wav
│   ├── App.config
│   └── assets/
│       └── ci-results.jpg
│
├── .github/
│   └── workflows/
│       └── ci.yml                    # GitHub Actions workflow for CI/CD
│
└── .gitignore                        # Git ignore rules
```

---

## How to Use

1. **Start the app** – Run the application
2. **Enter your name** – Chatbot asks "What should I call you?"
3. **Select your topic** – Respond with your favorite cybersecurity topic
4. **Chat away** – The bot will provide personalized responses about cybersecurity topics including:
   - Phishing prevention
   - Password security
   - Malware awareness
   - Privacy protection
   - Suspicious links

---

## References

- OpenAI. (2026). ChatGPT (GPT-5.3-mini) assistance for ASCII art animation and state management. https://chat.openai.com/
- Patorjk.com. (n.d.). Text to ASCII Art Generator (TAAG). https://patorjk.com/software/taag/
- Microsoft Docs. (n.d.). Windows Presentation Foundation (WPF). https://docs.microsoft.com/en-us/dotnet/desktop/wpf/
- Microsoft Docs. (n.d.). GitHub Actions CI/CD. https://docs.github.com/en/actions

