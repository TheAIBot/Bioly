using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class VariableBlock : Block
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
    }
}
