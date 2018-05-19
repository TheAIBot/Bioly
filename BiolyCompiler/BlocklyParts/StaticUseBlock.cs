using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class StaticUseageBlock : StaticBlock
    {
        //When the useage of a static module has finished, sometimes droplets needs to be moved out of the module:
        public Dictionary<string, List<Route>> OutputRoutes = new Dictionary<string, List<Route>>();

        public StaticUseageBlock(string moduleName, List<FluidInput> inputs, bool canBeOutput, string output, string id) : base(moduleName, inputs, canBeOutput, output, id)
        {
        }


        public override Module getAssociatedModule()
        {
            throw new Exception("As this block represents the use of a module, it has no associated module.");
        }


    }
}
