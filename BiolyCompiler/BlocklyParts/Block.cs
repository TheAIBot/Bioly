using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;
        public readonly string OutputVariable;
        public readonly string OriginalOutputVariable;
        private static int nameID;
        public const string DEFAULT_NAME = "anonymous var";

        //For the scheduling:
        public bool hasBeenScheduled = false;
        public int startTime = -1;
        public int endTime = -1;
        public int priority = Int32.MaxValue;

        public Block(bool canBeOutput, string output)
        {
            this.CanBeOutput = canBeOutput;
            this.OutputVariable = $"N{nameID}";
            nameID++;
            this.OriginalOutputVariable = output ?? DEFAULT_NAME;
        }

        protected abstract void ResetBlock();

        public void Reset()
        {
            ResetBlock();
            this.hasBeenScheduled = false;
            this.startTime = -1;
            this.endTime = -1;
            this.priority = Int32.MaxValue;
    }

        public override int GetHashCode()
        {
            return OutputVariable.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Block blockObj)
            {
                return blockObj.GetType() == this.GetType() && blockObj.OutputVariable == this.OutputVariable;
            }
            return false;
        }
    }
}
