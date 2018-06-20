using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class StaticBlock : FluidBlock
    {
        //Each static block gets one associated module name. It must be unique.
        //It will be used to find the associated module, during the scheduling.
        public const string MODULE_NAME_FIELD_NAME = "moduleName";
        public const string DEFAULT_MODULE_NAME = "module_name";
        public readonly string ModuleName; 

        public StaticBlock(string moduleName, bool canBeOutput, string output, string id) : base(canBeOutput, null, null, output, id)
        {
            this.ModuleName = moduleName;
        }

        public StaticBlock(string moduleName, List<FluidInput> inputFluids, List<string> inputNumbers, bool canBeOutput, string output, string id) : 
            base(canBeOutput, inputFluids, inputNumbers, output, id)
        {
            this.ModuleName = moduleName;
        }

    }
}
