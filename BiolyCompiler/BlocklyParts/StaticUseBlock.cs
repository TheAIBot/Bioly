using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class StaticUseageBlock : StaticBlock
    {

        public StaticUseageBlock(string moduleName, List<FluidInput> inputs, bool canBeOutput, string output, string id) : base(moduleName, inputs, canBeOutput, output, id)
        {
        }


        public override Module getAssociatedModule()
        {
            throw new Exception("As this block represents the use of a module, it has no associated module.");
        }


    }
}
