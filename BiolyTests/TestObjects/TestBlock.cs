using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.FluidBlock
    {
        public readonly Module associatedModule;

        public TestBlock(List<FluidInput> inputs, string output, Module associatedModule) : base(true, inputs, output, String.Empty)
        {
            this.associatedModule = associatedModule;
        }

        public TestBlock(List<FluidBlock> inputs, string output, Module associatedModule) : this(inputs.Select(input => (FluidInput)new BasicInput("", input.OutputVariable, input.OriginalOutputVariable, 1, true)).ToList(), output, associatedModule)
        {

        }

        public override Module getAssociatedModule()
        {
            return associatedModule;
        }

        public override string ToString()
        {
            return "Test block";
        }
    }
}