# Cybersecurity Chatbot

**A modern WPF chatbot application that delivers personalized cybersecurity awareness training through interactive conversations.**

---

## Student Information

**Name:** Smith Mbele 
**Student Number:** ST10472346

---

## Project Overview

This is a Windows Presentation Foundation (WPF) desktop application that engages users in an interactive conversation about cybersecurity topics. The chatbot learns the user's name and favorite cybersecurity topic during onboarding, then provides context-aware responses using sentiment analysis and keyword matching.

---

## Features Implemented (Part 2)

- ✅ **WPF Desktop Interface** – Modern, responsive chat UI with bubble-based message display
- ✅ **User Onboarding Flow** – Asks for user's name and favorite cybersecurity topic on first interaction
- ✅ **Memory System** – Stores and recalls user information across the session
- ✅ **Personalized Responses** – Tailors greeting and responses based on user preferences
- ✅ **Sentiment Detection** – Analyzes user input sentiment and responds empathetically
- ✅ **Keyword Responder** – Matches cybersecurity keywords (phishing, passwords, malware, etc.) and provides relevant answers
- ✅ **One-Shot Personalized Introduction** – Shows custom intro message only once after onboarding completes
- ✅ **Responsive Chat Layout** – Chat panel resizes and adapts to window size
- ✅ **Animated Welcome Avatar** – Spinning and blinking bot avatar with SVG-like design
- ✅ **Welcome Sound** – Plays greeting.wav on app startup
- ✅ **Collapsible Chat Panel** – Hide/show chat interface with smooth animations
- ✅ **Clean Separation of Concerns** – UI logic in MainWindow, business logic in ChatBot class
- ✅ **XAML Data Binding** – Chat messages rendered via ItemsControl and DataTemplateSelector
- ✅ **GitHub Actions CI/CD** – Automated build and test pipeline on push

---

## Prerequisites

- **Visual Studio 2022** (Community edition or higher)
- **.NET 8.0** or .NET 10.0 SDK installed
- **Windows** (7, 10, 11) – Required for WPF and SoundPlayer
- **Git** (for cloning)

---

## Clone and Run Instructions

### Step 1: Clone the Repository

```powershell
git clone https://github.com/YourUsername/ST10472346_CybersecurityChatbot.git
cd ST10472346_CybersecurityChatbot
```

### Step 2: Set Up Audio File

Place the `greeting.wav` file in the following location:

```
CybersecurityChatbot/assets/greeting.wav
```

If the file is missing, the app will still run but the welcome sound will not play. Create the `assets` folder if it doesn't exist.

### Step 3: Open the Project

```powershell
# Option A: Open in Visual Studio 2022
start CybersecurityChatbot\CybersecurityChatbot.sln

# Option B: Build and run from command line
cd CybersecurityChatbot
dotnet restore
dotnet build
dotnet run
```

### Step 4: Run the Application

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

---

## Video Demonstration

**YouTube Link (Unlisted):**  
https://www.youtube.com/watch?v=YOUR_VIDEO_ID_HERE

*Replace the URL above with your actual unlisted YouTube video demonstrating the chatbot.*

---

## GitHub Actions CI/CD

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
│   ├── MainWindow.xaml               # UI layout and data binding
│   ├── MainWindow.xaml.cs            # UI event handling
│   ├── ChatBot.cs                    # Core conversation logic
│   ├── ChatMessage.cs                # Message model for binding
│   ├── ChatRoleTemplateSelector.cs   # Template selector for bubble rendering
│   ├── MemoryStore.cs                # User information persistence
│   ├── KeywordResponder.cs           # Keyword-based response system
│   ├── SentimentDetector.cs          # Sentiment analysis
│   ├── UI.cs                         # UI utilities
│   ├── Logo.cs                       # ASCII art generation
│   ├── assets/
│   │   ├── greeting.wav              # Welcome sound (required)
│   │   ├── screenshot-main.png       # Main window screenshot
│   │   └── screenshot-chat.png       # Chat interface screenshot
│   └── bin/, obj/                    # Build artifacts
└── .github/
    └── workflows/
        └── ci.yml                    # GitHub Actions workflow
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

