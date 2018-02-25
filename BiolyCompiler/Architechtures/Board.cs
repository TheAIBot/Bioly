using System;
using BiolyCompiler.Modules;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth, width;
        public List<Module> placedModules = new List<Module>();
        public Module[][] grid;
        public List<Rectangle> emptyRectangles = new List<Rectangle>();


        public Board(int width, int heigth){
            this.width  = width;
            this.heigth = heigth;
            this.grid = new Module[heigth][width];
            emptyRectangles.add(new Rectangle(0,0, width, heigth));
        }

        //Based on the algorithm seen in figure 6.3, "Fault-Tolerant Digital Microfluidic Biochips - Compilation and Synthesis"
        public bool place(Module module){
            //List<Rectangle> rectangles = ConstructRectangleList(module.grid);
            
            //(*)Need to take into account if the module has been scheduled at a prior time,
            // and as such cannot be moved.

            Rectangle bestFit = SelectRectangle(module);
            if (bestFit != null){
                bool couldBePlaced = UpdatePlacement(Rectangle, Module);
                emptyRectangles = UpdateFreeSpace();
            }
        }

        private Rectangle SelectRectangle(Module module){
            List<Rectangle> fittingRectangles = emptyRectangles.Where(emptyRectangle => emptyRectangle.fits(module.rectangle));
            int bestFitValue = -1;
            Rectangle bestFit;
            foreach (var Rectangle in fittingRectangles)
            {
                
            }
        }




    }
}
