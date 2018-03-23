using System.Collections.Generic;

namespace BiolyCompiler.Modules
{
    public class BoardFluid
    {
        public readonly string fluidName;
        public HashSet<Droplet> droplets = new HashSet<Droplet>();

        public BoardFluid(string fluidName)
        {
            this.fluidName = fluidName;
        }

        public override int GetHashCode()
        {
            return fluidName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            BoardFluid fluidObj = obj as BoardFluid;
            if (fluidObj == null) return false;
            else return fluidName.Equals(fluidObj.fluidName);
        }
    }
}