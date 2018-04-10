using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class VariableBlock : Block
    {
        public readonly IReadOnlyList<string> InputVariables;

        private static readonly List<string> EmptyList = new List<string>();

        public VariableBlock(bool canBeOutput, string output) : base(canBeOutput, output)
        {
            InputVariables = EmptyList;
        }

        public VariableBlock(bool canBeOutput, List<string> input, string output) : base(canBeOutput, output)
        {
            InputVariables = input;
        }

        protected override void ResetBlock()
        {
        }

        public abstract float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor);
        
        public virtual (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            return (OutputVariable, Run(variables, executor));
        }
    }
}
