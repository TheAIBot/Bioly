using System.Collections.Generic;

namespace BiolyCompiler.Modules
{
    public class BoardFluid
    {
        public readonly string FluidName;
        public List<Droplet> droplets = new List<Droplet>();

        public BoardFluid(string fluidName)
        {
            this.FluidName = fluidName;
        }

        public override int GetHashCode()
        {
            return FluidName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BoardFluid fluidObj)
            {
                return FluidName == fluidObj.FluidName;
            }
            return false;
        }
    }
}