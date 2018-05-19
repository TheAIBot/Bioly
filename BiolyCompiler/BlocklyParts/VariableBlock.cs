using BiolyCompiler.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class VariableBlock : Block
    {
        public readonly IReadOnlyList<string> InputVariables;
        public readonly bool CanBeScheduled;

        private static readonly List<string> EmptyList = new List<string>();

        public VariableBlock(bool canBeOutput, string output, string id, bool canBeScheduled) : base(canBeOutput, output, id)
        {
            InputVariables = EmptyList;
            CanBeScheduled = canBeScheduled;
        }

        public VariableBlock(bool canBeOutput, List<string> input, string output, string id, bool canBeScheduled) : base(canBeOutput, output, id)
        {
            InputVariables = input;
            CanBeScheduled = canBeScheduled;
        }

        protected override void ResetBlock()
        {
        }

        public abstract float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor);
        
        public virtual (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            return (OriginalOutputVariable, Run(variables, executor));
        }
    }
}
