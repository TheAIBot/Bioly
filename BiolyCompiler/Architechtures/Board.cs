using System;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth, width;
        public List<Module> placedModules = new List<Module>();
        public List<Droplet> placedDroplets = new List<Droplet>();
        public Module[,] grid;
        public List<Rectangle> EmptyRectangles = new List<Rectangle>();


        public Board(int width, int heigth){
            this.width  = width;
            this.heigth = heigth;
            this.grid = new Module[heigth, width];
            EmptyRectangles.Add(new Rectangle(width, heigth));
        }

        //Based on the algorithm from "Fast template placement for reconfigurable computing systems"
        public bool FastTemplatePlace(Module module)
        {
            //(*)TODO use 2d range trees instead of a linear search.
            Rectangle bestFitRectangle = null;
            int bestFitScore = Int32.MaxValue;
            int bestIndex = 0;
            for (int i = 0; i < EmptyRectangles.Count; i++)
            {
                Rectangle Rectangle = EmptyRectangles[i];
                if (Rectangle.DoesFit(module))
                {
                    int Cost = RectangleCost(Rectangle, module);
                    if (Cost < bestFitScore)
                    {
                        bestFitRectangle = Rectangle;
                        bestFitScore = Cost;
                        bestIndex = i;
                    }

                }
            }
            if (bestFitRectangle != null)
            {
                //Removes bestFitRectangle, hopefully in constant time
                EmptyRectangles[bestIndex] = EmptyRectangles[EmptyRectangles.Count - 1];
                EmptyRectangles.RemoveAt(EmptyRectangles.Count - 1);
                PlaceModule(module, bestFitRectangle);
                Tuple<Rectangle, Rectangle> splittedRectangles = bestFitRectangle.SplitIntoSmallerRectangles(module);
                if (splittedRectangles.Item1 != null) EmptyRectangles.Add(splittedRectangles.Item1);
                if (splittedRectangles.Item2 != null) EmptyRectangles.Add(splittedRectangles.Item2);
                return true;
            }
            else return false;
        }

        public void FastTemplateRemove(Module module)
        {
            placedModules.Remove(module);
            //All dependencies on the rectangle from the module, should be moved to the new empty rectangle.
            //It is easier to just create a new rectangle for the module:
            Rectangle NewModuleRectangle = new Rectangle(module.shape);
            Rectangle EmptyRectangle     = module.shape;
            EmptyRectangles.Add(EmptyRectangle);
            EmptyRectangle.isEmpty = true;
            module.shape = NewModuleRectangle;
            NewModuleRectangle.isEmpty = false;

            ClearBoard(EmptyRectangle);
            EmptyRectangle.MergeWithOtherRectangles(this);            
        }

        private void ClearBoard(Rectangle emptyRectangle)
        {
            for (int i = 0; i < emptyRectangle.width; i++)
            {
                for (int j = 0; j < emptyRectangle.height; j++)
                {
                    grid[i + emptyRectangle.x, j + emptyRectangle.y] = null;
                }
            }
        }

        private void PlaceModule(Module module, Rectangle rectangleToPlaceAt)
        {
            module.shape.PlaceAt(rectangleToPlaceAt.x, rectangleToPlaceAt.y);
            for (int i = 0; i < module.shape.width; i++)
            {
                for (int j = 0; j < module.shape.height; j++)
                {
                    grid[i + module.shape.x, j + module.shape.y] = module;
                }
            }
            placedModules.Add(module);
        }

        private int RectangleCost(Rectangle rectangle, Module module)
        {
            return Math.Abs(rectangle.GetArea() - module.shape.GetArea());
        }




        //based on the algorithm seen in figure 6.3, "Fault-Tolerant Digital Microfluidic Biochips - Compilation and Synthesis"
        public bool place(Module module){
            
            //List<Rectangle> rectangles = ConstructRectangleList(module.grid);
            
            Rectangle bestFit = SelectRectangle(module);
            if (bestFit != null){
                bool couldBePlaced = UpdatePlacement(bestFit, module);
                EmptyRectangles = UpdateFreeSpace();
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

        public Node<RoutingInformation> getOperationFluidPlacementOnBoard(Block operation, Node<RoutingInformation>[,] dijkstraGraph)
        {
            return dijkstraGraph[operation.boundModule.shape.x, operation.boundModule.shape.y];
        }
    }
}
