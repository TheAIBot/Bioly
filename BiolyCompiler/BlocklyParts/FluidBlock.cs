using BiolyCompiler.BlocklyParts.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class FluidBlock : Block
    {
        public readonly IReadOnlyList<FluidAsInput> InputVariables;

        private static readonly List<FluidAsInput> EmptyList = new List<FluidAsInput>();

        public FluidBlock(bool canBeOutput, string output) : base(canBeOutput, output)
        {
            InputVariables = EmptyList;
        }

        public FluidBlock(bool canBeOutput, List<FluidAsInput> input, string output) : base(canBeOutput, output)
        {
            InputVariables = input;
        }
    }
}
