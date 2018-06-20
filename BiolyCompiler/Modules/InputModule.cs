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
        public int DropletCount { get; private set; }

        public InputModule(BoardFluid fluidType, int capacity) : base(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0, 1, null)
        {
            this.FluidType = fluidType;
            fluidType.dropletSources.Add(this);
            if (capacity < 1) throw new RuntimeException("A droplet source/spawner needs to be initally non-empty. The droplet source has fluid type = " + fluidType.ToString());
            this.Capacity = capacity;
            this.DropletCount = capacity;
        }
        

        public void DecrementDropletCount()
        {
            DropletCount--;
        }
                
        public override Module GetCopyOf()
        {
            InputModule newInputModule = new InputModule(new BoardFluid(FluidType.FluidName), Capacity);
            newInputModule.Shape = new Rectangle(Shape);
            for (int i = Capacity - DropletCount; i > 0; i--) newInputModule.DecrementDropletCount();
            newInputModule.InputLayout  = this.InputLayout.GetCopy();
            newInputModule.OutputLayout = this.OutputLayout?.GetCopy();
            return newInputModule;
        }
        
        public override int getNumberOfInputs()
        {
            return 0;
        }
        
        public override int getNumberOfOutputs()
        {
            return 0;
        }

        public bool IsInMiddleOfSource(RoutingInformation information)
        {
            (int xMiddle, int yMiddle) = GetMiddleOfSource();
            return xMiddle == information.x && yMiddle == information.y;
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
            if (this.FluidType != null) this.FluidType.dropletSources.Remove(this);
            this.FluidType = fluidType;
            fluidType.dropletSources.Add(this);
        }
    }
}
