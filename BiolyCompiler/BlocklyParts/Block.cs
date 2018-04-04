using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;
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
        public int startTime;
        public int endTime;
        public int priority = Int32.MaxValue;

        public Block(bool canBeOutput, string output)
        {
            this.CanBeOutput = canBeOutput;
            this.OutputVariable = $"N{nameID}";
            nameID++;
            this.OriginalOutputVariable = output ?? DEFAULT_NAME;
        }
        
        public override int GetHashCode()
        {
            return OutputVariable.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Block blockObj = obj as Block;
            if (blockObj == null) return false;
            else if (blockObj.GetType() != this.GetType()) return false;
            else return (OutputVariable.Equals(blockObj.OutputVariable));
        }
    }
}
