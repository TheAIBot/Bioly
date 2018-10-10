using System;
using System.Collections.Generic;
using System.Linq;

namespace BiolyCompiler.Modules
{
    public class BoardFluid
    {
        public readonly string FluidName;
        public List<IDropletSource> dropletSources = new List<IDropletSource>();
        public int RefCount = 1;

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
            if (obj is BoardFluid fluidObj) return FluidName == fluidObj.FluidName;
            else return false;
        }

        public int GetNumberOfDropletsAvailable()
        {
            return dropletSources.Sum(x => x is Droplet ? 1 : (x as InputModule).DropletCount);
        }
    }
}