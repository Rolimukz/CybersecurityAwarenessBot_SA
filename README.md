# Cybersecurity Awareness Bot - South Africa

## What I Built

I created a desktop chatbot that helps South Africans learn about staying safe online. The bot talks to you, remembers your name, understands how you're feeling, and answers questions about things like passwords, WhatsApp security, and spotting scams.

## What It Can Do

- Plays my recorded voice when the app starts
- Has a chat window where I type and the bot replies in different coloured bubbles
- Has buttons at the top for topics like Passwords, WhatsApp, 2FA, Wi-Fi, SA Scams, and Privacy
- Remembers my name once I tell it
- If I type "tell me more", it gives extra tips about whatever we were just talking about
- If I say I'm worried or confused, it responds in a kinder way
- I can type "random tip" and it gives me a cybersecurity fact
- I can clear the chat or click Exit to close

## How I Made It

I used C# with WPF for the user interface. There are three main files:

- MainWindow.xaml - This is where I designed how the app looks (colours, buttons, chat bubbles)
- MainWindow.xaml.cs - This handles what happens when you click buttons or type messages
- ChatbotEngine.cs - This is the brain. It stores all the answers, recognises keywords, and remembers your name

## How to Run It

You need Visual Studio 2022 and .NET 8.0.

1. Open the project in Visual Studio
2. Put your voice file (Recording005.wav) in the project folder if you want the greeting to play
3. Press F5 to run

## How to Use It

When the app starts, it will ask for your name. Just type it and press Enter.

Then you can:
- Click any of the topic buttons to learn about that subject
- Type "tell me more" after any answer to get more information
- Type "random tip" for a quick cybersecurity fact
- Type "menu" to see all topics
- Click Clear to erase the chat history
- Click Exit or type "exit" to close

## A Quick Example

Bot asks for my name
I type: Thabo
Bot says: Nice to meet you, Thabo!

I click the WhatsApp button
Bot tells me about two-step verification

I type: tell me more
Bot gives me step by step setup instructions

I type: I'm worried about scams
Bot says it understands my concern and gives me tips about fake SASSA messages

I type: exit
Bot says goodbye and closes

## File Structure
