using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using MoreLinq;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth, width;
        public HashSet<Module> placedModules        = new HashSet<Module>();
        public HashSet<Rectangle> EmptyRectangles   = new HashSet<Rectangle>();
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
            //Used when placing the module in any rectangle, blocks the routing.
            List<Rectangle> candidateRectangles = new List<Rectangle>();
            foreach (var rectangle in EmptyRectangles)
            {
                int Cost = RectangleCost(rectangle, module);
                if (rectangle.DoesRectangleFitInside(module.Shape) && Cost < bestFitScore)
                {
                    candidateRectangles.Add(rectangle);
                    if (DoesNotBlockRouteToAnyModuleOrEmptyRectangle(rectangle, module))
                    {
                        bestFitRectangle = rectangle;
                        bestFitScore = Cost;
                    }

                }
            }
            if (bestFitRectangle != null)
            {
                //Removes bestFitRectangle, hopefully in constant time
                PlaceModuleInRectangle(module, bestFitRectangle);
                return true;
            }
            else return PlaceBufferedModule(module, candidateRectangles);
        }

        public bool PlaceBufferedModule(Module module, List<Rectangle> candidateRectangles)
        {
            candidateRectangles.Sort((x, y) => RectangleCost(x, module) <= RectangleCost(y, module) ? 0 : 1);
            //The intention is that it should have a one wide buffer on each side, so that droplets always can be routed.
            Rectangle bufferedRectangle = new Rectangle(module.Shape.width + 2, module.Shape.height + 2);
            for (int i = 0; i < candidateRectangles.Count; i++)
            {
                Rectangle current = candidateRectangles[i];
                if (candidateRectangles[i].DoesRectangleFitInside(bufferedRectangle))
                {
                    return PlaceBufferedModuleInRectangle(module, bufferedRectangle, current);
                }
            }
            //If the module can't be placed, even with some buffer space, then it can't be placed at all:
            return false;
        }

        private bool PlaceBufferedModuleInRectangle(Module module, Rectangle bufferedRectangle, Rectangle current)
        {
            (Rectangle topRectangle, Rectangle rightRectangle) = current.SplitIntoSmallerRectangles(bufferedRectangle);
            EmptyRectangles.Remove(current);
            if (topRectangle != null) EmptyRectangles.Add(topRectangle);
            if (rightRectangle != null) EmptyRectangles.Add(rightRectangle);

            //The placed buffered rectangle is divided up into smaller empty rectangles, that can be used for routing.
            //This is done by first cutting a thin slice of the bottom off, and then the left. 
            //Because of the initial size of bufferedRectangle, PlaceModuleInRectangle will handle the top and right part.
            Rectangle lowerBufferingRectangle = new Rectangle(bufferedRectangle.width, 1, bufferedRectangle.x, bufferedRectangle.y);
            Rectangle remainingUpperRectangle = new Rectangle(bufferedRectangle.width, bufferedRectangle.height - 1, bufferedRectangle.x, bufferedRectangle.y + 1);
            bufferedRectangle.splitRectangleInTwo(lowerBufferingRectangle, remainingUpperRectangle);
            EmptyRectangles.Add(lowerBufferingRectangle);

            Rectangle leftBufferingRectangle = new Rectangle(1, remainingUpperRectangle.height, remainingUpperRectangle.x, remainingUpperRectangle.y);
            Rectangle remainingRightRectangle = new Rectangle(remainingUpperRectangle.width - 1, remainingUpperRectangle.height, remainingUpperRectangle.x + 1, remainingUpperRectangle.y);
            remainingUpperRectangle.splitRectangleInTwo(leftBufferingRectangle, remainingRightRectangle);
            EmptyRectangles.Add(leftBufferingRectangle);

            PlaceModuleInRectangle(module, remainingRightRectangle);
            return true;
        }

        /// <summary>
        /// Checks if, without this rectangle, it is possible to reach all modules on the board,
        /// and all empty rectangles.
        /// This is to ensure that it is always possible to route droplets between modules,
        /// and to ensure that there isn't any space on the board (an empty rectangle) that simply can't be used.
        /// 
        /// To construct the correct adjacency graph for the board, the module is temporarily placed and is the removed,
        /// giving the original board.
        /// 
        /// Checking that everything is connected is done using a Breadth first search, only moving between empty rectangles.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private bool DoesNotBlockRouteToAnyModuleOrEmptyRectangle(Rectangle rectangle, Module module)
        {
            //If the board is empty, the placement is legal iff it leaves at least 1 empty rectangle:
            if (EmptyRectangles.Count == 1 && placedModules.Count == 0) return (module.Shape.width != rectangle.width || module.Shape.height != rectangle.height);
            

            //The module is temporarily "placed" (but not really), to get the adjacency graph corresponding to the module being placed.
            //It is not really placed, as it would change EmptyRectangles, which is itterated over.
            (Rectangle emptyTopRectangle, Rectangle emptyRightRectangle) = rectangle.SplitIntoSmallerRectangles(module.Shape);
            int extraEmptyRectangles = ((emptyTopRectangle == null) ? 0 : 1) + ((emptyRightRectangle == null) ? 0 : 1) - 1; //-1 as the initial rectangle is removed.

            //The source empty rectangle for the search does not matter, as paths are symmetric:
            Rectangle randomEmptyRectangle = getEmptyAdjacentRectangle(module.Shape);
            if (randomEmptyRectangle == null) {
                //There were only one empty rectangle initally, and placing the module in it, filled the rectangle:

                //The placed module is the removed, leaving the original board.
                MergeToGetOriginalRectangle(module, rectangle, emptyTopRectangle, emptyRightRectangle);
                return false;
            }


            HashSet<Rectangle> visitedEmptyRectangles = new HashSet<Rectangle>() { randomEmptyRectangle };
            HashSet<Rectangle> connectedModuleRectangles = new HashSet<Rectangle>();
            Queue<Rectangle> emptyRectanglesToVisit = new Queue<Rectangle>();
            emptyRectanglesToVisit.Enqueue(randomEmptyRectangle);

            while (emptyRectanglesToVisit.Count > 0)
            {
                Rectangle currentEmptyRectangle = emptyRectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in currentEmptyRectangle.AdjacentRectangles) {
                    if (adjacentRectangle == rectangle) throw new Exception("Logic error: no rectangles should currently be adjacent to this rectangle");
                    //if it is an empty rectangle, it should be visited:
                    if (adjacentRectangle.isEmpty && visitedEmptyRectangles.Add(adjacentRectangle))
                        emptyRectanglesToVisit.Enqueue(adjacentRectangle);
                    else if (!adjacentRectangle.isEmpty) 
                        connectedModuleRectangles.Add(adjacentRectangle);                    
                }
            }

            //The placed module is the removed, leaving the original board.
            MergeToGetOriginalRectangle(module, rectangle, emptyTopRectangle, emptyRightRectangle);

            Schedule.checkAdjacencyMatrixCorrectness(this);
            bool visitsEverything = VisitsAllModulesAndEmptyRectangles(extraEmptyRectangles, 1, visitedEmptyRectangles, connectedModuleRectangles);
            return visitsEverything;
        }

        private Rectangle getEmptyAdjacentRectangle(Rectangle rectangle)
        {
            Rectangle randomEmptyRectangle = null;
            foreach (var adjacentRectangle in rectangle.AdjacentRectangles)
            {
                if (adjacentRectangle.isEmpty)
                {
                    randomEmptyRectangle = adjacentRectangle;
                    break;
                }
            }

            return randomEmptyRectangle;
        }

        private bool VisitsAllModulesAndEmptyRectangles(int extraEmptyRectangles, int extraPlacedModules, HashSet<Rectangle> visitedEmptyRectangles, HashSet<Rectangle> connectedModuleRectangles)
        {
            return (connectedModuleRectangles.Count == placedModules.Count + extraPlacedModules && visitedEmptyRectangles.Count == EmptyRectangles.Count + extraEmptyRectangles);
        }

        private HashSet<Rectangle> GetSetDifference(HashSet<Rectangle> set1, HashSet<Rectangle> set2)
        {
            HashSet<Rectangle> differenceSet = new HashSet<Rectangle>();
            foreach (var rectangle in set1)
                if (!set2.Contains(rectangle))
                    differenceSet.Add(rectangle);
            foreach (var rectangle in set2)
                if (!set1.Contains(rectangle))
                    differenceSet.Add(rectangle);
            return differenceSet;
        }

        private void MergeToGetOriginalRectangle(Module module, Rectangle originalRectangle, Rectangle emptyTopRectangle, Rectangle emptyRightRectangle)
        {
            //Dummy rectangles to avoid constant null checks:
            //The y=-5 position is to create unique hash values
            if (emptyTopRectangle == null)   emptyTopRectangle = new Rectangle(0, 0, 0, -5);
            if (emptyRightRectangle == null) emptyRightRectangle = new Rectangle(0, 0, 1, -5);


            /*
            //The module is the removed, leaving the original board. It is done this way, instead of using the mergin method,
            //to ensure that the rectangle doesn't merge with other rectangles, instead of the top and right rectangle:
            
            int originalWidth  = module.Shape.width  + emptyRightRectangle.width;
            int originalHeight = module.Shape.height + emptyTopRectangle.height ;
            Rectangle mergedRectangle = new Rectangle(originalWidth, originalHeight, module.Shape.x, module.Shape.y);

            mergedRectangle.AdjacentRectangles.UnionWith(module.Shape.AdjacentRectangles);
            mergedRectangle.AdjacentRectangles.UnionWith(emptyTopRectangle.AdjacentRectangles);
            mergedRectangle.AdjacentRectangles.UnionWith(emptyRightRectangle.AdjacentRectangles);
            */


            foreach (var adjacentRectangle in originalRectangle.AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(module.Shape);
                adjacentRectangle.AdjacentRectangles.Remove(emptyTopRectangle);
                adjacentRectangle.AdjacentRectangles.Remove(emptyRightRectangle);
                //This must be latter than the removes, in the case that the module.shape has the same size as the merged rectangle, 
                //curtesy of the equals methods.
                adjacentRectangle.AdjacentRectangles.Add(originalRectangle);
            }
            module.Shape.AdjacentRectangles.Clear();

            ClearBoard(originalRectangle);
        }

        public (Rectangle, Rectangle) PlaceModuleInRectangle(Module module, Rectangle bestFitRectangle)
        {
            EmptyRectangles.Remove(bestFitRectangle);
            UpdateGridWithModulePlacement(module, bestFitRectangle);            
            (Rectangle topRectangle, Rectangle rightRectangle) = bestFitRectangle.SplitIntoSmallerRectangles(module.Shape);
            if (topRectangle != null) EmptyRectangles.Add(topRectangle);
            if (rightRectangle != null) EmptyRectangles.Add(rightRectangle);
            return (topRectangle, rightRectangle);
        }

        public void FastTemplateRemove(Module module)
        {
            placedModules.Remove(module);
            //All dependencies on the rectangle from the module, should be moved to the new empty rectangle.
            //It is easier to just create a new rectangle for the module:
            Rectangle newModuleRectangle = new Rectangle(module.Shape);
            Rectangle emptyRectangle = module.Shape;
            module.Shape = newModuleRectangle;

            newModuleRectangle.isEmpty = false;
            EmptyRectangles.Add(emptyRectangle);
            emptyRectangle.isEmpty = true;

            ClearBoard(emptyRectangle);
            emptyRectangle.MergeWithOtherRectangles(this);
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

        public List<Droplet> replaceWithDroplets(FluidBlock finishedOperation, BoardFluid fluidType)
        {
            Module operationExecutingModule = finishedOperation.boundModule;
            //Checks for each pair of adjacent rectangle to the module on the board, and the rectangles in the modules layout,
            //if they are adjacent -> if so, it makes them adjacent.
            List<Rectangle> allRectangles = operationExecutingModule.GetOutputLayout().getAllRectanglesIncludingDroplets();

            //Copied, as sets work in mysterious ways,
            HashSet<Rectangle> adjacentRectangles = new HashSet<Rectangle>(operationExecutingModule.Shape.AdjacentRectangles);

            foreach (var moduleAdjacentRectangle in adjacentRectangles)
            {
                //They are no longer adjacent.
                moduleAdjacentRectangle.AdjacentRectangles.Remove(operationExecutingModule.Shape); 
                foreach (var moduleLayoutRectangle in allRectangles)
                {
                    if (moduleAdjacentRectangle.IsAdjacent(moduleLayoutRectangle))
                    {
                        moduleAdjacentRectangle.AdjacentRectangles.Add(moduleLayoutRectangle);
                        moduleLayoutRectangle.AdjacentRectangles.Add(moduleAdjacentRectangle);
                    }
                }
            }
            operationExecutingModule.Shape.AdjacentRectangles.Clear();

            //The droplets in the module layout, have now had their associated rectangles placed on the board. 
            //Thus it is only neccessary to change their fluidtype, to get the correct output.

            placedModules.Remove(operationExecutingModule);
            ClearBoard(operationExecutingModule.Shape);
            operationExecutingModule.GetOutputLayout().ChangeFluidType(fluidType);
            operationExecutingModule.GetOutputLayout().EmptyRectangles.ForEach(rectangle => EmptyRectangles.Add(rectangle));
            operationExecutingModule.GetOutputLayout().Droplets.ForEach(droplet => UpdateGridWithModulePlacement(droplet, droplet.Shape));

            return operationExecutingModule.GetOutputLayout().Droplets;
        }

        /*
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
        } */

        public Board Copy()
        {
            Board board = new Board(width, heigth);
            board.EmptyRectangles.Clear();
            foreach (var module in placedModules) board.placedModules.Add(module);
            foreach (var rectangle in EmptyRectangles) board.EmptyRectangles.Add(rectangle);
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
