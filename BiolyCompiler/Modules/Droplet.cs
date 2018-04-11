using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;


namespace BiolyCompiler.Modules
{
    public class Droplet : Module, IDropletSource
    {
        private BoardFluid fluidType;
        public const int DROPLET_WIDTH = 3, DROPLET_HEIGHT = 3;

        public Droplet(BoardFluid fluidType) : base(DROPLET_WIDTH, DROPLET_HEIGHT, 0, 0, 0)
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
            this.fluidType.droplets.Remove(this);
            this.fluidType = fluidType;
            fluidType.droplets.Add(this);
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;
            else return (obj as Droplet).fluidType.Equals(this.fluidType);
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }
        

        protected override List<Command> GetModuleCommands()
        {
            throw new Exception("Droplet can't be converted into commands");
        }
    }
}
