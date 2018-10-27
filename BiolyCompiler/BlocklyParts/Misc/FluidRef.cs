using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class FluidRef : FluidBlock
    {
        public FluidRef(string newName, string oldName) : 
            base(false, new List<FluidInput>() { new BasicInput(string.Empty, oldName, 1, false) }, null, newName, string.Empty)
        {
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new FluidRef(OutputVariable, InputFluids.First().OriginalFluidName);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            if (!renamer.ContainsKey(OutputVariable))
            {
                renamer.Add(OutputVariable, OutputVariable + namePostfix);
            }

            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, renamer, namePostfix)));

            renamer[OutputVariable] = OutputVariable + namePostfix;
            return new Fluid(inputFluids, OutputVariable + namePostfix, BlockID);
        }

        public override List<Block> GetBlockTreeList(List<Block> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public override string ToString()
        {
            return $"{OutputVariable} = ref {InputFluids.First().OriginalFluidName}";
        }
    }
}
