using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts;
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
        public Dictionary<string,BoardFluid> fluids = new Dictionary<string,BoardFluid>();
        public Module[,] grid;


        public Board(int width, int heigth){
            this.width  = width;
            this.heigth = heigth;
            this.grid = new Module[width,heigth];
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

        public (Rectangle, Rectangle) PlaceModuleInRectangle(Module module, Rectangle bestFitRectangle, int bestIndex)
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
            FastTemplateRemove(module.Shape);                   
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
            module.Shape.PlaceAt(rectangleToPlaceAt.x, rectangleToPlaceAt.y);
            Rectangle Shape = module.Shape;
            for (int i = 0; i < Shape.width; i++)
            {
                for (int j = 0; j < Shape.height; j++)
                {
                    grid[i + Shape.x, j + Shape.y] = module;
                }
            }
            placedModules.Add(module);
        }
        

        private int RectangleCost(Rectangle rectangle, Module module)
        {
            return Math.Abs(rectangle.GetArea() - module.Shape.GetArea());
        }

        private bool UpdatePlacement(Rectangle rectangle, Module module)
        {
            throw new NotImplementedException();
        }

        public String print(List<Module> allPlacedModules)
        {
            StringBuilder printedBoard = new StringBuilder();
            int paddingLenght = (int) Math.Log10(allPlacedModules.Count) + 1;
            for (int j = heigth - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    if (grid[i, j] == null) printedBoard.Append(String.Format("{0,2}", "O"));
                    else {
                        int index = allPlacedModules.IndexOf(grid[i,j]);
                        printedBoard.Append(String.Format("{0,2}", index));
                    }
                }
                printedBoard.AppendLine();
            }
            printedBoard.AppendLine();
            printedBoard.AppendLine();
            return printedBoard.ToString();
        }

        public Node<RoutingInformation> getSourceNodeForSourceModule(Module sourceModule, Node<RoutingInformation>[,] dijkstraGraph)
        {
            return dijkstraGraph[sourceModule.Shape.x, sourceModule.Shape.y];
        }

        public List<Droplet> replaceWithDroplets(FluidBlock finishedOperation, BoardFluid fluidType)
        {
            Module operationExecutingModule = finishedOperation.boundModule;
            //Checks for each pair of adjacent rectangle to the module on the board, and the rectangles in the modules layout,
            //if they are adjacent -> if so, it makes them adjacent.
            List<Rectangle> AllRectangles = new List<Rectangle>(operationExecutingModule.GetModuleLayout().EmptyRectangles);
            operationExecutingModule.GetModuleLayout().OutputDroplets.ForEach(droplet => AllRectangles.Add(droplet.Shape));
            foreach (var moduleAdjacentRectangle in operationExecutingModule.Shape.AdjacentRectangles)
            {
                foreach (var moduleLayoutRectangle in AllRectangles)
                {
                    if (moduleAdjacentRectangle.IsAdjacent(moduleLayoutRectangle))
                    {
                        moduleAdjacentRectangle.AdjacentRectangles.Add(moduleLayoutRectangle);
                        moduleLayoutRectangle.AdjacentRectangles.Add(moduleAdjacentRectangle);
                    }
                }
            }

            foreach (var moduleAdjacentRectangle in operationExecutingModule.Shape.AdjacentRectangles)
            {
                //The original module rectangle is replaced, and so it is no longer adjacent to anything:
                moduleAdjacentRectangle.AdjacentRectangles.Remove(operationExecutingModule.Shape);
            }
            operationExecutingModule.Shape.AdjacentRectangles.Clear();

            //The droplets in the module layout, have now had their associated rectangles placed on the board. 
            //Thus it is only neccessary to change their fluidtype, to get the correct output.

            operationExecutingModule.GetModuleLayout().ChangeOutputType(fluidType);
            operationExecutingModule.GetModuleLayout().EmptyRectangles.ForEach(rectangle => EmptyRectangles.Add(rectangle));
            ClearBoard(operationExecutingModule.Shape);
            operationExecutingModule.GetModuleLayout().OutputDroplets.ForEach(droplet => UpdateGridWithModulePlacement(droplet, droplet.Shape));
            placedModules.Remove(operationExecutingModule);

            return operationExecutingModule.GetModuleLayout().OutputDroplets;
            /*
            Droplet droplet = new Droplet(fluidType);
            Rectangle moduleRectangle = finishedOperation.boundModule.Shape;
            UpdateGridWithModulePlacement(droplet, moduleRectangle);
            FastTemplateReplace(moduleRectangle, droplet);
            placedModules.Remove(finishedOperation.boundModule);
            return droplet;
            */
        }

        private void FastTemplateReplace(Rectangle oldRectangle, Module replacingModule)
        {
            (Rectangle TopRectangle, Rectangle RightRectangle) = oldRectangle.SplitIntoSmallerRectangles(replacingModule);
            //All the adjacenecies from the old rectangle (moduleRectangle), should be removed, and added to the new rectangles.
            foreach (var adjacentRectangle in oldRectangle.AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(oldRectangle);
                if (adjacentRectangle.IsAdjacent(replacingModule.Shape))
                {
                    replacingModule.Shape.AdjacentRectangles.Add(adjacentRectangle);
                    adjacentRectangle.AdjacentRectangles.Add(replacingModule.Shape);
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
