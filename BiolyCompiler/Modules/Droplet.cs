using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Routing;
using MoreLinq;

namespace BiolyCompiler.Modules
{
    public class Droplet : Module, IDropletSource
    {
        private BoardFluid fluidType;
        public const int DROPLET_WIDTH = 3, DROPLET_HEIGHT = 3;
        public Dictionary<string, float> FluidConcentrations = new Dictionary<string, float>(); 

        public Droplet() : base(DROPLET_WIDTH, DROPLET_HEIGHT, 0, false)
        {
        }

        public Droplet(BoardFluid fluidType, HashSet<string> NameOfUsedFluids) : this(fluidType)
        {
            foreach (var name in NameOfUsedFluids)
            {
                FluidConcentrations.Add(name, 0);
            }
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
            this.fluidType?.dropletSources.Remove(this);
            this.fluidType = fluidType;
            fluidType.dropletSources.Add(this);
        }

        public void FakeSetFluidType(BoardFluid fluidType)
        {
            //Only to be used by waste!!!
            this.fluidType = fluidType;
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
            throw new InternalRuntimeException("This method is not suported.");
        }


        public override List<Command> GetModuleCommands(ref int time)
        {
            throw new InternalRuntimeException("Droplet can't be converted into commands");
        }

        public bool IsInMiddleOfSource(RoutingInformation location)
        {
            return location.x == Shape.x + DROPLET_WIDTH / 2 &&
                   location.y == Shape.y + DROPLET_HEIGHT / 2;
        }

        public (int, int) GetMiddleOfSource() {
            return Shape.getCenterPosition();
        }

        public void SetConcentrationOfFluid(string fluidName, float concentration)
        {
            FluidConcentrations[fluidName] = concentration;
        }

        public Dictionary<string, float> GetFluidConcentrations()
        {
            return FluidConcentrations;
        }

        public void SetFluidConcentrations(IDropletSource dropleSource)
        {
            dropleSource.GetFluidConcentrations().ForEach(pair => FluidConcentrations[pair.Key] = pair.Value);
        }
    }
}
