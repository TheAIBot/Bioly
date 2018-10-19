using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.FluidBlock
    {
        public readonly Module associatedModule;

        public TestBlock(List<FluidInput> inputs, string output, Module associatedModule) : base(true, inputs, null, output, String.Empty)
        {
            this.associatedModule = associatedModule;
        }

        public TestBlock(List<FluidBlock> inputs, string output, Module associatedModule) : this(inputs.Select(input => (FluidInput)new BasicInput("", input.OutputVariable, 1, true)).ToList(), output, associatedModule)
        {

        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            throw new NotImplementedException();
        }

        public override Module getAssociatedModule()
        {
            return associatedModule;
        }

        public override string ToString()
        {
            return "Test block";
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            throw new NotImplementedException();
        }
    }
}