using System;
using System.Collections.Generic;
using BiolyCompiler.Modules;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth, width;
        public List<Module> placedModules = new List<Module>();
        public List<Droplet> placedDroplets = new List<Droplet>();
        public Module[,] grid;
        public List<Rectangle> emptyRectangles = new List<Rectangle>();


        public Board(int width, int heigth){
            this.width  = width;
            this.heigth = heigth;
            this.grid = new Module[heigth, width];
            emptyRectangles.Add(new Rectangle(width, heigth));
        }

        //
        public bool FastTemplatePlace(Module module)
        {

            return false;
        }

        //Based on the algorithm seen in figure 6.3, "Fault-Tolerant Digital Microfluidic Biochips - Compilation and Synthesis"
        public bool place(Module module){
            //List<Rectangle> rectangles = ConstructRectangleList(module.grid);
            
            //(*)Need to take into account if the module has been scheduled at a prior time,
            // and as such cannot be moved.

            Rectangle bestFit = SelectRectangle(module);
            if (bestFit != null){
                bool couldBePlaced = UpdatePlacement(bestFit, module);
                emptyRectangles = UpdateFreeSpace();
            }

            return false;
        }

        private bool UpdatePlacement(Rectangle rectangle, Module module)
        {
            throw new NotImplementedException();
        }

        private List<Rectangle> UpdateFreeSpace()
        {
            throw new NotImplementedException();
        }

        internal void removeAllDroplets()
        {
            throw new NotImplementedException();
        }

        internal bool sequentiallyPlace(Module module)
        {
            throw new NotImplementedException();
        }

        private Rectangle SelectRectangle(Module module){
            List<Rectangle> fittingRectangles = new List<Rectangle>();// = emptyRectangles.Where(emptyRectangle => emptyRectangle.fits(module.rectangle));
            int bestFitValue = -1;
            Rectangle bestFit;
            foreach (var Rectangle in fittingRectangles)
            {
                
            }
            return null;
        }

        internal bool placeAllDroplets()
        {
            throw new NotImplementedException();
        }
    }
}
