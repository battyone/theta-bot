﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineKeyboardButtons;

namespace theta_bot
{
    public class ThetaBot
    {
        private readonly Dictionary<string, Action<long>> commands;

        private readonly Dictionary<string, string> answersCache = new Dictionary<string, string>();
        private readonly Dictionary<string, int> levelCache = new Dictionary<string, int>();
        
        private readonly Random random = new Random();
        private readonly TelegramBotClient bot;
        private readonly IDataProvider database;
        private readonly ILevel[] levels;

        public ThetaBot(TelegramBotClient bot, IDataProvider database, params ILevel[] levels)
        {
            this.bot = bot;
            this.database = database;
            this.levels = levels;
            commands = new Dictionary<string, Action<long>>
            {
                {
                    "/start", userId => SendMessage(userId,
                        "Hi! Press the button to get a task")
                },
                {"Give a task", SendNewTask},
                {
                    "Level up", userId =>
                    {
                        if (CanIncreaseLevel(userId))
                        {
                            IncreaseLevel(userId);
                            SendMessage(userId, "Level-up");
                        }
                        else SendMessage(userId, "You can't level-up yet");
                    }
                }
            };

            bot.OnMessage += MessageHandler;
            bot.OnCallbackQuery += AnswerHandler;

            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }

        #region Handlers

        private async void AnswerHandler(object sender, CallbackQueryEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                var message = e.CallbackQuery.Message;
                var userId = message.Chat.Id;
                var button = JsonConvert
                    .DeserializeObject<ButtonInfo>(e.CallbackQuery.Data);
                bool correct = button.Answer == GetAnswer(button.TaskKey);

                database.SetSolved(userId, button.TaskKey, correct);

                if (CanIncreaseLevel(userId))
                    SendMessage(userId,
                        "Good job! You can now raise the difficulty, if you want");
                CheckMessageSolved(message, correct, button.Answer);
            });
        }

        private async void MessageHandler(object sender, MessageEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                var message = e.Message.Text;
                var userId = e.Message.Chat.Id;
                if (commands.ContainsKey(message))
                    commands[message](userId);
                else
                    SendMessage(userId, "Sorry, I didn't catch that");
            });
        }

        #endregion

        private void IncreaseLevel(long userId)
        {
            var level = database.GetLevel(userId) ?? 0;
            database.SetLevel(userId, level + 1);
            var key = database.AddTask(userId, -1, new Exercise());
            database.SetSolved(userId, key, false);
        }

        private InlineKeyboardMarkup GetReplyMarkup(Exercise exercise, string taskKey)
        {
            var buttons = exercise
                .GetOptions(random, 4)
                .Select(option =>
                    new InlineKeyboardCallbackButton(
                        option,
                        JsonConvert.SerializeObject(
                            new ButtonInfo(option, taskKey))))
                .ToArray<InlineKeyboardButton>();
            return new InlineKeyboardMarkup(new[]
                {
                    new[] {buttons[0], buttons[1]},
                    new[] {buttons[2], buttons[3]}
                }
            );
        }

        private bool CanIncreaseLevel(long userId)
        {
            var level = database.GetLevel(userId) ?? 0;
            return level + 1 < levels.Length &&
                   levels[level].IsFinished(database, userId);
        }

        #region SendOrEdit

        private void CheckMessageSolved(Message message, bool correct, string answer)
        {
            var builder = new StringBuilder(message.Text);
            builder.Insert(0, "```\n");
            builder.Append("```\n\n");
            builder.Append(answer);
            builder.Append(" - ");
            builder.Append(correct ? "Correct" : "Wrong answer");
            bot.EditMessageTextAsync(
                message.Chat.Id,
                message.MessageId,
                builder.ToString(),
                ParseMode.Markdown);
        }

        private void SendNewTask(long userId)
        {
            var level = database.GetLevel(userId) ?? 0;
            var exercise = levels[level].Generate(random);
            var taskKey = database.AddTask(userId, level, exercise);
            answersCache[taskKey] = exercise.Complexity.Value;
            bot.SendTextMessageAsync(
                userId,
                $"```\nFind the complexity of the algorithm:\n\n{exercise.Message}\n```",
                ParseMode.Markdown,
                replyMarkup: GetReplyMarkup(exercise, taskKey));
        }

        private void SendMessage(long userId, string message)
        {
            var buttons = new List<string> {"Give a task"};
            if (CanIncreaseLevel(userId))
                buttons.Add("I want harder");
            bot.SendTextMessageAsync(
                userId,
                message,
                replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        buttons
                            .Select(str => new KeyboardButton(str))
                            .ToArray()
                    },
                    true));
        }

        #endregion
        
        #region Cache

        private string GetAnswer(string key)
        {
            if (answersCache.ContainsKey(key))
            {
                var answer = answersCache[key];
                answersCache.Remove(key);
                return answer;
            }

            return database.GetAnswer(key);
        }

        #endregion
    }
}