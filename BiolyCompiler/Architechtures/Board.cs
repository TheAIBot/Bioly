using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Modules.RectangleSides;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using MoreLinq;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth;
        public int width;
        public Dictionary<Module, Module> PlacedModules           = new Dictionary<Module, Module>();
        public Dictionary<Rectangle, Rectangle> EmptyRectangles   = new Dictionary<Rectangle, Rectangle>();
        public Dictionary<string,BoardFluid> fluids = new Dictionary<string,BoardFluid>();
        public Module[,] grid;


        public Board(int width, int heigth)
        {
            this.width  = width;
            this.heigth = heigth;
            this.grid = new Module[width,heigth];
            Rectangle emptyRectangle = new Rectangle(width, heigth);
            EmptyRectangles.Add(emptyRectangle, emptyRectangle);
        }


        /// <summary>
        /// Places a given module on the board, if it is deemed possible. 
        /// The algorithm is based on the fast template placement algorithm from "Fast template placement for reconfigurable computing systems".
        /// In essence, it divides the board into rectangles, finds the smallest empty rectangle the module can fit in,
        /// and places it there: it then updates the rectangles on the board.
        /// 
        /// It is not a complete implementation, and it has some modifications compared to the algorithm described in the article.
        /// If neccessary, optimizations described in the article, can be implemented for better performance.
        /// 
        /// Note that a module will not be placed, so that all routes to any module on the board, gets blocked.
        /// </summary>
        /// <param name="module">The module that should be placed on the board</param>
        /// <returns>true if the module could be placed on the board, else false.</returns>
        public bool FastTemplatePlace(Module module)
        {
            //2d range trees can be used here instead of a linear search, for better performance.
            Rectangle bestFitRectangle = null;
            int bestFitScore = Int32.MaxValue;
            //Used when placing the module in any rectangle, blocks the routing.
            List<Rectangle> candidateBufferedRectangles = new List<Rectangle>();
            foreach (var rectangle in EmptyRectangles.Values)
            {
                int Cost = RectangleCost(rectangle, module);
                if (rectangle.DoesRectangleFitInside(module.Shape) && Cost < bestFitScore)
                {
                    candidateBufferedRectangles.Add(rectangle);
                    if (DoesNotBlockRouteToAnyModuleOrEmptyRectangle(rectangle, module, EmptyRectangles, PlacedModules))
                    {
                        bestFitRectangle = rectangle;
                        bestFitScore = Cost;
                    }

                }
            }
            //If a rectangle where the module can fit inside, was found, it can be placed in the best of those rectangles.
            //Else, it might be neccessary to "buffer" the module, by placing empty space around it, 
            //so that it doesn't block routing to other modules.
            if (bestFitRectangle != null){
                PlaceModuleInRectangle(module, bestFitRectangle, this);
                return true;
            }
            else return PlaceBufferedModule(module, candidateBufferedRectangles);
        }


        public bool PlaceBufferedModule(Module module, List<Rectangle> candidateRectangles)
        {
            //It is neccessary to buffer the module, so that droplets can be routed around it.
            //First it will try with a smaller buffering area just above the module,
            //and if this does not suffice, it will try with buffers around the whole module.
            candidateRectangles.Sort((x, y) => RectangleCost(x, module) <= RectangleCost(y, module) ? 0 : 1);
            Rectangle bufferedRectangle;
            
            //bufferedRectangle = new Rectangle(module.Shape.width, module.Shape.height + 1);
            //for (int i = 0; i < candidateRectangles.Count; i++)
            //{
            //    Rectangle current = candidateRectangles[i];
            //    if (candidateRectangles[i].DoesRectangleFitInside(bufferedRectangle))
            //    {
            //        (bool couldBePlaced, Rectangle newCurrentRectangle) = PlaceBottomBufferedModuleInRectangle(module, current);
            //        if (couldBePlaced) return true;
            //        else candidateRectangles[i] = newCurrentRectangle; //Necessary, as current has been replaced internally in the system with newCurrentRectangle.

            //    }
            //}
            //DebugTools.checkAdjacencyMatrixCorrectness(this);
            
            //Bigger buffer in the case it failed:

            //The intention is that it should have a one wide buffer on each side,
            //so that droplets always can be routed around the module.
            //This would make the rectangles unable to block any routing between modules.
            bufferedRectangle = new Rectangle(module.Shape.width + 2, module.Shape.height + 2);
            for (int i = 0; i < candidateRectangles.Count; i++)
            {
                Rectangle current = candidateRectangles[i];
                if (candidateRectangles[i].DoesRectangleFitInside(bufferedRectangle))
                {
                    return PlaceCompletlyBufferedModuleInRectangle(module, current);
                }
            }
            DebugTools.checkAdjacencyMatrixCorrectness(this);
            //If the module can't be placed, even with some buffer space, then it can't be placed at all:
            return false;
        }


        //public (bool, Rectangle) PlaceBottomBufferedModuleInRectangle(Module module, Rectangle current)
        //{
        //    Rectangle bufferedRectangle = new Rectangle(module.Shape.width, module.Shape.height + 1);
        //    //It reserves/places the area for the whole buffered rectangle
        //    (Rectangle topRectangle, Rectangle rightRectangle) = current.SplitIntoSmallerRectangles(bufferedRectangle);
        //    EmptyRectangles.Remove(current);
        //    if (topRectangle != null) EmptyRectangles.Add(topRectangle);
        //    if (rightRectangle != null) EmptyRectangles.Add(rightRectangle);

        //    //The placed buffered rectangle is divided up into smaller empty rectangles, that can be used for routing.
        //    //Here a thin slice is cut off from the bottom, for the purpose of routing
        //    Rectangle lowerBufferingRectangle = new Rectangle(bufferedRectangle.width, 1, bufferedRectangle.x, bufferedRectangle.y);
        //    Rectangle remainingUpperRectangle = new Rectangle(bufferedRectangle.width, bufferedRectangle.height - 1, bufferedRectangle.x, bufferedRectangle.y + 1);
        //    bufferedRectangle.splitRectangleInTwo(lowerBufferingRectangle, remainingUpperRectangle);
        //    EmptyRectangles.Add(remainingUpperRectangle);
        //    EmptyRectangles.Add(lowerBufferingRectangle);
        //    //It needs to be checked, if with the buffer, one can still route to all other rectangles.
        //    //If not, then one should fail.
        //    if (DoesNotBlockRouteToAnyModuleOrEmptyRectangle(remainingUpperRectangle, module, EmptyRectangles, PlacedModules))
        //    {
        //        PlaceModuleInRectangle(module, remainingUpperRectangle);
        //        return (true, null);
        //    }
        //    else
        //    {
        //        //Everything is returned to the same state as before:
        //        //This must be done in a certain order, to avoid error cases, where one do not return to the original board.
        //        Rectangle intermediateCurrent = remainingUpperRectangle.MergeWithRectangle(RectangleSide.Bottom, lowerBufferingRectangle);
        //        EmptyRectangles.Remove(remainingUpperRectangle);
        //        EmptyRectangles.Remove(lowerBufferingRectangle);

        //        //A lot of conditionals, depending on the original splitting of topRectangle and rightRectangle:
        //        if (topRectangle != null)
        //        {
        //            (RectangleSide side, bool canMerge) = intermediateCurrent.CanMerge(topRectangle);
        //            if (canMerge)
        //            {
        //                intermediateCurrent = intermediateCurrent.MergeWithRectangle(side, topRectangle);
        //                if (rightRectangle != null)
        //                {
        //                    //It should then be able to merge
        //                    (RectangleSide secondSide, bool canTotallyMerge) = intermediateCurrent.CanMerge(rightRectangle);
        //                    if (!canTotallyMerge) throw new internalRuntimeException("Logic error");
        //                    intermediateCurrent = intermediateCurrent.MergeWithRectangle(secondSide, rightRectangle);

        //                }
        //            }
        //            else
        //            { //Then the right rectangle must exists, and it can be merged with first
        //                (RectangleSide secondSide, bool canTotallyMerge) = intermediateCurrent.CanMerge(rightRectangle);
        //                if (!canTotallyMerge) throw new internalRuntimeException("Logic error");
        //                intermediateCurrent = intermediateCurrent.MergeWithRectangle(secondSide, rightRectangle);
        //                intermediateCurrent = intermediateCurrent.MergeWithRectangle(side, topRectangle);
        //            }
        //        }
        //        else if (rightRectangle != null)
        //        {
        //            //It should then be able to merge
        //            (RectangleSide secondSide, bool canTotallyMerge) = intermediateCurrent.CanMerge(rightRectangle);
        //            if (!canTotallyMerge) throw new internalRuntimeException("Logic error");
        //        }

        //        if (topRectangle != null)   EmptyRectangles.Remove(topRectangle);
        //        if (rightRectangle != null) EmptyRectangles.Remove(rightRectangle);
        //        EmptyRectangles.Add(intermediateCurrent);
        //        return (false, intermediateCurrent);
        //    }


        //}

        public bool PlaceCompletlyBufferedModuleInRectangle(Module module, Rectangle current)
        {
            Rectangle bufferedRectangle = new Rectangle(module.Shape.width + 2, module.Shape.height + 2);
            //It reserves/places the area for the whole buffered rectangle
            (Rectangle topRectangle, Rectangle rightRectangle) = current.SplitIntoSmallerRectangles(bufferedRectangle);
            EmptyRectangles.Remove(current);
            if (topRectangle != null) EmptyRectangles.Add(topRectangle, topRectangle);
            if (rightRectangle != null) EmptyRectangles.Add(rightRectangle, rightRectangle);

            //The placed buffered rectangle is divided up into smaller empty rectangles, that can be used for routing.
            //This is done by first cutting a thin slice of the bottom off, and then a thin slice of the left. 
            //Because of the initial size of bufferedRectangle, PlaceModuleInRectangle will handle the top and right part.
            Rectangle lowerBufferingRectangle = new Rectangle(bufferedRectangle.width, 1, bufferedRectangle.x, bufferedRectangle.y);
            Rectangle remainingUpperRectangle = new Rectangle(bufferedRectangle.width, bufferedRectangle.height - 1, bufferedRectangle.x, bufferedRectangle.y + 1);
            bufferedRectangle.splitRectangleInTwo(lowerBufferingRectangle, remainingUpperRectangle);
            EmptyRectangles.Add(lowerBufferingRectangle, lowerBufferingRectangle);

            Rectangle leftBufferingRectangle = new Rectangle(1, remainingUpperRectangle.height, remainingUpperRectangle.x, remainingUpperRectangle.y);
            Rectangle remainingRightRectangle = new Rectangle(remainingUpperRectangle.width - 1, remainingUpperRectangle.height, remainingUpperRectangle.x + 1, remainingUpperRectangle.y);
            remainingUpperRectangle.splitRectangleInTwo(leftBufferingRectangle, remainingRightRectangle);
            EmptyRectangles.Add(leftBufferingRectangle, leftBufferingRectangle);

            PlaceModuleInRectangle(module, remainingRightRectangle, this);
            return true;
        }

        /// <summary>
        /// Checks if, without this rectangle, it is possible to reach all modules on the board,
        /// and all empty rectangles, without having to go over fields reserved for the modules.
        /// This is to ensure that it is always possible to route droplets between modules,
        /// and to ensure that there isn't any space on the board (an empty rectangle) that simply can't be used.
        /// 
        /// To construct the correct adjacency graph for the board, the module is temporarily placed and is then removed,
        /// giving the original board.
        /// 
        /// Checking that everything is connected is done using a Breadth first search, only moving between empty rectangles.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="module"></param>
        /// <returns>true iff it is still possible to reach all modules and empty rectangles on the board</returns>
        public static bool DoesNotBlockRouteToAnyModuleOrEmptyRectangle(Rectangle rectangle, Module module, Dictionary<Rectangle,Rectangle> emptyRectangles, Dictionary<Module, Module> placedModules)
        {
            //If the board is empty, the placement is legal iff it leaves at least 1 empty rectangle:
            if (emptyRectangles.Count == 1 && placedModules.Count == 0) return (module.Shape.width != rectangle.width || module.Shape.height != rectangle.height);
            

            //The module is temporarily "placed" (but not really), to get the adjacency graph corresponding to the module being placed.
            //It is not really placed, as it would change EmptyRectangles, which is itterated over. Trust me, it would crash everything - Jesper.
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


            //Breadth first search, finding all the empty rectangles and placed modules that can be visited.
            HashSet<Rectangle> visitedEmptyRectangles = new HashSet<Rectangle>() { randomEmptyRectangle };
            HashSet<Rectangle> connectedModuleRectangles = new HashSet<Rectangle>();
            Queue<Rectangle> emptyRectanglesToVisit = new Queue<Rectangle>();
            emptyRectanglesToVisit.Enqueue(randomEmptyRectangle);

            while (emptyRectanglesToVisit.Count > 0)
            {
                Rectangle currentEmptyRectangle = emptyRectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in currentEmptyRectangle.AdjacentRectangles) {
                    if (adjacentRectangle == rectangle) throw new InternalRuntimeException("Logic error: no rectangles should currently be adjacent to this rectangle");
                    //if it is an empty rectangle, it should be visited:
                    if (adjacentRectangle.isEmpty && visitedEmptyRectangles.Add(adjacentRectangle))
                        emptyRectanglesToVisit.Enqueue(adjacentRectangle);
                    else if (!adjacentRectangle.isEmpty) 
                        connectedModuleRectangles.Add(adjacentRectangle);                    
                }
            }

            //The placed module is the removed, leaving the original board.
            MergeToGetOriginalRectangle(module, rectangle, emptyTopRectangle, emptyRightRectangle);

            //DebugTools.checkAdjacencyMatrixCorrectness(this);
            bool visitsEverything = VisitsAllModulesAndEmptyRectangles(extraEmptyRectangles, 1, visitedEmptyRectangles, connectedModuleRectangles, emptyRectangles, placedModules);
            return visitsEverything;
        }

        public static bool DoesNotBlockConnectionToSourceEmptyRectangles(Droplet dropletInput, Dictionary<Rectangle, Rectangle> outsideEmptyRectangles, Dictionary<Rectangle, Rectangle> layoutEmptyRectangles)
        {
            return DoesNotBlockConnectionToSourceEmptyRectangles(dropletInput, outsideEmptyRectangles.Values.ToHashSet(), layoutEmptyRectangles.Values.ToHashSet());
        }

        public static bool DoesNotBlockConnectionToSourceEmptyRectangles(Droplet dropletInput, HashSet<Rectangle> outsideEmptyRectangles, HashSet<Rectangle> layoutEmptyRectangles)
        {
            //Breadth first search, finding all the empty rectangles that can be visited.
            HashSet<Rectangle> visitedEmptyRectangles = new HashSet<Rectangle>(outsideEmptyRectangles);
            Queue<Rectangle> emptyRectanglesToVisit = new Queue<Rectangle>();
            foreach (var rectangle in outsideEmptyRectangles)
                emptyRectanglesToVisit.Enqueue(rectangle);

            while (emptyRectanglesToVisit.Count > 0)
            {
                Rectangle current = emptyRectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in current.AdjacentRectangles)
                {
                    //We do not care for rectangles, that are not inside the module layout.
                    if ( layoutEmptyRectangles.Contains(adjacentRectangle) &&
                        !visitedEmptyRectangles.Contains(adjacentRectangle))
                    {
                        //If dropletInput is placed, it is not possible to go through it, 
                        //so it is not added to the nodes to visit:
                        visitedEmptyRectangles.Add(adjacentRectangle);
                        if (dropletInput.Shape != adjacentRectangle)
                        {
                            emptyRectanglesToVisit.Enqueue(adjacentRectangle);
                        }
                    }
                }
            }

            bool hasAllEmptyRectanglesBeenVisited = layoutEmptyRectangles.IsSubsetOf(visitedEmptyRectangles);
            return hasAllEmptyRectanglesBeenVisited;
        }
        
        /// <summary>
        /// Return an empty rectangle adjacent to the given rectangle: and null if such a rectangle does not exist.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private static Rectangle getEmptyAdjacentRectangle(Rectangle rectangle)
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

        /// <summary>
        /// Compares the size of the given sets, to find if exactely the right rectangles and modules have been visisted.
        /// It returns true if this is the case, else false.
        /// </summary>
        /// <param name="extraEmptyRectangles"></param>
        /// <param name="extraPlacedModules"></param>
        /// <param name="visitedEmptyRectangles"></param>
        /// <param name="connectedModuleRectangles"></param>
        /// <param name="originalEmptyRectangles"></param>
        /// <param name="originalPlacedModules"></param>
        /// <returns></returns>
        private static bool VisitsAllModulesAndEmptyRectangles(int extraEmptyRectangles, int extraPlacedModules, HashSet<Rectangle> visitedEmptyRectangles, 
                                                               HashSet<Rectangle> connectedModuleRectangles, Dictionary<Rectangle, Rectangle> originalEmptyRectangles, Dictionary<Module, Module> originalPlacedModules)
        {
            return (connectedModuleRectangles.Count == originalPlacedModules.Count   + extraPlacedModules && 
                    visitedEmptyRectangles.Count    == originalEmptyRectangles.Count + extraEmptyRectangles);
        }

        /// <summary>
        /// Returns the rectangles that are only present in one of the two sets given.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
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

        private static void MergeToGetOriginalRectangle(Module module, Rectangle originalRectangle, Rectangle emptyTopRectangle, Rectangle emptyRightRectangle)
        {
            //Dummy rectangles to avoid constant null checks:
            //The y=-5 position is to create unique hash values
            if (emptyTopRectangle == null)   emptyTopRectangle = new Rectangle(0, 0, 0, -5);
            if (emptyRightRectangle == null) emptyRightRectangle = new Rectangle(0, 0, 1, -5);
            

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

            //ClearBoard(originalRectangle);
        }

        public void PlaceModuleInRectangle(Module module, Rectangle bestFitRectangle, Board board)
        {
            EmptyRectangles.Remove(bestFitRectangle);
            UpdateGridWithModulePlacement(module, bestFitRectangle);            
            (Rectangle topRectangle, Rectangle rightRectangle) = bestFitRectangle.SplitIntoSmallerRectangles(module.Shape);
            if (topRectangle != null) {
                EmptyRectangles.Add(topRectangle, topRectangle);
            }
            if (rightRectangle != null) {
                EmptyRectangles.Add(rightRectangle, rightRectangle);
            }

            if (topRectangle != null)
            {
                topRectangle.MergeWithOtherRectangles(board);
            }
            if (rightRectangle != null)
            {
                //In the case that adjacentRectangle have been modified in the above merge, 
                //we must ensure that the rectangle is actually placed on the board:
                if (board.EmptyRectangles.TryGetValue(rightRectangle, out Rectangle rightRectangleInDictionary))
                {
                    rightRectangleInDictionary.MergeWithOtherRectangles(board);
                }
            }
            
        }

        public void FastTemplateRemove(Module module)
        {
            PlacedModules.Remove(module);
            //All dependencies on the rectangle from the module, should be moved to the new empty rectangle.
            //It is easier to just create a new rectangle for the module:
            Rectangle newModuleRectangle = new Rectangle(module.Shape);
            Rectangle emptyRectangle = module.Shape;
            module.Shape = newModuleRectangle;

            newModuleRectangle.isEmpty = false;
            EmptyRectangles.Add(emptyRectangle, emptyRectangle);
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

        public void UpdateGridAtGivenLocation(Module module, Rectangle rectangleToPlaceAt)
        {
            for (int i = 0; i < module.Shape.width; i++)
            {
                for (int j = 0; j < module.Shape.height; j++)
                {
                    grid[i + rectangleToPlaceAt.x, j + rectangleToPlaceAt.y] = module;
                }
            }
        }

        public void UpdateGridWithModulePlacement(Module module, Rectangle rectangleToPlaceAt)
        {
            module.Shape.PlaceAt(rectangleToPlaceAt.x, rectangleToPlaceAt.y);
            UpdateGridAtGivenLocation(module, rectangleToPlaceAt);
            PlacedModules.Add(module, module);
        }
        

        private int RectangleCost(Rectangle rectangle, Module module)
        {
            return Math.Abs(rectangle.GetArea() - module.Shape.GetArea());
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
            Module operationExecutingModule = finishedOperation.BoundModule;
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

            PlacedModules.Remove(operationExecutingModule);
            ClearBoard(operationExecutingModule.Shape);
            operationExecutingModule.GetOutputLayout().ChangeFluidType(fluidType);
            operationExecutingModule.GetOutputLayout().EmptyRectangles.ForEach(rectangle => EmptyRectangles.Add(rectangle, rectangle));
            operationExecutingModule.GetOutputLayout().Droplets.ForEach(droplet => UpdateGridWithModulePlacement(droplet, droplet.Shape));

            return operationExecutingModule.GetOutputLayout().Droplets;
        }

        public Board Copy()
        {
            Board board = new Board(width, heigth);
            board.EmptyRectangles.Clear();
            foreach (var module in PlacedModules.Values) board.PlacedModules.Add(module, module);
            foreach (var rectangle in EmptyRectangles.Values) board.EmptyRectangles.Add(rectangle, rectangle);
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
