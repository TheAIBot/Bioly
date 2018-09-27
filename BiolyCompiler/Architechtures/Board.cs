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

        public static bool DoesNotBlockRouteToAnyModuleOrEmptyRectangle(Rectangle rectangle, Rectangle[] newRectangles, Rectangle newModuleRectangle, Module module, Dictionary<Rectangle, Rectangle> emptyRectangles, Dictionary<Module, Module> placedModules)
        {
            Rectangle.ReplaceRectangles(rectangle, newRectangles);

            //The source empty rectangle for the search does not matter, as paths are symmetric:
            Rectangle randomEmptyRectangle = newModuleRectangle.AdjacentRectangles.FirstOrDefault(x => x.isEmpty);
            if (randomEmptyRectangle == null)
            {
                //Revert back to the original board
                Rectangle.ReplaceRectangles(newRectangles, rectangle);
                return false;
            }

            HashSet<Rectangle> foundRectangles = new HashSet<Rectangle>();
            Queue<Rectangle> rectanglesToCheck = new Queue<Rectangle>();
            rectanglesToCheck.Enqueue(randomEmptyRectangle);

            while (rectanglesToCheck.Count > 0)
            {
                Rectangle toCheck = rectanglesToCheck.Dequeue();

                foundRectangles.Add(toCheck);

                if (!toCheck.isEmpty)
                {
                    continue;
                }

                foreach (var adjacent in toCheck.AdjacentRectangles)
                {
                    if (!foundRectangles.Contains(adjacent))
                    {
                        rectanglesToCheck.Enqueue(adjacent);
                    }
                }
            }

            //Revert back to the original board
            Rectangle.ReplaceRectangles(newRectangles, rectangle);

            //-1 to empty rectangles because it contains the rectangle that was replaced
            //by the new rectangles
            return foundRectangles.Count == newRectangles.Length + (emptyRectangles.Count - 1) + placedModules.Count;
        }

        public static bool DoesNotBlockConnectionToSourceEmptyRectangles(Droplet dropletInput, HashSet<Rectangle> outsideEmptyRectangles, HashSet<Rectangle> layoutEmptyRectangles)
        {
            //Breadth first search, finding all the empty rectangles that can be visited.
            HashSet<Rectangle> visitedEmptyRectangles = new HashSet<Rectangle>(outsideEmptyRectangles);
            Queue<Rectangle> emptyRectanglesToVisit = new Queue<Rectangle>();
            foreach (var rectangle in outsideEmptyRectangles)
            {
                emptyRectanglesToVisit.Enqueue(rectangle);
            }

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

        public void FastTemplateRemove(Module module)
        {
            Rectangle emptyReplacement = new Rectangle(module.Shape.width, module.Shape.height, module.Shape.x, module.Shape.y);

            Rectangle.ReplaceRectangles(module.Shape, emptyReplacement);

            PlacedModules.Remove(module);
            EmptyRectangles.Add(emptyReplacement, emptyReplacement);
            ClearBoard(emptyReplacement);

            RectangleOptimizations.OptimizeRectangle(this, emptyReplacement);
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

        public void UpdateGridWithModulePlacement(Module module)
        {
            UpdateGridAtGivenLocation(module, module.Shape);
            PlacedModules.Add(module, module);
        }

        public string print(List<Module> allPlacedModules)
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

        public List<Droplet> replaceWithDroplets(Module module, BoardFluid fluidType)
        {
            Rectangle[] allRectangles = module.GetOutputLayout().GetAllRectanglesIncludingDroplets();
            Rectangle.ReplaceRectangles(module.Shape, allRectangles);

            List<Droplet> droplets = module.GetOutputLayout().Droplets;
            droplets.ForEach(droplet => droplet.SetFluidType(fluidType));

            PlacedModules.Remove(module);
            ClearBoard(module.Shape);

            allRectangles.Where(x => x.isEmpty).ForEach(x => EmptyRectangles.Add(x, x));
            droplets.ForEach(x => UpdateGridWithModulePlacement(x));

            return droplets;
        }

        public Board Copy()
        {
            Board board = new Board(width, heigth);
            board.EmptyRectangles.Clear();

            foreach (var module in PlacedModules.Values)
            {
                board.PlacedModules.Add(module, module);
            }
            foreach (var rectangle in EmptyRectangles.Values)
            {
                board.EmptyRectangles.Add(rectangle, rectangle);
            }

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
