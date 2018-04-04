using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using BiolyCompiler.Modules.OperationTypes;
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
        public Module boundModule;

        public FluidBlock(bool canBeOutput, string output) : base(canBeOutput, output)
        {
            InputVariables = EmptyList;
        }

        public FluidBlock(bool canBeOutput, List<FluidInput> input, string output) : base(canBeOutput, output)
        {
            InputVariables = input;
        }

        public virtual OperationType getOperationType()
        {
            return OperationType.Unknown;
        }

        public virtual Module getAssociatedModule()
        {
            throw new NotImplementedException("No modules have been associated with blocks/operations of type " + this.GetType().ToString());
        }

        public void Bind(Module module)
        {
            boundModule = module;
            module.BindingOperation = this;
        }

        internal void Unbind(Module module)
        {
            throw new NotImplementedException();
        }
    }
}
