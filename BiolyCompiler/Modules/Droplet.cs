using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Routing;

namespace BiolyCompiler.Modules
{
    public class Droplet : Module, IDropletSource
    {
        private BoardFluid fluidType;
        public const int DROPLET_WIDTH = 3, DROPLET_HEIGHT = 3;

        public Droplet() : base(DROPLET_WIDTH, DROPLET_HEIGHT, 0, false)
        {
        }

        public Droplet(BoardFluid fluidType) : base(DROPLET_WIDTH, DROPLET_HEIGHT, 0, false)
        {
            this.fluidType = fluidType;
            fluidType.droplets.Add(this);
        }

        public BoardFluid getFluidType()
        {
            return fluidType;
        }

        public void SetFluidType(BoardFluid fluidType)
        {
            if (this.fluidType != null) this.fluidType.droplets.Remove(this);
            this.fluidType = fluidType;
            fluidType.droplets.Add(this);
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;
            else return (obj as Droplet).fluidType.Equals(this.fluidType);
        }

        public override int getNumberOfInputs()
        {
            return 0;
        }

        public override int getNumberOfOutputs()
        {
            return 0;
        }

        public override OperationType getOperationType(){
            return OperationType.DropletStorage;
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }
        

        protected override List<Command> GetModuleCommands()
        {
            throw new Exception("Droplet can't be converted into commands");
        }

        public bool isInMiddleOfSource(RoutingInformation location)
        {
            return location.x == Shape.x + DROPLET_WIDTH / 2 &&
                   location.y == Shape.y + DROPLET_HEIGHT / 2;
        }

        public (int, int) getMiddleOfSource() {
            return (Shape.x + DROPLET_WIDTH / 2, Shape.y + DROPLET_HEIGHT / 2);
        }
    }
}
