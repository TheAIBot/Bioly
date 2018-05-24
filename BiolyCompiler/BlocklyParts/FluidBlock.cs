using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class FluidBlock : Block
    {
        public readonly IReadOnlyList<FluidInput> InputVariables;

        private static readonly List<FluidInput> EmptyList = new List<FluidInput>();
        //For the scheduling.
        public Module BoundModule = null;
        //The key is the input fluid name, see InputVariables.
        public Dictionary<string, List<Route>> InputRoutes = new Dictionary<string, List<Route>>();

        public FluidBlock(bool canBeOutput, string output, string id) : base(canBeOutput, output, id)
        {
            InputVariables = EmptyList;
        }

        public FluidBlock(bool canBeOutput, List<FluidInput> input, string output, string id) : base(canBeOutput, output, id)
        {
            InputVariables = input;
        }

        public virtual Module getAssociatedModule()
        {
            throw new NotImplementedException("No modules have been associated with blocks/operations of type " + this.GetType().ToString());
        }

        public virtual void Bind(Module module, Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            BoundModule = module;
            //module.BindingOperation = this;

            //The fluid types of the module layout, is changedto fit with the operation.
            //Thus for example, when the module is removed when the operations have finished,
            //the remaining droplets will have the correct type, and can be used by operations requiring the output of that module.

            int currentDroplet = 0;
            foreach (var fluid in InputVariables)
            {
                BoardFluid fluidType = new BoardFluid(fluid.OriginalFluidName);
                for (int i = 0; i < fluid.GetAmountInDroplets(FluidVariableLocations); i++)
                {
                    module.GetInputLayout().Droplets[currentDroplet].SetFluidType(fluidType);
                    currentDroplet++;
                }
            }
            BoardFluid outputFluidType = new BoardFluid(OriginalOutputVariable);
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
            throw new NotImplementedException();
        }

        protected override void ResetBlock()
        {
            this.BoundModule = null;
            InputRoutes.Clear();
        }
    }
}
