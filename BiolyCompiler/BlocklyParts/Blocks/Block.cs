﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks
{
    public abstract class Block : IBlocklyPart
    {
        public readonly bool CanBeOutput;
        public readonly IReadOnlyList<string> InputVariables;
        public readonly string OutputVariable;

        private static readonly List<string> EmptyList = new List<string>();

        public Block(bool canBeOutput, string output) : this(canBeOutput, EmptyList, output)
        {
        }

        public Block(bool canBeOutput, List<string> input, string output)
        {
            this.CanBeOutput = canBeOutput;
            this.InputVariables = input;
            this.OutputVariable = output;
        }
    }
}
