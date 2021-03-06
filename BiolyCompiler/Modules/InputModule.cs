using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Routing;

namespace BiolyCompiler.Modules
{
    public class InputModule : Module, IDropletSource
    {
        private BoardFluid FluidType;
        public readonly int Capacity;
        public Dictionary<string, float> FluidConcentrations = new Dictionary<string, float>();
        public int DropletCount { get; private set; }

        public InputModule(BoardFluid fluidType, int capacity) : base(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0, 1, null)
        {
            this.FluidType = fluidType;
            fluidType.dropletSources.Add(this);
            if (capacity < 1) throw new RuntimeException("A droplet source/spawner needs to be initally non-empty. The droplet source has fluid type = " + fluidType.ToString());
            this.Capacity = capacity;
            this.DropletCount = capacity;
            FluidConcentrations.Add(FluidType.FluidName, 1);
        }
        

        public void DecrementDropletCount()
        {
            DropletCount--;
        }
        
        public override int getNumberOfInputs()
        {
            return 0;
        }
        
        public override int getNumberOfOutputs()
        {
            return 0;
        }

        public (int, int) GetMiddleOfSource() {
            return (Shape.x + Droplet.DROPLET_WIDTH / 2, Shape.y + Droplet.DROPLET_HEIGHT / 2);
        }

        public BoardFluid GetFluidType()
        {
            return FluidType;
        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            throw new InternalRuntimeException("A droplet spawner does not execute any commands.");
        }

        public void SetFluidType(BoardFluid fluidType)
        {
            this.FluidType?.dropletSources.Remove(this);
            this.FluidType = fluidType;
            fluidType.dropletSources.Add(this);
        }

        public Dictionary<string, float> GetFluidConcentrations()
        {
            return FluidConcentrations;
        }
    }
}
