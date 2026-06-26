using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace CybersecurityAwarenessBot_SA
{
    // Task item class for database storage
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Quiz question class
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public string Type { get; set; }
    }

    // Activity log entry class
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }

    public class ChatbotEngine
    {
        // Events
        public event Action<string> OnResponseGenerated;

        // Database connection
        private string connectionString = "Server=localhost;Database=cyberbot;Uid=root;Pwd=good;";

        // Task management
        private List<TaskItem> tasks = new List<TaskItem>();

        // Quiz system
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private bool quizActive = false;
        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private int totalQuestions = 0;

        // Activity log
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private string logFilePath = "activity_log.json";

        // NLP patterns
        private Dictionary<string, List<string>> nlpPatterns;

        // Existing variables
        private Dictionary<string, List<string>> responses;
        private Dictionary<string, List<string>> followUpResponses;
        private Random random = new Random();
        private string userName = null;
        private string currentTopic = null;
        private string[] randomTips;
        private Dictionary<string, string[]> sentimentKeywords;
        private bool waitingForTaskTitle = false;

        // Properties for MainWindow
        public bool HasUserMemory
        {
            get { return !string.IsNullOrEmpty(userName); }
        }

        public string GetUserInfo()
        {
            if (!string.IsNullOrEmpty(userName))
            {
                return $"👤 {userName}";
            }
            return "No user data";
        }

        public ChatbotEngine()
        {
            InitializeResponses();
            InitializeSentimentKeywords();
            InitializeRandomTips();
            InitializeNLPPatterns();
            InitializeQuizQuestions();
            InitializeDatabase();
            LoadTasksFromDatabase();
            LoadActivityLog();
            LogActivity("System", "Chatbot initialized and ready");
        }

        private void InitializeDatabase()
        {
            try
            {
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS tasks (
                        Id INT PRIMARY KEY AUTO_INCREMENT,
                        Title VARCHAR(255) NOT NULL,
                        Description TEXT,
                        ReminderDate VARCHAR(100),
                        IsCompleted BOOLEAN DEFAULT FALSE,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(createTableQuery, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }

        private void LoadTasksFromDatabase()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM tasks WHERE IsCompleted = FALSE ORDER BY CreatedAt DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        tasks.Clear();
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32("Id"),
                                Title = reader.GetString("Title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? null : reader.GetString("ReminderDate"),
                                IsCompleted = reader.GetBoolean("IsCompleted"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load tasks error: {ex.Message}");
            }
        }

        private void SaveTaskToDatabase(TaskItem task)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO tasks (Title, Description, ReminderDate, IsCompleted) 
                                    VALUES (@Title, @Description, @ReminderDate, @IsCompleted)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Title", task.Title);
                    cmd.Parameters.AddWithValue("@Description", task.Description);
                    cmd.Parameters.AddWithValue("@ReminderDate", task.ReminderDate);
                    cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
                    cmd.ExecuteNonQuery();

                    task.Id = (int)cmd.LastInsertedId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save task error: {ex.Message}");
            }
        }

        private void UpdateTaskInDatabase(TaskItem task)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE tasks SET IsCompleted = @IsCompleted WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
                    cmd.Parameters.AddWithValue("@Id", task.Id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update task error: {ex.Message}");
            }
        }

        private void DeleteTaskFromDatabase(int taskId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM tasks WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete task error: {ex.Message}");
            }
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What is phishing?",
                    Options = new List<string> { "A type of fishing", "A scam to steal personal information", "A password manager", "A type of antivirus" },
                    CorrectAnswer = 1,
                    Explanation = "Phishing is when scammers try to trick you into giving personal information through fake emails or messages.",
                    Type = "multiple"
                },
                new QuizQuestion
                {
                    Question = "A strong password should be at least 8 characters long.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "Actually, strong passwords should be 12+ characters with a mix of letters, numbers, and symbols!",
                    Type = "truefalse"
                },
                new QuizQuestion
                {
                    Question = "What does 2FA stand for?",
                    Options = new List<string> { "Two Factor Authentication", "Two File Access", "Transfer File Application", "Total File Archive" },
                    CorrectAnswer = 0,
                    Explanation = "2FA means Two-Factor Authentication - an extra security layer for your accounts.",
                    Type = "multiple"
                },
                new QuizQuestion
                {
                    Question = "It is safe to use the same password for multiple accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "False! Using the same password everywhere means if one account is hacked, all your accounts are at risk.",
                    Type = "truefalse"
                },
                new QuizQuestion
                {
                    Question = "What should you do if you receive a suspicious email?",
                    Options = new List<string> { "Reply and ask questions", "Click the links to check", "Report it as phishing and delete", "Forward it to friends" },
                    CorrectAnswer = 2,
                    Explanation = "Always report phishing emails and delete them. Never click links or reply!",
                    Type = "multiple"
                },
                new QuizQuestion
                {
                    Question = "Public Wi-Fi is completely safe for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "False! Public Wi-Fi is not secure. Use a VPN or your mobile data for banking.",
                    Type = "truefalse"
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string> { "Software that steals passwords", "Software that locks your files and demands payment", "A type of antivirus", "A social media scam" },
                    CorrectAnswer = 1,
                    Explanation = "Ransomware is malicious software that locks your files and demands payment to unlock them.",
                    Type = "multiple"
                },
                new QuizQuestion
                {
                    Question = "You should never share your WhatsApp verification code with anyone.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 0,
                    Explanation = "True! Your WhatsApp verification code is private. Sharing it lets scammers take over your account.",
                    Type = "truefalse"
                },
                new QuizQuestion
                {
                    Question = "What is a VPN used for?",
                    Options = new List<string> { "To make internet faster", "To hide your online activity and encrypt data", "To block all websites", "To install antivirus" },
                    CorrectAnswer = 1,
                    Explanation = "A VPN (Virtual Private Network) encrypts your internet traffic and hides your online activity from hackers.",
                    Type = "multiple"
                },
                new QuizQuestion
                {
                    Question = "You should update your software immediately when updates are available.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 0,
                    Explanation = "True! Software updates often fix security holes that hackers could exploit.",
                    Type = "truefalse"
                },
                new QuizQuestion
                {
                    Question = "What is social engineering?",
                    Options = new List<string> { "Engineering social media", "Manipulating people to reveal information", "Building social networks", "A type of firewall" },
                    CorrectAnswer = 1,
                    Explanation = "Social engineering is tricking people into revealing confidential information through manipulation.",
                    Type = "multiple"
                }
            };
            totalQuestions = quizQuestions.Count;
        }

        private void InitializeNLPPatterns()
        {
            nlpPatterns = new Dictionary<string, List<string>>
            {
                ["add_task"] = new List<string>
                {
                    "add task", "new task", "create task", "add to my tasks", "add a task", "create a task",
                    "add reminder", "create reminder"
                },
                ["set_reminder"] = new List<string>
                {
                    "remind me", "set reminder", "remember to", "reminder for", "remind me to",
                    "set a reminder"
                },
                ["show_tasks"] = new List<string>
                {
                    "show tasks", "list tasks", "my tasks", "view tasks", "what tasks", "show me my tasks",
                    "show all tasks", "display tasks", "tasks", "show my tasks"
                },
                ["complete_task"] = new List<string>
                {
                    "complete task", "mark done", "finish task", "task done", "mark as complete",
                    "complete", "done", "finish"
                },
                ["delete_task"] = new List<string>
                {
                    "delete task", "remove task", "clear task", "delete this task", "remove this task",
                    "erase task", "delete", "remove"
                },
                ["start_quiz"] = new List<string>
                {
                    "start quiz", "take quiz", "play quiz", "cyber quiz", "test me", "quiz me",
                    "begin quiz", "start the quiz", "take the quiz", "let's quiz", "quiz"
                },
                ["show_log"] = new List<string>
                {
                    "show activity log", "what have you done", "activity log", "show log", "recent actions",
                    "what did you do", "show history", "log", "activities"
                },
                ["random_tip"] = new List<string>
                {
                    "random tip", "give me a tip", "tip please", "cyber tip", "security tip",
                    "tell me a tip", "give me a random tip", "tip", "tips", "security tips", "cyber tips"
                },
                ["help"] = new List<string>
                {
                    "help", "what can you do", "menu", "commands", "what can i ask",
                    "show commands", "show menu", "help me", "options"
                }
            };
        }

        private void InitializeRandomTips()
        {
            randomTips = new string[]
            {
                "81% of data breaches are caused by weak passwords! Use 12+ characters!",
                "Never share your WhatsApp verification code with anyone!",
                "Always use a VPN on public Wi-Fi to protect your data.",
                "Enable 2FA on all your important accounts today!",
                "Update your software when prompted - it fixes security holes!",
                "Think before you click - scammers rely on urgency and fear!",
                "Back up your important files to the cloud or external drive!",
                "Review app permissions regularly - remove ones you don't need!"
            };
        }

        private void InitializeResponses()
        {
            responses = new Dictionary<string, List<string>>
            {
                ["greeting"] = new List<string>
                {
                    "Hello! I can help with cybersecurity tips, tasks, or a quiz. Type 'help' to see all options!",
                    "Hi there! Need help staying safe online? Try 'start quiz' or 'add task'!",
                    "Welcome! I can teach you about cybersecurity, manage your tasks, or test your knowledge with a quiz!"
                },
                ["name_response"] = new List<string>
                {
                    "Nice to meet you, {0}! I can help you with cybersecurity tips, tasks, or a quiz. Type 'help'!",
                    "Thanks, {0}! What would you like to do? Learn about cybersecurity, manage tasks, or take a quiz?",
                    "Great to meet you, {0}! Type 'help' to see everything I can do for you."
                },
                ["whatsapp"] = new List<string>
                {
                    "Enable two-step verification in WhatsApp Settings, then Account, then Two-step verification. Create a 6-digit PIN to secure your account!"
                },
                ["password"] = new List<string>
                {
                    "Use strong passwords with 12+ characters including letters, numbers, and symbols. Never reuse passwords across different sites!"
                },
                ["phishing"] = new List<string>
                {
                    "Never click links in suspicious emails. Check the sender's address carefully and hover over links to see the real URL before clicking!"
                },
                ["2fa"] = new List<string>
                {
                    "Two-Factor Authentication adds an extra security layer. Download Google Authenticator and enable 2FA on all your important accounts!"
                },
                ["privacy"] = new List<string>
                {
                    "Review your app permissions regularly. Does your flashlight app really need access to your contacts? Use a VPN to protect your privacy online!"
                }
            };

            followUpResponses = new Dictionary<string, List<string>>
            {
                ["whatsapp"] = new List<string>
                {
                    "WhatsApp Setup: 1. Open WhatsApp 2. Settings 3. Account 4. Two-step verification 5. Enable 6. Create PIN",
                    "Never share your 6-digit WhatsApp code with anyone, even friends!"
                },
                ["password"] = new List<string>
                {
                    "Use a password manager like Bitwarden to generate and store strong passwords!",
                    "Change your passwords every 3-6 months for better security."
                }
            };
        }

        private void InitializeSentimentKeywords()
        {
            sentimentKeywords = new Dictionary<string, string[]>
            {
                ["worried"] = new string[] { "worried", "scared", "nervous", "anxious", "concerned", "afraid" },
                ["frustrated"] = new string[] { "frustrated", "annoying", "difficult", "complicated", "hard", "confusing" },
                ["curious"] = new string[] { "curious", "interested", "want to learn", "tell me", "explain" },
                ["thankful"] = new string[] { "thank", "thanks", "helpful", "appreciate" }
            };
        }

        private void LoadActivityLog()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    string json = File.ReadAllText(logFilePath);
                    activityLog = JsonSerializer.Deserialize<List<ActivityLogEntry>>(json) ?? new List<ActivityLogEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load log error: {ex.Message}");
                activityLog = new List<ActivityLogEntry>();
            }
        }

        private void SaveActivityLog()
        {
            try
            {
                var recentLogs = activityLog.Take(50).ToList();
                string json = JsonSerializer.Serialize(recentLogs);
                File.WriteAllText(logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save log error: {ex.Message}");
            }
        }

        private void LogActivity(string action, string details)
        {
            var logEntry = new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Details = details
            };

            activityLog.Insert(0, logEntry);

            if (activityLog.Count > 50)
                activityLog.RemoveAt(activityLog.Count - 1);

            SaveActivityLog();
        }

        // ============================================
        // SHOW NEXT OPTIONS - Shows what user can do next
        // ============================================
        private void ShowNextOptions()
        {
            string options = "\n📋 What would you like to do next?\n" +
                             "• Type 'help' to see all commands\n" +
                             "• Type 'tasks' to manage your tasks\n" +
                             "• Type 'quiz' to test your knowledge\n" +
                             "• Type 'tip' for a security tip\n" +
                             "• Type 'exit' to close the application";

            OnResponseGenerated?.Invoke(options);
        }

        public void ProcessUserInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                OnResponseGenerated?.Invoke("I didn't catch that. Could you please type something?");
                return;
            }

            string lowerInput = input.ToLower().Trim();
            string detectedSentiment = DetectSentiment(lowerInput);

            // Handle quiz first
            if (quizActive)
            {
                ProcessQuizAnswer(lowerInput);
                return;
            }

            // Handle general responses
            if (lowerInput == "okay" || lowerInput == "ok" || lowerInput == "thanks" || lowerInput == "thank you")
            {
                OnResponseGenerated?.Invoke($"You're welcome, {userName}! Is there anything else I can help you with?");
                return;
            }

            if (lowerInput == "yes" || lowerInput == "yeah" || lowerInput == "sure")
            {
                OnResponseGenerated?.Invoke("Great! What would you like to do? Type 'help' to see all options.");
                return;
            }

            if (lowerInput == "no" || lowerInput == "nope" || lowerInput == "not really")
            {
                OnResponseGenerated?.Invoke("No problem! Type 'help' anytime if you need something. Type 'exit' to close the application.");
                return;
            }

            // Check for exit
            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "bye" || lowerInput == "goodbye")
            {
                OnResponseGenerated?.Invoke($"Stay safe online, {userName}! Goodbye! 👋");
                return;
            }

            // Direct command mapping
            if (lowerInput == "tasks" || lowerInput == "show tasks" || lowerInput == "my tasks" ||
                lowerInput == "view tasks" || lowerInput == "list tasks")
            {
                ShowAllTasks();
                return;
            }

            if (lowerInput == "quiz" || lowerInput == "start quiz" || lowerInput == "take quiz" ||
                lowerInput == "play quiz" || lowerInput == "quiz me")
            {
                StartQuiz();
                return;
            }

            if (lowerInput == "log" || lowerInput == "activity log" || lowerInput == "show log" ||
                lowerInput == "show activity log" || lowerInput == "what have you done")
            {
                ShowActivityLog();
                return;
            }

            if (lowerInput == "tip" || lowerInput == "tips" || lowerInput == "random tip" ||
                lowerInput == "give me a tip" || lowerInput == "security tip" || lowerInput == "cyber tip" ||
                lowerInput == "security tips" || lowerInput == "cyber tips")
            {
                ShowRandomTip();
                return;
            }

            if (lowerInput == "help" || lowerInput == "menu" || lowerInput == "what can you do")
            {
                ShowHelpMenu();
                return;
            }

            // Check for add task
            if (lowerInput.Contains("add task") || lowerInput.Contains("new task") || lowerInput.Contains("create task"))
            {
                ProcessAddTask(input);
                return;
            }

            // Check for reminder
            if (lowerInput.Contains("remind me") || lowerInput.Contains("set reminder") || lowerInput.Contains("remember to"))
            {
                ProcessSetReminder(input);
                return;
            }

            // Check for complete/delete with numbers
            if (lowerInput.Contains("complete") && Regex.IsMatch(lowerInput, @"\d+"))
            {
                ProcessCompleteTask(input);
                return;
            }

            if ((lowerInput.Contains("delete") || lowerInput.Contains("remove")) && Regex.IsMatch(lowerInput, @"\d+"))
            {
                ProcessDeleteTask(input);
                return;
            }

            // Check for tell me more
            if (lowerInput.Contains("tell me more") && !string.IsNullOrEmpty(currentTopic))
            {
                if (followUpResponses.ContainsKey(currentTopic))
                {
                    string followUp = followUpResponses[currentTopic][random.Next(followUpResponses[currentTopic].Count)];
                    OnResponseGenerated?.Invoke(followUp);
                    return;
                }
            }

            // Check for name
            if (string.IsNullOrEmpty(userName) && !lowerInput.Contains("?") && lowerInput.Length < 25)
            {
                ProcessNameInput(input);
                return;
            }

            // Regular cybersecurity topic response
            string response = GetKeywordResponse(lowerInput, detectedSentiment);
            if (!string.IsNullOrEmpty(userName))
                response = response.Replace("{0}", userName);

            OnResponseGenerated?.Invoke(response);
        }

        private string RecognizeIntent(string input)
        {
            foreach (var intent in nlpPatterns)
            {
                foreach (string pattern in intent.Value)
                {
                    if (input.Contains(pattern))
                        return intent.Key;
                }
            }
            return "unknown";
        }

        private void ProcessAddTask(string input)
        {
            string taskTitle = "";
            string[] removePhrases = { "add task", "new task", "create task", "add to my tasks", "add a task", "create a task", "add reminder", "create reminder" };

            taskTitle = input;
            foreach (string phrase in removePhrases)
            {
                taskTitle = taskTitle.Replace(phrase, "").Trim();
            }

            taskTitle = taskTitle.TrimStart('-').Trim();

            if (string.IsNullOrEmpty(taskTitle))
            {
                OnResponseGenerated?.Invoke("What task would you like to add? Example: 'add task - Review privacy settings'");
                return;
            }

            var newTask = new TaskItem
            {
                Title = taskTitle,
                Description = taskTitle,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            SaveTaskToDatabase(newTask);
            LoadTasksFromDatabase();

            LogActivity("Task Added", $"Task '{taskTitle}' was created");
            OnResponseGenerated?.Invoke($"✅ Task '{taskTitle}' has been added!");
            ShowNextOptions();
        }

        private void ProcessSetReminder(string input)
        {
            string reminderText = "";
            string[] removePhrases = { "remind me to", "set reminder", "remember to", "reminder for", "remind me", "set a reminder" };

            reminderText = input;
            foreach (string phrase in removePhrases)
            {
                reminderText = reminderText.Replace(phrase, "").Trim();
            }

            string reminderDate = "";
            if (input.Contains("tomorrow"))
                reminderDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            else if (input.Contains("in 3 days"))
                reminderDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
            else if (input.Contains("in 7 days") || input.Contains("in a week"))
                reminderDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            else
                reminderDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            LogActivity("Reminder Set", $"Reminder for '{reminderText}' on {reminderDate}");
            OnResponseGenerated?.Invoke($"⏰ Reminder set for '{reminderText}' on {reminderDate}. I'll remind you then!");
            ShowNextOptions();
        }

        private void ShowAllTasks()
        {
            if (tasks.Count == 0)
            {
                OnResponseGenerated?.Invoke("📋 You have no pending tasks. Type 'add task - description' to create one!");
                return;
            }

            string taskList = "📋 Here are your pending tasks:\n\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                taskList += $"{i + 1}. {task.Title}";
                if (!string.IsNullOrEmpty(task.ReminderDate))
                    taskList += $" (Reminder: {task.ReminderDate})";
                taskList += "\n";
            }
            taskList += "\n💡 Type 'complete 1' to mark a task as done, or 'delete 1' to remove it.";

            LogActivity("Viewed Tasks", "User requested to see all tasks");
            OnResponseGenerated?.Invoke(taskList);
            ShowNextOptions();
        }

        private void ProcessCompleteTask(string input)
        {
            var match = Regex.Match(input, @"\d+");
            if (match.Success && tasks.Count > 0)
            {
                int taskIndex = int.Parse(match.Value) - 1;
                if (taskIndex >= 0 && taskIndex < tasks.Count)
                {
                    var task = tasks[taskIndex];
                    task.IsCompleted = true;
                    UpdateTaskInDatabase(task);
                    LoadTasksFromDatabase();

                    LogActivity("Task Completed", $"Task '{task.Title}' was marked as completed");
                    OnResponseGenerated?.Invoke($"🎉 Great job! Task '{task.Title}' marked as completed!");
                    ShowNextOptions();
                    return;
                }
            }
            OnResponseGenerated?.Invoke("Please specify which task to complete. Example: 'complete 1'");
        }

        private void ProcessDeleteTask(string input)
        {
            var match = Regex.Match(input, @"\d+");
            if (match.Success && tasks.Count > 0)
            {
                int taskIndex = int.Parse(match.Value) - 1;
                if (taskIndex >= 0 && taskIndex < tasks.Count)
                {
                    var task = tasks[taskIndex];
                    DeleteTaskFromDatabase(task.Id);
                    LoadTasksFromDatabase();

                    LogActivity("Task Deleted", $"Task '{task.Title}' was deleted");
                    OnResponseGenerated?.Invoke($"🗑️ Task '{task.Title}' has been deleted.");
                    ShowNextOptions();
                    return;
                }
            }
            OnResponseGenerated?.Invoke("Please specify which task to delete. Example: 'delete 1'");
        }

        private void StartQuiz()
        {
            quizActive = true;
            currentQuestionIndex = 0;
            quizScore = 0;

            LogActivity("Quiz Started", "User started the cybersecurity quiz");
            OnResponseGenerated?.Invoke("🎯 Starting the cybersecurity quiz!\n\nType the number of your answer (1, 2, 3, etc.) or 'true'/'false'.\n");
            ShowNextQuestion();
        }

        private void ShowNextQuestion()
        {
            if (currentQuestionIndex >= totalQuestions)
            {
                EndQuiz();
                return;
            }

            var q = quizQuestions[currentQuestionIndex];
            string questionText = $"\n📚 Question {currentQuestionIndex + 1} of {totalQuestions}\n\n{q.Question}\n";

            for (int i = 0; i < q.Options.Count; i++)
            {
                questionText += $"{i + 1}. {q.Options[i]}\n";
            }

            OnResponseGenerated?.Invoke(questionText);
        }

        private void ProcessQuizAnswer(string input)
        {
            var q = quizQuestions[currentQuestionIndex];
            int answerIndex = -1;

            var match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                answerIndex = int.Parse(match.Value) - 1;
            }

            if (input.Contains("true") || input.Contains("false"))
            {
                answerIndex = input.Contains("true") ? 0 : 1;
            }

            if (answerIndex >= 0 && answerIndex < q.Options.Count)
            {
                bool isCorrect = (answerIndex == q.CorrectAnswer);

                if (isCorrect)
                {
                    quizScore++;
                    OnResponseGenerated?.Invoke($"✅ Correct! {q.Explanation}\n\n📊 Score: {quizScore}/{currentQuestionIndex + 1}");
                }
                else
                {
                    string correctAnswer = q.Options[q.CorrectAnswer];
                    OnResponseGenerated?.Invoke($"❌ Incorrect. The correct answer is: {correctAnswer}\n\n{q.Explanation}\n\n📊 Score: {quizScore}/{currentQuestionIndex + 1}");
                }

                currentQuestionIndex++;
                ShowNextQuestion();
            }
            else
            {
                OnResponseGenerated?.Invoke("Please enter the number of your answer (1, 2, 3, etc.) or 'true'/'false'.");
            }
        }

        private void EndQuiz()
        {
            quizActive = false;
            string feedback;

            if (quizScore >= totalQuestions * 0.8)
                feedback = "🏆 Excellent! You're a cybersecurity pro! Keep up the great work!";
            else if (quizScore >= totalQuestions * 0.6)
                feedback = "👍 Good job! You have a solid understanding. Keep learning to become a cybersecurity expert!";
            else
                feedback = "📚 Good try! Cybersecurity is important for everyone. Review the tips and try the quiz again!";

            LogActivity("Quiz Completed", $"User scored {quizScore}/{totalQuestions} on the quiz");
            OnResponseGenerated?.Invoke($"🎯 Quiz complete! Your final score: {quizScore}/{totalQuestions}\n\n{feedback}");
            ShowNextOptions();
        }

        private void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                OnResponseGenerated?.Invoke("No activities have been logged yet.");
                return;
            }

            string logDisplay = "📋 Here's a summary of recent actions:\n\n";
            int displayCount = Math.Min(10, activityLog.Count);

            for (int i = 0; i < displayCount; i++)
            {
                var log = activityLog[i];
                logDisplay += $"{i + 1}. {log.Timestamp:HH:mm:ss} - {log.Action}: {log.Details}\n";
            }

            OnResponseGenerated?.Invoke(logDisplay);
            ShowNextOptions();
        }

        private void ShowRandomTip()
        {
            string tip = randomTips[random.Next(randomTips.Length)];
            LogActivity("Random Tip", "User requested a random cybersecurity tip");
            OnResponseGenerated?.Invoke($"💡 Security Tip: {tip}");
            ShowNextOptions();
        }

        private void ShowHelpMenu()
        {
            string menu = "🔐 CYBERSECURITY BOT - HELP MENU 🔐\n\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "📚 CYBERSECURITY TOPICS:\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "• 'whatsapp' - WhatsApp security tips\n" +
                          "• 'password' - Password safety tips\n" +
                          "• 'phishing' - How to spot scams\n" +
                          "• '2fa' - Two-Factor Authentication\n" +
                          "• 'privacy' - Privacy protection tips\n" +
                          "• 'tell me more' - Get additional tips\n\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "✅ TASKS AND REMINDERS:\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "• 'add task - description' - Add a new task\n" +
                          "• 'tasks' - View all your tasks\n" +
                          "• 'complete 1' - Mark task as done\n" +
                          "• 'delete 1' - Remove a task\n" +
                          "• 'remind me to [task]' - Set a reminder\n\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "🎮 QUIZ:\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "• 'quiz' - Test your knowledge (11 questions)\n\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "📋 OTHER COMMANDS:\n" +
                          "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                          "• 'tip' - Get a cybersecurity tip\n" +
                          "• 'log' - See what I've done\n" +
                          "• 'help' - Show this menu\n" +
                          "• 'exit' - Close the application";

            OnResponseGenerated?.Invoke(menu);
        }

        private string DetectSentiment(string input)
        {
            if (sentimentKeywords["worried"].Any(k => input.Contains(k)))
                return "worried";
            if (sentimentKeywords["frustrated"].Any(k => input.Contains(k)))
                return "frustrated";
            if (sentimentKeywords["curious"].Any(k => input.Contains(k)))
                return "curious";
            if (sentimentKeywords["thankful"].Any(k => input.Contains(k)))
                return "thankful";
            return "neutral";
        }

        private string GetKeywordResponse(string input, string sentiment)
        {
            string response = "";

            if (input.Contains("whatsapp") || input.Contains("verification"))
            {
                currentTopic = "whatsapp";
                response = GetRandomResponse("whatsapp");
            }
            else if (input.Contains("password"))
            {
                currentTopic = "password";
                response = GetRandomResponse("password");
            }
            else if (input.Contains("phish") || input.Contains("scam"))
            {
                currentTopic = "phishing";
                response = GetRandomResponse("phishing");
            }
            else if (input.Contains("2fa") || input.Contains("two factor"))
            {
                currentTopic = "2fa";
                response = GetRandomResponse("2fa");
            }
            else if (input.Contains("privacy"))
            {
                currentTopic = "privacy";
                response = GetRandomResponse("privacy");
            }
            else
            {
                string[] defaultResponses = {
                    "I'm not sure what you mean. Type 'help' to see all available commands!",
                    "Not sure about that. Try 'help' to see what I can do!",
                    "I didn't catch that. Type 'help' for a list of commands!"
                };
                response = defaultResponses[random.Next(defaultResponses.Length)];
            }

            if (sentiment == "worried")
            {
                return $"😟 I understand your concern. {response}";
            }
            else if (sentiment == "curious")
            {
                return $"🤔 Great question! {response}";
            }

            return response;
        }

        private void ProcessNameInput(string input)
        {
            string[] words = input.Trim().Split(' ');
            string extractedName = words[0];
            extractedName = Regex.Replace(extractedName, @"[^a-zA-Z]", "");

            if (!string.IsNullOrEmpty(extractedName) && extractedName.Length >= 2)
            {
                userName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(extractedName.ToLower());
                string response = string.Format(GetRandomResponse("name_response"), userName);
                LogActivity("User Recognized", $"User identified as {userName}");
                OnResponseGenerated?.Invoke(response);
            }
        }

        private string GetRandomResponse(string key)
        {
            if (responses.ContainsKey(key) && responses[key].Count > 0)
                return responses[key][random.Next(responses[key].Count)];
            return "Type 'help' to see everything I can help you with!";
        }
    }
}