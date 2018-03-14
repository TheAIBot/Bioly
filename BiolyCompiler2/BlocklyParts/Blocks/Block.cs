using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Blocks
{
    public abstract class Block : IBlocklyPart
    {
        public readonly bool CanBeOutput;
        public readonly IReadOnlyList<string> InputVariables;
        public readonly string OutputVariable;
        //For the scheduling:
        public Module boundedModule;
        public bool hasBeenScheduled = false;
        public int estimatedLongestPath = Int32.MaxValue;
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


        public virtual OperationType getOperationType(){
            return getOperationType.Unknown;
        }

        internal void Bind(Module module)
        {
            throw new NotImplementedException();
        }
    }
}
