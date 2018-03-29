using System;
using System.Collections.Generic;

namespace BiolyCompiler.Modules
{
    public class ModuleLayout
    {
        //The empty rectangles and the output locations should partition the whole module, with no overlap.
        //It should also be done in such a way that the fast template placement merges everything correctly.
        public List<Rectangle> EmptyRectangles;
        public List<Droplet> OutputLocations;
        public readonly int width, height;

        public ModuleLayout(int width, int height, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations)
        {
            this.width = width;
            this.height = height;
            if (!IsValidModuleDivision(EmptyRectangles, OutputLocations)) throw new Exception();
            this.EmptyRectangles = EmptyRectangles;
            this.OutputLocations = OutputLocations;
        }

        private bool IsValidModuleDivision(List<Rectangle> emptyRectangle, List<Droplet> outputLocations)
        {
            throw new NotImplementedException();
        }
    }
}