﻿using Telegram.Bot;

namespace theta_bot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new ThetaBot(
                new TelegramBotClient(args[0]),
                new FirebaseProvider(args[1], args[2]), 
                new Level0(),
                new Level1(),
                new Level2() 
                );
        }
    }
}