using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;
        public readonly string OutputVariable;
        public string OriginalOutputVariable { get; protected set; }
        public readonly string BlockID;
        private static int nameID;
        //first symbol is important because it makes it an invalid name to parse
        //but that's okay here because this is after the parser step
        public const string DEFAULT_NAME = "@anonymous var";
        public const string TypeFieldName = "type";
        public const string IDFieldName = "id";

        //For the scheduling:
        public bool IsDone = false;
        public int StartTime = -1;
        public int EndTime = -1;
        public int priority = Int32.MaxValue;

        public Block(bool canBeOutput, string output, string blockID)
        {
            this.CanBeOutput = canBeOutput;
            this.OutputVariable = $"N{nameID}";
            nameID++;
            this.BlockID = blockID;
            this.OriginalOutputVariable = output ?? DEFAULT_NAME;
        }

        protected abstract void ResetBlock();

        public void Reset()
        {
            ResetBlock();
            this.IsDone = false;
            this.StartTime = -1;
            this.EndTime = -1;
            this.priority = Int32.MaxValue;
    }

        public virtual void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {

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

        public abstract string ToString();
    }
}
