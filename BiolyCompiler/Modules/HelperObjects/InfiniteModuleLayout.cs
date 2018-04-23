using System;
using System.Collections.Generic;
using System.Linq;

namespace BiolyCompiler.Modules
{
    //Primarily for the output to use, it permits the overlap of droplets in the layout
    public class InfiniteModuleLayout : ModuleLayout
    {
        
        public InfiniteModuleLayout(int width, int height, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations) : base(EmptyRectangles, OutputLocations)
        {
            this.width = width;
            this.height = height;
        }



        public void SetGivenAmountOfDroplets(int dropletAmounts, Module module)
        {
            Droplets.Clear();
            for (int i = 0; i < dropletAmounts; i++){
                Droplet droplet = new Droplet();
                droplet.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 0, 0);
                Droplets.Add(new Droplet());
            }
            module.RepositionLayout();
        }

    }
}