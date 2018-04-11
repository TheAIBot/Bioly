using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;


namespace BiolyCompiler.Modules
{
    public class InputModule : Module, IDropletSource
    {
        private BoardFluid FluidType;
        public readonly int Capacity;
        public int DropletCount { get; private set; }

        public InputModule(BoardFluid fluidType, int capacity) : base(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0, 0)
        {
            this.FluidType = fluidType;
            fluidType.droplets.Add(this);
            if (capacity < 1) throw new Exception("A droplet source/spawner needs to be non-empty. The droplet source has fluid type = " + fluidType.ToString());
            this.Capacity = capacity;
            this.DropletCount = capacity;
        }

        public void DecrementDropletCount()
        {
            DropletCount--;
        }
                
        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }

        public BoardFluid getFluidType()
        {
            return FluidType;
        }

        protected override List<Command> GetModuleCommands()
        {
            throw new Exception("A droplet spawner does not execute any commands.");
        }
    }
}
