using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
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
            fluidType.dropletSources.Add(this);
        }

        public BoardFluid GetFluidType()
        {
            return fluidType;
        }

        public void SetFluidType(BoardFluid fluidType)
        {
            if (this.fluidType != null) this.fluidType.dropletSources.Remove(this);
            this.fluidType = fluidType;
            fluidType.dropletSources.Add(this);
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

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }


        public override List<Command> GetModuleCommands(ref int time)
        {
            throw new Exception("Droplet can't be converted into commands");
        }

        public bool IsInMiddleOfSource(RoutingInformation location)
        {
            return location.x == Shape.x + DROPLET_WIDTH / 2 &&
                   location.y == Shape.y + DROPLET_HEIGHT / 2;
        }

        public (int, int) GetMiddleOfSource() {
            return Shape.getCenterPosition();
        }
        
    }
}
