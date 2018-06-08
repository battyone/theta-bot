﻿using System.Collections.Generic;
using theta_bot.Classes;

namespace theta_bot.Database
{
    public interface IDataProvider
    {
        string AddTask(long chatId, int level, IExercise exercise);
        string GetAnswer(string taskKey);
        void SetSolved(long chatId, string taskKey, bool solved);
        IEnumerable<bool?> GetLastStats(long chatId, int count);
        void SetLevel(long chatId, int level);
        int? GetLevel(long chatId);
    }
}