﻿using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
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

        public virtual void Bind(Module module)
        {
            BoundModule = module;
            module.BindingOperation = this;

            //The fluid types of the module layout, is changedto fit with the operation.
            //Thus for example, when the module is removed when the operations have finished,
            //the remaining droplets will have the correct type, and can be used by operations requiring the output of that module.

            int currentDroplet = 0;
            foreach (var fluid in InputVariables)
            {
                BoardFluid fluidType = new BoardFluid(fluid.FluidName);
                for (int i = 0; i < fluid.GetAmountInDroplets(); i++)
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

        internal void Unbind(Module module)
        {
            throw new NotImplementedException();
        }

        protected override void ResetBlock()
        {
            this.BoundModule = null;
        }
    }
}
