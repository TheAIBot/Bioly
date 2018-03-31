using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class Droplet : Module
    {
        public BoardFluid fluidType;
        public const int DROPLET_WIDTH = 3, DROPLET_HEIGHT = 3;

        public Droplet(BoardFluid fluidType) : base(DROPLET_WIDTH, DROPLET_HEIGHT, 0, 0, 0)
        {
            this.fluidType = fluidType;
        }


        public override OperationType getOperationType(){
            return OperationType.DropletStorage;
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }

        public void SetFluidType(BoardFluid fluidType)
        {
            this.fluidType = fluidType;
            fluidType.droplets.Add(this);
        }
    }
}
