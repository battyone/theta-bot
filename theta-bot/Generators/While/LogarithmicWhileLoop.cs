﻿using System;
using System.Collections.Generic;
using System.Text;

namespace theta_bot
{
    public class LograrithmicWhileLoop : ICycleGenerator
    {
        private readonly Dictionary<Complexity, Complexity> complexities = 
            new Dictionary<Complexity, Complexity>
                {{Complexity.Constant, Complexity.LogN}};
        
        private readonly string[] templates =
        {
            "var {0} = {1};\nwhile({0} < {2})\n{{\n    {0}*={3};\n",
            "var {0} = {1};\nwhile({0} < {2})\n{{\n    {0}={0}*{3};\n",
            "var {0} = {1};\nwhile({0} < {2} * {2})\n{{\n    {0}*={3};\n",
            "var {0} = {2};\nwhile({0} > {1})\n{{\n    {0}/={3};\n",
            "var {0} = {2};\nwhile({0} > {1})\n{{\n    {0}={0}/{3};\n",
            "var {0} = {2} * {2};\nwhile({0} > {1})\n{{\n    {0}/={3};\n",
        };
        
        public void ChangeCode(StringBuilder code, Func<Variable> getNextVar, Random random) => 
            AddCycle("n", code, getNextVar, random);

        public void AddCycle(string cycleVar, StringBuilder code, Func<Variable> getNextVar, Random random)
        {
            var variable = getNextVar();
            var startValue = random.Next(1, 3);
            var stepValue = random.Next(1, 5);
            var template = templates[random.Next(templates.Length)];
            var newCode = string.Format(template, variable.Label, startValue, cycleVar, stepValue);
            
            code.ShiftLines(4);
            code.Insert(0, newCode);
            code.Append("}\n");

            variable.IsBounded = true;
        }

        public bool TryGetComplexity(Complexity oldComplexity, out Complexity newComplexity)
        {
            newComplexity = complexities.ContainsKey(oldComplexity)
                ? complexities[oldComplexity]
                : oldComplexity;
            return complexities.ContainsKey(oldComplexity);
        }
    }
}