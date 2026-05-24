using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CybersecurityAwarenessBot_SA
{
    public class ChatbotEngine
    {
        // Event
        public event Action<string> OnResponseGenerated;

        // Generic collections
        private Dictionary<string, List<string>> responses;
        private Dictionary<string, List<string>> followUpResponses;
        private Dictionary<string, string> userMemory;
        private List<string> conversationHistory;

        private Random random = new Random();
        private string userName = null;
        private string userInterest = null;
        private string currentTopic = null;
        private int topicCount = 0;

        private string currentSentiment = "neutral";
        private Dictionary<string, string[]> sentimentKeywords;

        private string[] randomTips;

        public ChatbotEngine()
        {
            InitializeResponses();
            InitializeSentimentKeywords();
            InitializeRandomTips();
            userMemory = new Dictionary<string, string>();
            conversationHistory = new List<string>();
        }

        public bool HasUserMemory => !string.IsNullOrEmpty(userName);

        public string GetUserInfo()
        {
            if (!string.IsNullOrEmpty(userName))
            {
                return $"👤 {userName}" + (!string.IsNullOrEmpty(userInterest) ? $" (interests: {userInterest})" : "");
            }
            return "No user data";
        }

        private void InitializeRandomTips()
        {
            randomTips = new string[]
            {
                "💡 81% of data breaches are caused by weak passwords! Use 12+ characters!",
                "💡 Never use the same password for multiple accounts!",
                "💡 Update your software when prompted - it fixes security holes!",
                "💡 Back up your important files to the cloud or external drive!",
                "💡 Think before you click - scammers rely on urgency and fear!",
                "💡 Enable 2FA on all your important accounts today!",
                "💡 Public Wi-Fi is not secure - always use a VPN!",
                "💡 Your phone needs security updates too - don't ignore them!",
                "💡 Never share your WhatsApp verification code with anyone!",
                "💡 If an offer seems too good to be true online, it probably is!"
            };
        }

        private void InitializeResponses()
        {
            responses = new Dictionary<string, List<string>>
            {
                ["greeting"] = new List<string>
                {
                    "Hello! Click any button above to learn about cybersecurity!",
                    "Hi there! Just click a button above to get started!",
                    "Welcome! Choose a topic from the buttons above!"
                },

                ["name_response"] = new List<string>
                {
                    "Nice to meet you, {0}! 👋 Click on any button above - Passwords, WhatsApp, 2FA, or others - to learn about cybersecurity!",
                    "Thanks, {0}! I'll remember your name. Just click any topic button above to get started!",
                    "Great to meet you, {0}! Tap on Passwords, WhatsApp, or any button above for cybersecurity tips!"
                },

                ["password"] = new List<string>
                {
                    "🔐 Strong passwords: Use 12+ characters with letters, numbers, and symbols. Never reuse passwords!",
                    "💡 Use a password manager like Bitwarden. It generates and stores complex passwords securely!",
                    "⚠️ Avoid using personal info like birthdays or pet names. Hackers can find this on social media!"
                },

                ["phishing"] = new List<string>
                {
                    "🎣 Never click links in suspicious emails. Check the sender's address carefully!",
                    "🔍 Hover over links before clicking to see the real URL. Scammers use similar-looking domains!",
                    "🚨 Red flags: Urgent language, spelling errors, requests for personal info = PHISHING!"
                },

                ["2fa"] = new List<string>
                {
                    "🔐 Two-Factor Authentication adds an extra security layer! Use Google Authenticator.",
                    "📱 Enable 2FA on email, banking, and social media. It protects you even if your password is stolen!",
                    "✅ Want to set up 2FA? Download an authenticator app, go to account security, and scan the QR code!"
                },

                ["whatsapp"] = new List<string>
                {
                    "📱 **WhatsApp Two-Step Verification:**\n\nGo to Settings → Account → Two-step verification → Enable\n\nCreate a 6-digit PIN that will be required when registering your number on a new device!\n\n💡 Also add a recovery email in case you forget your PIN.",
                    "🔒 **Never share your 6-digit WhatsApp verification code!** Scammers pretend to be friends who 'accidentally' sent you a code. They use it to take over your account!",
                    "⚠️ **WhatsApp Scam Alert:** If a contact asks for money on WhatsApp, call them first to verify! Voice verification prevents account takeover scams."
                },

                ["wifi"] = new List<string>
                {
                    "📶 Never do banking or shopping on public Wi-Fi without a VPN! Hackers can see your data.",
                    "🏠 Change your home router's default password! Many routers come with 'admin/admin'!",
                    "⚠️ Use a VPN like ProtonVPN (free) or NordVPN to encrypt your internet traffic."
                },

                ["sascams"] = new List<string>
                {
                    "🇿🇦 Beware of fake SASSA messages! SASSA never asks for PIN or password via SMS.",
                    "⚠️ Common SA scams: 'You've won a prize', fake courier delivery, banking OTP scams.",
                    "📞 Report scams to SAPS (10111) or SABRIC (0860 557 557)!"
                },

                ["privacy"] = new List<string>
                {
                    "🔒 Review app permissions! Does your flashlight app need access to your contacts?",
                    "🛡️ Use a VPN and clear browser cookies regularly to protect your privacy online.",
                    "📧 Use email aliases for online shopping to protect your real email address."
                },

                ["socialmedia"] = new List<string>
                {
                    "📱 **Set social media to PRIVATE!** Don't share your location or travel plans online.",
                    "🤳 **Think before you post!** Birthday posts help hackers guess your security questions.",
                    "🔒 **Review privacy settings monthly** - social media changes policies often!"
                },

                ["ransomware"] = new List<string>
                {
                    "💰 NEVER pay the ransom! It encourages more attacks!",
                    "💾 3-2-1 backup rule: 3 copies, 2 media types, 1 offsite backup!",
                    "🛡️ Keep antivirus updated and be careful with email attachments!"
                },

                ["shopping"] = new List<string>
                {
                    "🛒 Only shop on HTTPS websites! Look for the padlock icon in your browser!",
                    "💳 Use credit cards instead of debit cards for better fraud protection!",
                    "📦 Track packages and beware of fake delivery SMS asking for fees!"
                }
            };

            followUpResponses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "🔐 **Another password tip:** Change passwords every 3-6 months, never use obvious variations!",
                    "💡 **Combine for maximum security:** Use a strong password + 2FA together!",
                    "🔐 **Password managers:** They can generate unbreakable random passwords for you!"
                },
                ["phishing"] = new List<string>
                {
                    "🎣 **Check the sender's domain:** 'support@paypa1.com' is fake (number 1 instead of letter l)!",
                    "🚨 **Scammers create urgency:** 'Your account will be closed in 24 hours!' Verify first.",
                    "🔍 **Unsure about an email?** Type the website address manually - don't click the link!"
                },
                ["2fa"] = new List<string>
                {
                    "🔐 **Setting up 2FA?** Download 'Google Authenticator' from your app store!",
                    "📱 **Most banks offer 2FA** through their mobile apps. Check security settings today!",
                    "✅ **Save backup codes!** Keep them in a safe place to recover your account!"
                },
                ["whatsapp"] = new List<string>
                {
                    "📱 **WhatsApp Two-Step Setup:**\n1. Open WhatsApp\n2. Go to Settings\n3. Tap Account\n4. Tap Two-step verification\n5. Tap Enable\n6. Create a 6-digit PIN\n7. Add recovery email",
                    "🔒 **WhatsApp Security Tip:** Enable fingerprint lock in WhatsApp Settings → Privacy → Fingerprint lock!",
                    "⚠️ **WhatsApp Recovery Email:** Always add a recovery email to reset your PIN if you forget it!"
                },
                ["wifi"] = new List<string>
                {
                    "📶 **Free VPN options:** ProtonVPN (free tier), Windscribe (10GB free/month)",
                    "🏠 **Home Wi-Fi security:** Disable WPS on your router - it's a security vulnerability!",
                    "⚠️ **Public Wi-Fi danger:** Hackers can create fake Wi-Fi hotspots called 'Evil Twins'!"
                },
                ["sascams"] = new List<string>
                {
                    "🇿🇦 **SASSA Scam Alert:** SASSA will NEVER ask for your PIN or password!",
                    "📞 **Report scams to:** SAPS Crime Stop: 08600 10111 or SABRIC: 0860 557 557",
                    "⚠️ **Fake delivery SMS:** Legitimate couriers don't ask for 'redelivery fees' via SMS!"
                },
                ["privacy"] = new List<string>
                {
                    "🔒 **Browser privacy:** Use DuckDuckGo and install Privacy Badger extension!",
                    "🛡️ **Email aliases:** Services like SimpleLogin protect your real email address!",
                    "📱 **Phone privacy:** Remove permissions from apps that don't need them!"
                },
                ["socialmedia"] = new List<string>
                {
                    "📱 **Facebook privacy:** Go to Settings → Privacy → 'Limit past posts'!",
                    "🤳 **Instagram safety:** Turn off 'Show activity status' in privacy settings!",
                    "🔒 **LinkedIn tip:** Change profile settings to show only to connections!"
                }
            };
        }

        private void InitializeSentimentKeywords()
        {
            sentimentKeywords = new Dictionary<string, string[]>
            {
                ["worried"] = new string[] { "worried", "scared", "nervous", "anxious", "concerned", "afraid", "fear" },
                ["frustrated"] = new string[] { "frustrated", "annoying", "difficult", "complicated", "hard", "confusing", "angry" },
                ["curious"] = new string[] { "curious", "interested", "want to learn", "tell me", "explain", "how to" },
                ["thankful"] = new string[] { "thank", "thanks", "helpful", "appreciate", "grateful" }
            };
        }

        public void ProcessUserInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                OnResponseGenerated?.Invoke("I didn't catch that. Could you please type something?");
                return;
            }

            string lowerInput = input.ToLower();

            conversationHistory.Add(input);
            if (conversationHistory.Count > 20) conversationHistory.RemoveAt(0);

            string detectedSentiment = DetectSentiment(lowerInput);

            // Check for random tip request
            if (lowerInput.Contains("random tip") || lowerInput.Contains("give me a tip") || lowerInput.Contains("tip please"))
            {
                string randomTip = randomTips[random.Next(randomTips.Length)];
                OnResponseGenerated?.Invoke(randomTip);
                return;
            }

            // Check for exit
            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "bye" || lowerInput == "goodbye")
            {
                OnResponseGenerated?.Invoke(GetGoodbyeMessage());
                return;
            }

            // Check for menu
            if (lowerInput == "menu" || lowerInput == "help" || lowerInput == "topics" || lowerInput == "what can you do")
            {
                ShowMenu();
                return;
            }

            // ============================================
            // FIXED: Check for follow-up FIRST before anything else
            // ============================================
            if (lowerInput.Contains("tell me more") || lowerInput.Contains("explain more") ||
                lowerInput.Contains("another tip") || lowerInput.Contains("more details") ||
                lowerInput == "tell more" || lowerInput == "more tips" || lowerInput == "more info")
            {
                if (!string.IsNullOrEmpty(currentTopic) && followUpResponses.ContainsKey(currentTopic))
                {
                    string followUp = followUpResponses[currentTopic][random.Next(followUpResponses[currentTopic].Count)];
                    OnResponseGenerated?.Invoke(followUp);
                    return;
                }
                else if (!string.IsNullOrEmpty(currentTopic))
                {
                    OnResponseGenerated?.Invoke($"I have more information about {currentTopic}! {GetRandomResponse(currentTopic)}\n\n💡 Click the {currentTopic} button again for more tips!");
                    return;
                }
                else
                {
                    OnResponseGenerated?.Invoke("I'm not sure which topic you want more about. Try clicking a button above first (Passwords, WhatsApp, or Privacy) then ask 'tell me more'!");
                    return;
                }
            }

            // Check for name if not set
            if (string.IsNullOrEmpty(userName) && !lowerInput.Contains("?") && lowerInput.Length < 25 &&
                !lowerInput.Contains("password") && !lowerInput.Contains("phish") && !lowerInput.Contains("whatsapp"))
            {
                ProcessNameInput(input);
                return;
            }

            // Get response based on keywords - this will update currentTopic
            string response = GetKeywordResponse(lowerInput, detectedSentiment);

            if (!string.IsNullOrEmpty(userName))
            {
                response = response.Replace("{0}", userName);
            }

            OnResponseGenerated?.Invoke(response);
        }

        private string DetectSentiment(string input)
        {
            if (sentimentKeywords["worried"].Any(k => input.Contains(k)))
            {
                currentSentiment = "worried";
                return "worried";
            }
            if (sentimentKeywords["frustrated"].Any(k => input.Contains(k)))
            {
                currentSentiment = "frustrated";
                return "frustrated";
            }
            if (sentimentKeywords["curious"].Any(k => input.Contains(k)))
            {
                currentSentiment = "curious";
                return "curious";
            }
            if (sentimentKeywords["thankful"].Any(k => input.Contains(k)))
            {
                currentSentiment = "thankful";
                OnResponseGenerated?.Invoke("You're welcome! I'm glad I could help you stay safe online! 😊");
                return "thankful";
            }
            currentSentiment = "neutral";
            return "neutral";
        }

        private string GetKeywordResponse(string input, string sentiment)
        {
            string baseResponse = "";

            // Check for WiFi
            if (input.Contains("wifi") || input.Contains("wi-fi") || input.Contains("wireless") ||
                input.Contains("public wifi") || input.Contains("vpn") || input.Contains("hotspot"))
            {
                currentTopic = "wifi";
                baseResponse = GetRandomResponse("wifi");
            }
            // Check for WhatsApp
            else if (input.Contains("whatsapp") || input.Contains("wa") ||
                input.Contains("two-step") || input.Contains("two step") ||
                input.Contains("verification") || input.Contains("two step verification") ||
                (input.Contains("enable") && input.Contains("whatsapp")) ||
                (input.Contains("protect") && input.Contains("whatsapp")))
            {
                currentTopic = "whatsapp";
                baseResponse = GetRandomResponse("whatsapp");
            }
            // Check for Privacy
            else if (input.Contains("privacy") || input.Contains("private") || input.Contains("data protection") || input.Contains("personal data"))
            {
                currentTopic = "privacy";
                baseResponse = GetRandomResponse("privacy");
            }
            // Check for Passwords
            else if (input.Contains("password") || input.Contains("pass") || input.Contains("login") || input.Contains("strong password"))
            {
                currentTopic = "password";
                baseResponse = GetRandomResponse("password");
            }
            // Check for Phishing
            else if (input.Contains("phish") || input.Contains("scam") || input.Contains("fraud") || input.Contains("fake email") || input.Contains("phishing"))
            {
                currentTopic = "phishing";
                baseResponse = GetRandomResponse("phishing");
            }
            // Check for 2FA
            else if (input.Contains("2fa") || input.Contains("two factor") || input.Contains("authenticator") || input.Contains("two-factor") || input.Contains("multi factor"))
            {
                currentTopic = "2fa";
                baseResponse = GetRandomResponse("2fa");
            }
            // Check for SA Scams
            else if (input.Contains("sassa") || input.Contains("sa scam") || input.Contains("south african") || input.Contains("sabric") || input.Contains("saps"))
            {
                currentTopic = "sascams";
                baseResponse = GetRandomResponse("sascams");
            }
            // Check for Social Media
            else if (input.Contains("social") || input.Contains("facebook") || input.Contains("instagram") || input.Contains("twitter") || input.Contains("tiktok") || input.Contains("social media"))
            {
                currentTopic = "socialmedia";
                baseResponse = GetRandomResponse("socialmedia");
            }
            // Check for Ransomware
            else if (input.Contains("ransom") || input.Contains("ransomware") || input.Contains("virus") || input.Contains("malware"))
            {
                currentTopic = "ransomware";
                baseResponse = GetRandomResponse("ransomware");
            }
            // Check for Shopping
            else if (input.Contains("shop") || input.Contains("shopping") || input.Contains("buy") || input.Contains("online purchase") || input.Contains("payment"))
            {
                currentTopic = "shopping";
                baseResponse = GetRandomResponse("shopping");
            }
            // Check for Greeting
            else if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey") || input.Contains("greeting") || input.Contains("good morning"))
            {
                baseResponse = GetRandomResponse("greeting");
            }
            else
            {
                // Default responses
                string[] defaultResponses = {
                    "I'm not sure what you're asking. Try clicking one of the buttons above - Passwords, WhatsApp, Privacy, Wi-Fi, or 2FA!",
                    "Not sure what you mean. Click the Passwords, WhatsApp, Wi-Fi, Privacy, or 2FA button above!",
                    "I didn't quite catch that. Click any button above to get cybersecurity tips!"
                };
                return defaultResponses[random.Next(defaultResponses.Length)];
            }

            // Add sentiment-based empathy
            if (sentiment == "worried")
            {
                return $"It's understandable to be concerned. Let me help you stay safe. {baseResponse}\n\n💡 Ask 'tell me more' for additional tips!";
            }
            else if (sentiment == "frustrated")
            {
                return $"Cybersecurity can seem complicated, but I'll make it simple. {baseResponse}\n\n💡 Ask 'tell me more' if you need more help!";
            }
            else if (sentiment == "curious")
            {
                return $"Great question! I love your curiosity. {baseResponse}\n\n💡 Ask 'tell me more' for additional tips!";
            }

            return baseResponse + "\n\n💡 Ask 'tell me more' for additional tips!";
        }

        private void ProcessNameInput(string input)
        {
            string[] words = input.Trim().Split(' ');
            string extractedName = words[0];
            extractedName = Regex.Replace(extractedName, @"[^a-zA-Z]", "");

            if (!string.IsNullOrEmpty(extractedName) && extractedName.Length >= 2)
            {
                userName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(extractedName.ToLower());
                userMemory["name"] = userName;

                string response = string.Format(GetRandomResponse("name_response"), userName);
                OnResponseGenerated?.Invoke(response);
            }
        }

        private void ShowMenu()
        {
            string menu = @"📋 **AVAILABLE CYBERSECURITY TOPICS** 📋

🔐 PASSWORDS - Click the Passwords button above
🎣 PHISHING - Click the Phishing button above
🔑 TWO-FACTOR AUTH - Click the 2FA button above
📱 WHATSAPP - Click the WhatsApp button above
📶 WI-FI SECURITY - Click the Wi-Fi button above
🇿🇦 SA SCAMS - Click the ZA SA Scams button above
🔒 PRIVACY - Click the Privacy button above
📱 SOCIAL MEDIA - Ask about 'social media'
💰 RANSOMWARE - Ask about 'ransomware'
🛒 SHOPPING - Ask about 'shopping'

💡 **TIP:** After clicking any button, ask 'tell me more' for additional information!";

            OnResponseGenerated?.Invoke(menu);
        }

        private string GetRandomResponse(string key)
        {
            if (responses.ContainsKey(key) && responses[key].Count > 0)
            {
                return responses[key][random.Next(responses[key].Count)];
            }
            return "I don't have info on that yet. Try clicking one of the buttons above!";
        }

        private string GetGoodbyeMessage()
        {
            string[] goodbyes = {
                $"Stay safe online{(!string.IsNullOrEmpty(userName) ? $", {userName}" : "")}! Click the buttons anytime to learn more about cybersecurity! 🇿🇦",
                "Thanks for learning! Remember to use strong passwords and enable 2FA. Click any button to continue learning! 🛡️",
                "Goodbye! Stay safe online and come back anytime to learn more cybersecurity tips! 👋"
            };
            return goodbyes[random.Next(goodbyes.Length)];
        }
    }
}