using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class FluidBlock : Block
    {
        //For the scheduling.
        public Module BoundModule = null;
        //The key is the input fluid name, see InputVariables.
        public Dictionary<string, List<Route>> InputRoutes = new Dictionary<string, List<Route>>();
        //For garbage collection:
        public Dictionary<string, List<Route>> WasteRoutes = new Dictionary<string, List<Route>>();


        public FluidBlock(bool canBeOutput, List<FluidInput> inputFluids, List<string> inputNumbers, string output, string id) : 
            base(canBeOutput, inputFluids, inputNumbers, output, id)
        {
        }

        public abstract Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix);

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            InputFluids.ForEach(x => x.Update(variables, executor, dropPositions));
        }

        public virtual Module getAssociatedModule()
        {
            throw new InternalRuntimeException("No modules have been associated with blocks/operations of type " + this.GetType().ToString());
        }

        public virtual void Bind(Module module, Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            BoundModule = module;
            //module.BindingOperation = this;

            //The fluid types of the module layout, is changedto fit with the operation.
            //Thus for example, when the module is removed when the operations have finished,
            //the remaining droplets will have the correct type, and can be used by operations requiring the output of that module.

            int currentDroplet = 0;
            foreach (var fluid in InputFluids)
            {
                BoardFluid fluidType = new BoardFluid(fluid.OriginalFluidName);
                for (int i = 0; i < fluid.GetAmountInDroplets(FluidVariableLocations); i++)
                {
                    module.GetInputLayout().Droplets[currentDroplet].SetFluidType(fluidType);
                    currentDroplet++;
                }
            }
            BoardFluid outputFluidType = new BoardFluid(OutputVariable);
            foreach (var droplet in module.GetOutputLayout().Droplets)
            {
                droplet.SetFluidType(outputFluidType);
            }

        }

        public int GetRunningTime()
        {
            return ToCommands().Last().Time;
        }

        public virtual List<Command> ToCommands()
        {
            int time = 0;
            List<Command> commands = new List<Command>();

            if (!(this is StaticBlock))
            {
                //show module on simulator
                commands.Add(new AreaCommand(BoundModule.Shape, CommandType.SHOW_AREA, 0));
            }

            //add commands for waste routes. They must be before the other routes
            foreach (List<Route> wasteRouteList in WasteRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                wasteRouteList.ForEach(route => commands.AddRange(route.ToCommands(ref time)));
            }

            //add commands for the routes
            foreach (List<Route> routeList in InputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routeList.ForEach(route => commands.AddRange(route.ToCommands(ref time)));
            }

            //add commands for the module itself
            commands.AddRange(BoundModule.GetModuleCommands(ref time));

            if (!(this is StaticBlock))
            {
                //remove module from simulator
                commands.Add(new AreaCommand(BoundModule.Shape, CommandType.REMOVE_AREA, time));
            }

            return commands;
        }

        internal void Unbind(Module module)
        {
            throw new InternalRuntimeException("This method is not supported for this block.");
        }

        protected override void ResetBlock()
        {
            this.BoundModule = null;
            InputRoutes.Clear();
            WasteRoutes.Clear();
        }

        public virtual void UpdateInternalDropletConcentrations()
        {
            List<Route> allRoutes = new List<Route>();
            InputRoutes.Select(pair => pair.Value).ForEach(list => allRoutes.AddRange(list));
            List<IDropletSource> dropletInputs = allRoutes.Select(route => route.routedDroplet).ToList();
            for (int i = 0; i < dropletInputs.Count; i++)
            {
                BoundModule.GetOutputLayout().Droplets[i].SetFluidConcentrations(dropletInputs[i]);
            }
        }
    }
}
