using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class VariableBlock : Block
    {
        public readonly bool CanBeScheduled;

        public VariableBlock(bool canBeOutput, List<FluidInput> inputFluids, List<string> inputNumbers, string output, string id, bool canBeScheduled) : base(canBeOutput, inputFluids, inputNumbers, output, id)
        {
            CanBeScheduled = canBeScheduled;
        }

        protected override void ResetBlock()
        {
        }

        public abstract float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions);
        
        public virtual (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return (OutputVariable, Run(variables, executor, dropPositions));
        }

        public abstract string ToXml();

        public abstract List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks);

        public override List<Block> GetBlockTreeList(List<Block> blocks)
        {
            blocks.AddRange(GetVariableTreeList(new List<VariableBlock>()));

            return blocks;
        }
    }
}
