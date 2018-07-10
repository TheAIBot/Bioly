using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using System.Linq;
using BiolyCompiler.Graphs;
using System.Threading;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;
        public readonly string OutputVariable;
        public string OriginalOutputVariable { get; protected set; }
        public readonly string BlockID;
        private static int nameID;

        public readonly IReadOnlyList<FluidInput> InputFluids;
        private static readonly List<FluidInput> EmptyFluidList = new List<FluidInput>();
        public readonly IReadOnlyList<string> InputNumbers;
        private static readonly List<string> EmptyNumberList = new List<string>();

        //first symbol is important because it makes it an invalid name to parse
        //but that's okay here because this is after the parser step
        public const string DEFAULT_NAME = "@anonymous var";
        public const string TYPE_FIELD_NAME = "type";
        public const string ID_FIELD_NAME = "id";

        //For the scheduling:
        public bool IsDone = false;
        public int priority = Int32.MaxValue;
        public int StartTime = -1;
        public int EndTime = -1;

        public Block(bool canBeOutput, List<FluidInput> inputFluids, List<string> inputNumbers, string output, string blockID)
        {
            this.CanBeOutput = canBeOutput;
            this.InputFluids = inputFluids ?? EmptyFluidList;

            inputNumbers = inputNumbers ?? EmptyNumberList;
            inputFluids?.Where(x => x != null).ToList().ForEach(x => inputNumbers.AddRange(x?.InputNumbers));
            this.InputNumbers = inputNumbers ?? EmptyNumberList;
            this.OutputVariable = $"N{Interlocked.Increment(ref nameID)}";
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

        public abstract override string ToString();
    }
}
