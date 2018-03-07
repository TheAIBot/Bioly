using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;
        public readonly IReadOnlyList<string> InputVariables;
        public readonly string OutputVariable;
        public readonly string OriginalOutputVariable;
        private static int nameID;
        public const string DEFAULT_NAME = "anonymous var";

        private static readonly List<string> EmptyList = new List<string>();

        public Block(bool canBeOutput, string output) : this(canBeOutput, EmptyList, output)
        {
        }

        public Block(bool canBeOutput, List<string> input, string output)
        {
            this.CanBeOutput = canBeOutput;
            this.InputVariables = input;
            this.OutputVariable = $"N{nameID}";
            nameID++;
            this.OriginalOutputVariable = output ?? DEFAULT_NAME;
        }
    }
}
