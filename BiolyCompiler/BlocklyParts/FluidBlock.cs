using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
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
        public Module boundModule = null;

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

        public void Bind(Module module)
        {
            boundModule = module;
            module.BindingOperation = this;

            //The fluid types of the module layout, is changedto fit with the operation:
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
            this.boundModule = null;
        }
    }
}
