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
        public readonly string BlockID;
        private static int nameID;
        //first symbol is important because it makes it an invalid name to parse
        //but that's okay here because this is after the parser step
        public const string DEFAULT_NAME = "@anonymous var";
        public const string TypeFieldName = "type";
        public const string IDFieldName = "id";

        //For the scheduling:
        public bool hasBeenScheduled = false;
        public int StartTime = -1;
        public int endTime = -1;
        public int priority = Int32.MaxValue;

        public Block(bool canBeOutput, string output, string blockID)
        {
            this.CanBeOutput = canBeOutput;
            this.OutputVariable = $"N{nameID}";
            this.BlockID = blockID;
            nameID++;
            this.OriginalOutputVariable = output ?? DEFAULT_NAME;
        }

        protected abstract void ResetBlock();

        public void Reset()
        {
            ResetBlock();
            this.hasBeenScheduled = false;
            this.StartTime = -1;
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
