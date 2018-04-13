using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Routing;

namespace BiolyCompiler.Modules
{
    public class InputModule : Module, IDropletSource
    {
        private BoardFluid FluidType;
        public readonly int Capacity;
        public int DropletCount { get; private set; }

        public InputModule(BoardFluid fluidType, int capacity) : base(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0, 1, null)
        {
            this.FluidType = fluidType;
            fluidType.droplets.Add(this);
            if (capacity < 1) throw new Exception("A droplet source/spawner needs to be initally non-empty. The droplet source has fluid type = " + fluidType.ToString());
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
        
        public override int getNumberOfInputs()
        {
            return 0;
        }
        
        public override int getNumberOfOutputs()
        {
            return 0;
        }

        public bool isInMiddleOfSource(RoutingInformation information)
        {
            (int xMiddle, int yMiddle) = getMiddleOfSource();
            return xMiddle == information.x && yMiddle == information.y;
        }

        public (int, int) getMiddleOfSource() {
            return (Shape.x + Droplet.DROPLET_WIDTH / 2, Shape.y + Droplet.DROPLET_HEIGHT / 2);
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
