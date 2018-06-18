using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class StaticUseageBlock : StaticBlock
    {
        //When the useage of a static module has finished, sometimes droplets needs to be moved out of the module:
        public Dictionary<string, List<Route>> OutputRoutes = new Dictionary<string, List<Route>>();

        public StaticUseageBlock(string moduleName, List<FluidInput> inputFluids, List<string> inputNumbers, bool canBeOutput, string output, string id) : 
            base(moduleName, inputFluids, inputNumbers, canBeOutput, output, id)
        {

        }

        public override List<Command> ToCommands()
        {
            List<Command> commands =  base.ToCommands();
            int time = commands.Last().Time;
            //There can be extra output routes associated with a static use block:
            foreach (List<Route> routeList in OutputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routeList.ForEach(route => commands.AddRange(route.ToCommands(ref time)));
            }
            return commands;
        }

        protected override void ResetBlock()
        {
            base.ResetBlock();
            OutputRoutes.Clear();
        }

        public override Module getAssociatedModule()
        {
            throw new InternalRuntimeException("As this block represents the use of a module, it has no associated module.");
        }


    }
}
