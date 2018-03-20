using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts;
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
        public List<Rectangle> EmptyRectangles = new List<Rectangle>();
        public HashSet<BoardFluid> fluids = new HashSet<BoardFluid>();
        public Module[,] grid;


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
                PlaceModuleInRectangle(module, bestFitRectangle, bestIndex);
                return true;
            }
            else return false;
        }

        private (Rectangle, Rectangle) PlaceModuleInRectangle(Module module, Rectangle bestFitRectangle, int bestIndex)
        {
            EmptyRectangles[bestIndex] = EmptyRectangles[EmptyRectangles.Count - 1];
            EmptyRectangles.RemoveAt(EmptyRectangles.Count - 1);
            UpdateGridWithModulePlacement(module, bestFitRectangle);            
            (Rectangle topRectangle, Rectangle rightRectangle) = bestFitRectangle.SplitIntoSmallerRectangles(module);
            if (topRectangle != null) EmptyRectangles.Add(topRectangle);
            if (rightRectangle != null) EmptyRectangles.Add(rightRectangle);
            return (topRectangle, rightRectangle);
        }

        public void FastTemplateRemove(Module module)
        {
            placedModules.Remove(module);
            FastTemplateRemove(module.shape);                   
        }


        public void FastTemplateRemove(Rectangle rectangle)
        {
            //All dependencies on the rectangle from the module, should be moved to the new empty rectangle.
            //It is easier to just create a new rectangle for the module:
            Rectangle NewModuleRectangle = new Rectangle(rectangle);
            Rectangle EmptyRectangle = rectangle;
            EmptyRectangles.Add(EmptyRectangle);
            EmptyRectangle.isEmpty = true;
            rectangle = NewModuleRectangle;
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

        public void UpdateGridWithModulePlacement(Module module, Rectangle rectangleToPlaceAt)
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

        private bool UpdatePlacement(Rectangle rectangle, Module module)
        {
            throw new NotImplementedException();
        }

        public String print()
        {
            StringBuilder printedBoard = new StringBuilder();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < heigth; j++)
                {
                    if (grid[i, j] == null) printedBoard.Append("O");
                    else printedBoard.Append("X");
                }
                printedBoard.AppendLine();
            }
            return printedBoard.ToString();
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

        public Node<RoutingInformation> getOperationFluidPlacementOnBoard(Module sourceModule, Node<RoutingInformation>[,] dijkstraGraph)
        {
            return dijkstraGraph[sourceModule.shape.x, sourceModule.shape.y];
        }

        public Droplet replaceWithDroplets(Block finishedOperation)
        {
            Droplet droplet = new Droplet();
            Rectangle moduleRectangle = finishedOperation.boundModule.shape;
            UpdateGridWithModulePlacement(droplet, moduleRectangle);
            FastTemplateReplace(moduleRectangle, droplet);
            placedModules.Remove(finishedOperation.boundModule);
            return droplet;
        }

        private void FastTemplateReplace(Rectangle oldRectangle, Module replacingModule)
        {
            (Rectangle TopRectangle, Rectangle RightRectangle) = oldRectangle.SplitIntoSmallerRectangles(replacingModule);
            //All the adjacenecies from the old rectangle (moduleRectangle), should be removed, and added to the new rectangles.
            foreach (var adjacentRectangle in oldRectangle.AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(oldRectangle);
                if (adjacentRectangle.IsAdjacent(replacingModule.shape))
                {
                    replacingModule.shape.AdjacentRectangles.Add(adjacentRectangle);
                    adjacentRectangle.AdjacentRectangles.Add(replacingModule.shape);
                }
                if (TopRectangle != null && adjacentRectangle.IsAdjacent(TopRectangle))
                {
                    TopRectangle.AdjacentRectangles.Add(adjacentRectangle);
                    adjacentRectangle.AdjacentRectangles.Add(TopRectangle);
                }
                if (RightRectangle != null && adjacentRectangle.IsAdjacent(RightRectangle))
                {
                    RightRectangle.AdjacentRectangles.Add(adjacentRectangle);
                    adjacentRectangle.AdjacentRectangles.Add(RightRectangle);
                }
            }
            //The two empty rectangles from the splitting can be removed, as they are not used by the placed module:
            if (TopRectangle != null) FastTemplateRemove(TopRectangle);
            if (RightRectangle != null) FastTemplateRemove(RightRectangle);
        }

        public Board Copy()
        {
            Board board = new Board(width, heigth);
            board.EmptyRectangles.Clear();
            placedModules.  ForEach(x => board.placedModules.Add(x));
            EmptyRectangles.ForEach(x => board.EmptyRectangles.Add(x));
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < heigth; j++)
                {
                    board.grid[i, j] = grid[i, j];
                }
            }
            return board;
        }
    }
}
