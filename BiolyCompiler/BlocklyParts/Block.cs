using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;

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
        //For the scheduling:
        public Module boundModule;
        public bool hasBeenScheduled = false;
        public int startTime, endTime;
        public int priority = Int32.MaxValue;
        
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


        public virtual OperationType getOperationType(){
            return OperationType.Unknown;
        }

        public void Bind(Module module)
        {
            boundModule = module;
            module.bindingOperation = this;
        }

        internal void Unbind(Module module)
        {
            throw new NotImplementedException();
        }

        public virtual Module getAssociatedModule()
        {
            throw new NotImplementedException("No modules have been associated with blocks/operations of type " + this.GetType().ToString());
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
