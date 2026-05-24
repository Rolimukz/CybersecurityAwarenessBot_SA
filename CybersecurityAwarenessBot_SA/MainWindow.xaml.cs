using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace CybersecurityAwarenessBot_SA
{
    public partial class MainWindow : Window
    {
        // Core components
        private ChatbotEngine chatbot;
        private SoundPlayer greetingPlayer;
        private bool welcomeShown = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            PlayVoiceGreeting();
            ShowWelcomeMessage();
        }

        private void InitializeChatbot()
        {
            chatbot = new ChatbotEngine();
            chatbot.OnResponseGenerated += Chatbot_OnResponseGenerated;
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                string[] possiblePaths = {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recording0005.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Recording0005.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "greeting.wav"),
            Path.Combine(Directory.GetCurrentDirectory(), "Recording0005.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Recording0005.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "Recording0005.wav"),
        };

                string soundPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        soundPath = path;
                        break;
                    }
                }

                if (soundPath != null)
                {
                    greetingPlayer = new SoundPlayer(soundPath);
                    greetingPlayer.Play();
                    StatusText.Text = "🎵 Playing your voice greeting!";
                    Thread.Sleep(500);
                }
                else
                {
                    Console.Beep(440, 200);
                    Thread.Sleep(50);
                    Console.Beep(523, 200);
                    Thread.Sleep(50);
                    Console.Beep(659, 400);
                    StatusText.Text = "🔔 Beep greeting (place greeting.wav in folder)";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"⚠️ Error: {ex.Message}";
            }
        }

        private void ShowWelcomeMessage()
        {
            if (!welcomeShown)
            {
                welcomeShown = true;
                AddBotMessage("🔊 Hello welcome to the cybersecurity awareness bot, I'm here to help you stay safe online. 🇿🇦");
                AddBotMessage("What's your name? 😊");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                ProcessUserInput();
            }
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatStackPanel.Children.Clear();
            AddBotMessage("Chat cleared! How can I help you today? 😊");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotMessage("Thank you for using the Cybersecurity Awareness Bot! Stay safe online! 🛡️");
            Thread.Sleep(1000);
            Application.Current.Shutdown();
        }

        private void QuickTip_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string topic = button.Tag?.ToString();
            if (!string.IsNullOrEmpty(topic))
            {
                InputTextBox.Text = $"Tell me about {topic}";
                ProcessUserInput();
            }
        }

        private void MenuTip_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "menu";
            ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            string userInput = InputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
                return;

            if (userInput.ToLower() == "exit" || userInput.ToLower() == "quit")
            {
                ExitButton_Click(null, null);
                return;
            }

            AddUserMessage(userInput);
            InputTextBox.Clear();
            chatbot.ProcessUserInput(userInput);
        }

        private void Chatbot_OnResponseGenerated(string response)
        {
            Dispatcher.Invoke(() =>
            {
                AddBotMessage(response);
                StatusText.Text = $"Last response: {DateTime.Now:HH:mm:ss}";

                // Update memory status safely
                try
                {
                    if (chatbot.HasUserMemory)
                    {
                        MemoryStatus.Text = $"📝 Memory: Knows {chatbot.GetUserInfo()}";
                        MemoryStatus.Foreground = (Brush)new BrushConverter().ConvertFrom("#00FFAA");
                    }
                }
                catch { }
            });
        }

        private void AddUserMessage(string message)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ChatBubbleUser"),
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            ChatStackPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ChatBubbleBot")
            };

            var stackPanel = new StackPanel();

            var iconText = new TextBlock
            {
                Text = "🤖 Bot: ",
                Foreground = (Brush)new BrushConverter().ConvertFrom("#00FFAA"),
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };
            stackPanel.Children.Add(iconText);

            var messageText = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(messageText);

            border.Child = stackPanel;
            ChatStackPanel.Children.Add(border);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (greetingPlayer != null)
            {
                greetingPlayer.Stop();
                greetingPlayer.Dispose();
            }
            base.OnClosed(e);
        }
    }
}