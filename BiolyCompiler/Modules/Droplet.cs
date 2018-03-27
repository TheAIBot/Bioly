using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class Droplet : Module
    {
        public BoardFluid fluidType;

        public Droplet(BoardFluid fluidType) : base(3, 3, 0, 0, 0)
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
    }
}
