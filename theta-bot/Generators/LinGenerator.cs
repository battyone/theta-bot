﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using theta_bot.Classes;
using theta_bot.Extentions;

namespace theta_bot.Generators
{
    public class LinGenerator : Generator
    {   
        private static readonly string[] ForTemplates =
        {
            "for (var {0}={1}; {0}<{2}; {0}+={3})\n{{\n",
            "for (var {0}={1}; {0}<{2}; {0}={0}+{3})\n{{\n",
            "for (var {0}={2}; {0}>{1}; {0}-={3})\n{{\n",
            "for (var {0}={2}; {0}>{1}; {0}={0}-{3})\n{{\n",
        };

        private static readonly string[] WhileTemplates =
        {
            "var {0} = {1};\nwhile ({0} < {2})\n{{\n    {0} += {3};\n",
            "var {0} = {1};\nwhile ({0} < {2})\n{{\n    {0} = {0} + {3};\n",
            "var {0} = {2};\nwhile ({0} < {1})\n{{\n    {0} -= {3};\n",
            "var {0} = {2};\nwhile ({0} > {1})\n{{\n    {0} = {0} - {3};\n",
        };

        private static readonly Dictionary<Tag, string[]> Templates = 
            new Dictionary<Tag, string[]>
            {
                {Tag.For, ForTemplates},
                {Tag.While, WhileTemplates}
            };
        
        public override IExercise Generate(IExercise exercise, Random random, params Tag[] tags)
        {
            var complexity = GetComplexity(exercise, tags);
            if (complexity == null) return exercise;

            var codeType = CodeType(tags);
            var newVar = new Variable(true);
            
            var start = new Variable(random.Next(1, 3));
            var end = exercise.MainVar;
            var step = new Variable(random.Next(2, 5));
            var template = Templates[codeType].Random();

            var newCode = string.Format(template, newVar, start, end, step);
            var newTags = new List<Tag> {codeType};
            var newVars = new List<Variable> {newVar, start, end, step};

            if (tags.Contains(Tag.DependFromValue))
            {
                exercise = exercise.ReplaceVar(exercise.Vars[2], newVar);
                newTags.Add(Tag.DependFromValue);
            }

            if (tags.Contains(Tag.DependFromStep))
            {
                exercise = exercise.ReplaceVar(exercise.Vars[3], newVar);
                newTags.Add(Tag.DependFromStep);
            }
            
            return new IExercise(newVars, newCode, newTags, (Complexity)complexity, exercise);
        }

        private static Complexity? GetComplexity(IExercise previous, Tag[] tags)
        {
            if (!Depend(tags))
                return Complexity.Linear;
            if (previous.Complexity == Complexity.Const)
                return Complexity.Linear;
            if (previous.Complexity == Complexity.Log)
                return tags.Contains(Tag.DependFromValue)
                    ? new Complexity(1, 1)
                    : Complexity.Linear;
            if (previous.Complexity == Complexity.Linear)
                return tags.Contains(Tag.DependFromValue)
                    ? new Complexity(2, 0)
                    : new Complexity(1, 1);
            return null;
        }
    }
    
    [TestFixture]
    public class LinForShould
    {
        [Test]
        public void Test()
        {
            var a = new IExercise()
                .Generate(new ConstGenerator(), Tag.Code)
                .Generate(new LinGenerator(), Tag.For)
                .Generate(new LinGenerator(), Tag.While)
                .Build();
            Console.WriteLine(a);
            Console.WriteLine(a.Complexity);
        }
        
        [Test]
        public void Test2()
        {
            var a = new IExercise()
                .Generate(new ConstGenerator(), Tag.Code)
                .Generate(new LinGenerator(), Tag.For)
                .Generate(new LinGenerator(), Tag.For, Tag.DependFromValue)
                .Build();
            Console.WriteLine(a);
            Console.WriteLine(a.Complexity);
        }
        
        [Test]
        public void Test3()
        {
            var a = new IExercise()
                .Generate(new ConstGenerator(), Tag.Code)
                .Generate(new LinGenerator(), Tag.For)
                .Generate(new LinGenerator(), Tag.For, Tag.DependFromStep)
                .Build();
            Console.WriteLine(a);
            Console.WriteLine(a.Complexity);
        }
    }
}