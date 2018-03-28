﻿using System;
using theta_bot.Generators;
using Telegram.Bot.Types;

namespace theta_bot.Levels
{
    public class Level1 : ILevel
    {
        public bool IsFinished(Contact person) => false;
        private readonly SimpleCodeGenerator simple = new SimpleCodeGenerator();
        private readonly SimpleLoopGenerator loop = new SimpleLoopGenerator();
        
        public Exercise Generate(Random random)
        {
            return new Exercise()
                .Generate(simple)
                .Generate(loop);
        }
    }
}