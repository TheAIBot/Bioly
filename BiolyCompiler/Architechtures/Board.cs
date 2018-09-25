using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using MoreLinq;
using System.Linq;
using System.Diagnostics;

namespace BiolyCompiler.Architechtures
{
    public class Board
    {
        //Dummy class for now.
        public int heigth;
        public int width;
        public Dictionary<Module, Module> PlacedModules = new Dictionary<Module, Module>();
        public Dictionary<Rectangle, Rectangle> EmptyRectangles = new Dictionary<Rectangle, Rectangle>();
        public Dictionary<string, BoardFluid> fluids = new Dictionary<string, BoardFluid>();
        public Module[,] grid;


        public Board(int width, int heigth)
        {
            this.width = width;
            this.heigth = heigth;
            this.grid = new Module[width, heigth];
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
            return PlaceBufferedModule(module);
        }


        public bool PlaceBufferedModule(Module module)
        {
            var bufferConfigurations = new (bool left, bool right, bool top, bool bottom)[]
{
                (false, false, false, false),
                (false, false, false, true ),
                //(false, false, true , false),
                //(false, true , false, false),
                (true , false, false, false),

                //(false, false, true , true ),
                //(false, true , true , false),
                //(true , true , false, false),
                (true , false, false, true ),
                //(false, true , false, true ),
                //(true , false, true , false),

                //(false, true , true , true ),
                //(true , true , true , false),
                //(true , true , false, true ),
                //(true , false, true , true ),

                //(true , true , true , true ),
            };

            List<Rectangle> sortedRectangles = new List<Rectangle>();
            foreach (var emptyRect in EmptyRectangles)
            {
                if (emptyRect.Key.DoesRectangleFitInside(module.Shape))
                {
                    sortedRectangles.Add(emptyRect.Key);
                }
            }
            sortedRectangles.Sort((x, y) => x.GetArea() - y.GetArea());


            foreach (Rectangle rectangle in sortedRectangles)
            {
                foreach (var useBuffer in bufferConfigurations)
                {
                    int bufferWidth = module.Shape.width + (useBuffer.left ? 1 : 0) + (useBuffer.right ? 1 : 0);
                    int bufferHeight = module.Shape.height + (useBuffer.top ? 1 : 0) + (useBuffer.bottom ? 1 : 0);

                    if (!rectangle.DoesRectangleFitInside(bufferWidth, bufferHeight))
                    {
                        continue;
                    }

                    //List<Rectangle> usedModules = PlacedModules.Select(x => x.Key.Shape).ToList();
                    //usedModules.Add(module.Shape);
                    //usedModules.AddRange(EmptyRectangles.Select(x => x.Key));
                    //RectangleIntMap(usedModules, width, heigth);
                    //Debug.WriteLine(this.print(PlacedModules.Select(x => x.Key).ToList()));
                    var bufferRectangles = CreateBufferedModuleRectangles(module, rectangle, useBuffer.left, useBuffer.right, useBuffer.top, useBuffer.bottom);
                    if (DoesNotBlockRouteToAnyModuleOrEmptyRectangle(rectangle, bufferRectangles.all, bufferRectangles.center, module, EmptyRectangles, PlacedModules))
                    {
                        //Replace module rectangle with one that has the correct position on the board
                        module.Shape = bufferRectangles.center;
                        module.Shape.isEmpty = false;

                        //Place the buffered module on theboard
                        Rectangle.ReplaceRectangles(rectangle, bufferRectangles.all);

                        //This reactangle has been replaced is therefore no longer a part of the board
                        EmptyRectangles.Remove(rectangle);

                        //The module has now been placed on the board so mark it on the map
                        UpdateGridAtGivenLocation(module, module.Shape);
                        PlacedModules.Add(module, module);

                        //Add all new rectangles except the module rectangle to the list of empty rectangles as 
                        //they have been added to the board
                        Rectangle[] newEmptyRectangles = bufferRectangles.all.Where(x => x.isEmpty).ToArray();
                        newEmptyRectangles.ForEach(x => EmptyRectangles.Add(x, x));

                        //Now try and refactor the empty rectangles
                        RectangleOptimizations.OptimizeRectangles(this, newEmptyRectangles);

                        return true;
                    }
                }
            }

            return false;









            ////It is neccessary to buffer the module, so that droplets can be routed around it.
            ////First it will try with a smaller buffering area just above the module,
            ////and if this does not suffice, it will try with buffers around the whole module.
            //candidateRectangles.Sort((x, y) => RectangleCost(x, module) <= RectangleCost(y, module) ? 0 : 1);



            ////The intention is that it should have a one wide buffer on each side,
            ////so that droplets always can be routed around the module.
            ////This would make the rectangles unable to block any routing between modules.
            //Rectangle bufferedRectangle = new Rectangle(module.Shape.width + 2, module.Shape.height + 2);
            //for (int i = 0; i < candidateRectangles.Count; i++)
            //{
            //    Rectangle current = candidateRectangles[i];
            //    //Find rectangle that can fit the buffered module
            //    if (current.DoesRectangleFitInside(bufferedRectangle))
            //    {
            //        return PlaceCompletlyBufferedModuleInRectangle(module, current);
            //    }
            //}
            //DebugTools.checkAdjacencyMatrixCorrectness(this);
            ////If the module can't be placed, even with some buffer space, then it can't be placed at all:
            //return false;
        }

        public static void RectangleIntMap(List<Rectangle> rectangles, int width, int height)
        {
            int[][] map = new int[height][];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new int[width];
            }
            int index = 1;
            foreach (Rectangle rectangle in rectangles)
            {
                for (int y = rectangle.y; y < rectangle.height + rectangle.y; y++)
                {
                    for (int x = rectangle.x; x < rectangle.width + rectangle.x; x++)
                    {
                        map[y][x] = index;
                    }
                }
                index++;
            }

            Debug.WriteLine(String.Join(Environment.NewLine, map.Select(x => String.Join(", ", x.Select(z => (char)(z + 33))))) + Environment.NewLine + Environment.NewLine);
        }

        public static (Rectangle[] all, Rectangle center) CreateBufferedModuleRectangles(Module module, Rectangle bigRectangle, bool leftBuffer, bool rightBuffer, bool topBuffer, bool bottomBuffer)
        {
            int bufferWidth = module.Shape.width + (leftBuffer ? 1 : 0) + (rightBuffer ? 1 : 0);
            int bufferHeight = module.Shape.height + (topBuffer ? 1 : 0) + (bottomBuffer ? 1 : 0);

            List<Rectangle> rectangles = new List<Rectangle>();

            Rectangle bufferedRectangle = new Rectangle(bufferWidth, bufferHeight);
            var splittedRectangle = Rectangle.SplitIntoSmallerRectangles(bigRectangle, bufferedRectangle);
            if (bufferHeight < bigRectangle.height)
            {
                rectangles.Add(splittedRectangle.top);
            }
            if (bufferWidth < bigRectangle.width)
            {
                rectangles.Add(splittedRectangle.right);
            }

            if (leftBuffer)
            {
                rectangles.Add(new Rectangle(1, bufferHeight - (topBuffer ? 1 : 0) - (bottomBuffer ? 1 : 0), splittedRectangle.newSmaller.x, splittedRectangle.newSmaller.y + (bottomBuffer ? 1 : 0)));
            }
            if (rightBuffer)
            {
                rectangles.Add(new Rectangle(1, bufferHeight - (topBuffer ? 1 : 0) - (bottomBuffer ? 1 : 0), splittedRectangle.newSmaller.getRightmostXPosition() - 1 + (leftBuffer ? 1 : 0), splittedRectangle.newSmaller.y + (bottomBuffer ? 1 : 0)));
            }
            if (topBuffer)
            {
                rectangles.Add(new Rectangle(bufferWidth, 1, splittedRectangle.newSmaller.x, splittedRectangle.newSmaller.getTopmostYPosition() - 1 + (bottomBuffer ? 1 : 0)));
            }
            if (bottomBuffer)
            {
                rectangles.Add(new Rectangle(bufferWidth, 1, splittedRectangle.newSmaller.x, splittedRectangle.newSmaller.y));
            }

            Rectangle centerRectangle = new Rectangle(module.Shape.width, module.Shape.height, splittedRectangle.newSmaller.x + (leftBuffer ? 1 : 0), splittedRectangle.newSmaller.y + (bottomBuffer ? 1 : 0));
            centerRectangle.isEmpty = false;
            rectangles.Add(centerRectangle);

            return (rectangles.ToArray(), centerRectangle);
        }

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
        public static bool DoesNotBlockRouteToAnyModuleOrEmptyRectangle(Rectangle rectangle, Module module, Dictionary<Rectangle, Rectangle> emptyRectangles, Dictionary<Module, Module> placedModules)
        {
            //If the board is empty, the placement is legal iff it leaves at least 1 empty rectangle:
            if (emptyRectangles.Count == 1 && placedModules.Count == 0)
            {
                return (module.Shape.width != rectangle.width || module.Shape.height != rectangle.height);
            }

            var splittedRectangles = Rectangle.SplitIntoSmallerRectangles(rectangle, module.Shape);
            splittedRectangles.newSmaller.isEmpty = false;

            Rectangle[] allRectangles = new Rectangle[]
            {
                splittedRectangles.newSmaller,
                splittedRectangles.top,
                splittedRectangles.right
            };
            return DoesNotBlockRouteToAnyModuleOrEmptyRectangle(rectangle, allRectangles, splittedRectangles.newSmaller, module, emptyRectangles, placedModules);

        }

        public static bool DoesNotBlockRouteToAnyModuleOrEmptyRectangle(Rectangle rectangle, Rectangle[] newRectangles, Rectangle newModuleRectangle, Module module, Dictionary<Rectangle, Rectangle> emptyRectangles, Dictionary<Module, Module> placedModules)
        {
            Rectangle.ReplaceRectangles(rectangle, newRectangles);

            //The source empty rectangle for the search does not matter, as paths are symmetric:
            Rectangle randomEmptyRectangle = getEmptyAdjacentRectangle(newModuleRectangle);
            if (randomEmptyRectangle == null)
            {
                //Revert back to the original board
                Rectangle.ReplaceRectangles(newRectangles, rectangle);
                return false;
            }

            //Breadth first search, finding all the empty rectangles and placed modules that can be visited.
            HashSet<Rectangle> visitedEmptyRectangles = new HashSet<Rectangle>() { randomEmptyRectangle };
            HashSet<Rectangle> visitedModuleRectangles = new HashSet<Rectangle>();
            Queue<Rectangle> emptyRectanglesToVisit = new Queue<Rectangle>();
            emptyRectanglesToVisit.Enqueue(randomEmptyRectangle);

            while (emptyRectanglesToVisit.Count > 0)
            {
                Rectangle currentEmptyRectangle = emptyRectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in currentEmptyRectangle.AdjacentRectangles)
                {
                    ////Is module
                    //if (!adjacentRectangle.isEmpty)
                    //{
                    //    visitedModuleRectangles.Add(adjacentRectangle);
                    //}
                    ////Hasn't seen rectangle before
                    //else if (!visitedEmptyRectangles.Contains(adjacentRectangle))
                    //{
                    //    visitedEmptyRectangles.Add(adjacentRectangle);
                    //    emptyRectanglesToVisit.Enqueue(adjacentRectangle);
                    //}

                    if (adjacentRectangle.isEmpty && visitedEmptyRectangles.Add(adjacentRectangle))
                        emptyRectanglesToVisit.Enqueue(adjacentRectangle);
                    else if (!adjacentRectangle.isEmpty)
                        visitedModuleRectangles.Add(adjacentRectangle);
                }
            }

            //Revert back to the original board
            Rectangle.ReplaceRectangles(newRectangles, rectangle);


            //+1 because a module was added
            bool visitedAllModules = placedModules.Count + 1 == visitedModuleRectangles.Count;
            //Add the rectangles from the splitted rectangle array. - 2 because one rectangle is a module
            //and another for the rectangle that was splitted up.
            bool visitedAllEmptyRectangless = emptyRectangles.Count + newRectangles.Length - 2 == visitedEmptyRectangles.Count;

            return visitedAllModules && visitedAllEmptyRectangless;
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

        public void PlaceModuleInRectangle(Module module, Rectangle bestFitRectangle, Board board)
        {
            module.Shape = new Rectangle(module.Shape.width, module.Shape.height, bestFitRectangle.x, bestFitRectangle.y);
            module.Shape.isEmpty = false;

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
                RectangleOptimizations.OptimizeRectangle(board, topRectangle);
            }
            if (rightRectangle != null)
            {
                //In the case that adjacentRectangle have been modified in the above merge, 
                //we must ensure that the rectangle is actually placed on the board:
                if (board.EmptyRectangles.TryGetValue(rightRectangle, out Rectangle rightRectangleInDictionary))
                {
                    RectangleOptimizations.OptimizeRectangle(board, rightRectangleInDictionary);
                }
            }
            
        }

        public void FastTemplateRemove(Module module)
        {
            PlacedModules.Remove(module);
            //All dependencies on the rectangle from the module, should be moved to the new empty rectangle.
            //It is easier to just create a new rectangle for the module:
            Rectangle newModuleRectangle = new Rectangle(module.Shape);
            newModuleRectangle.isEmpty = false;

            Rectangle emptyRectangle = module.Shape;
            module.Shape = newModuleRectangle;

            EmptyRectangles.Add(emptyRectangle, emptyRectangle);
            emptyRectangle.isEmpty = true;

            ClearBoard(emptyRectangle);
            RectangleOptimizations.OptimizeRectangle(this, emptyRectangle);
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
                    if (grid[i, j] == null) printedBoard.Append(String.Format("{0,3}", "O"));
                    else {
                        int index = allPlacedModules.IndexOf(grid[i,j]);
                        printedBoard.Append(String.Format("{0,3}", index));
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
            List<Rectangle> allRectangles = operationExecutingModule.GetOutputLayout().GetAllRectanglesIncludingDroplets();

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
