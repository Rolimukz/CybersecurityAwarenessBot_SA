Cybersecurity Awareness Bot - South Africa
What I Built
I created a desktop chatbot that helps South Africans learn about staying safe online. The bot talks to you, remembers your name, understands how you're feeling, and answers questions about things like passwords, WhatsApp security, and spotting scams. For Part 3, I added four new features: a Task Assistant with MySQL database, a Cybersecurity Quiz, Natural Language Processing (NLP) simulation, and an Activity Log.

What It Can Do
Plays my recorded voice when the app starts. Has a chat window where I type and the bot replies in different coloured bubbles. Has buttons at the top for topics like Passwords, WhatsApp, 2FA, Wi-Fi, SA Scams, and Privacy. Remembers my name once I tell it. If I type "tell me more", it gives extra tips about whatever we were just talking about. If I say I'm worried or confused, it responds in a kinder way. I can type "random tip" and it gives me a cybersecurity fact. I can clear the chat or click Exit to close.

New Part 3 Features
I can add tasks like "Enable 2FA on my email", view all my tasks, complete them, or delete them. All tasks are saved in a MySQL database so they don't disappear when I close the app. I can type "quiz" to test my knowledge with 11 questions about cybersecurity. The bot tells me if I'm right or wrong and gives me a final score. The bot understands different ways I might ask for things. For example, "add task", "new task", and "create task" all do the same thing. The bot remembers everything it has done for me. I can type "log" to see the last 10 actions it performed.

How I Made It
I used C# with WPF for the user interface. There are three main files.

MainWindow.xaml is where I designed how the app looks including colours, buttons, and chat bubbles. MainWindow.xaml.cs handles what happens when you click buttons or type messages. ChatbotEngine.cs is the brain. It stores all the answers, recognises keywords, remembers your name, and now also handles tasks, quiz, NLP, and activity logging.

How to Run It
You need Windows 10 or 11, Visual Studio 2022 or later, MySQL Server installed, and .NET 8.0 SDK.

First, clone or download the project. Open the project in Visual Studio. Install MySQL on your computer. Create a database called "cyberbot" and a table called "tasks". Update the connection string in ChatbotEngine.cs with your MySQL password. Put your voice file called Recording005.wav in the project folder if you want the greeting to play. Press F5 to run.

MySQL Setup Commands
CREATE DATABASE cyberbot;
USE cyberbot;
CREATE TABLE tasks (
Id INT PRIMARY KEY AUTO_INCREMENT,
Title VARCHAR(255) NOT NULL,
Description TEXT,
ReminderDate VARCHAR(100),
IsCompleted BOOLEAN DEFAULT FALSE,
CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

How to Use It
When the app starts, it will ask for your name. Just type it and press Enter.

New Commands for Part 3
Type "add task - description" to add a new task. Type "tasks" or "show tasks" to see all your pending tasks. Type "complete 1" to mark a task as completed. Type "delete 1" to delete a task. Type "remind me to task" to set a reminder. Type "quiz" or "start quiz" to start the cybersecurity quiz. Type "log" or "show activity log" to see recent bot actions. Type "tip" or "random tip" to get a cybersecurity tip.

Old Commands That Still Work
You can click any topic button to learn about that subject. Type "tell me more" after any answer to get more information. Type "menu" to see all topics. Click Clear to erase the chat history. Click Exit or type "exit" to close.

A Quick Example
Bot asks for my name
I type: Thabo
Bot says: Nice to meet you, Thabo!

I type: add task - Enable 2FA on my email
Bot says: Task 'Enable 2FA on my email' has been added!

I type: tasks
Bot says: Here are your pending tasks:

Enable 2FA on my email

I type: quiz
Bot asks: Question 1 of 11: What is phishing?

A type of fishing

A scam to steal personal information

A password manager

A type of antivirus

I type: 2
Bot says: Correct! Phishing is when scammers try to trick you into giving personal information.

I type: log
Bot says: Here's a summary of recent actions:

Task Added: Task 'Enable 2FA on my email' was created

Quiz Started: User started the cybersecurity quiz

I type: exit
Bot says: Stay safe online, Thabo! Goodbye!

File Structure
CybersecurityAwarenessBot_SA/
├── MainWindow.xaml # User interface design
├── MainWindow.xaml.cs # Button clicks and message display
├── ChatbotEngine.cs # Bot responses, tasks, quiz, NLP, activity log
├── Recording005.wav # Your voice greeting
├── activity_log.json # Auto-generated activity log file
└── README.md # This file

Technologies Used
I used C# with .NET 8.0 as the core programming language. WPF was used for the GUI framework. XAML was used for user interface design. MySQL was used for database task storage. System.Media was used for playing the voice greeting. System.Text.Json was used for activity log file storage.

Video Presentation
YouTube link: https://youtu.be/NcZlZTCtRzc

GitHub Repository
Repository link: https://github.com/Rolimukz/CybersecurityAwarenessBot_SA.git

Author
Name: Mukwevho Makwarela Rolivhuwa




---

Stay safe online, South Africa.

