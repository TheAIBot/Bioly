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
    /// <summary>
    /// The algorithm is based on the fast template placement algorithm from "Fast template placement for reconfigurable computing systems".
    /// In essence, it divides the board into rectangles, finds the smallest empty rectangle the module can fit in,
    /// and places it there: it then updates the rectangles on the board.
    /// 
    /// It is not a complete implementation, and it has some modifications compared to the algorithm described in the article.
    /// If neccessary, optimizations described in the article, can be implemented for better performance.
    /// 
    /// Note that a module will not be placed, so that all routes to any module on the board, gets blocked.
    /// </summary>
    public class Board
    {
        public readonly int Width;
        public readonly int Heigth;
        public readonly Module[,] ModuleGrid;
        public readonly Dictionary<Module, Module> PlacedModules = new Dictionary<Module, Module>();
        public readonly Dictionary<Rectangle, Rectangle> EmptyRectangles = new Dictionary<Rectangle, Rectangle>();


        public Board(int width, int heigth)
        {
            this.Width = width;
            this.Heigth = heigth;
            this.ModuleGrid = new Module[width, heigth];

            //To start out with, the board is completely empty.
            //So to start out with the board is a single big empty rectangle.
            Rectangle emptyRectangle = new Rectangle(width, heigth);
            EmptyRectangles.Add(emptyRectangle, emptyRectangle);
        }

        public bool FastTemplatePlace(Module module)
        {
            var bufferConfigurations = new (bool left, bool bottom)[]
{
                (false, false),
                (false, true ),
                (true , false),
                (true , true ),
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
                    int bufferWidth  = module.Shape.width  + (useBuffer.left   ? 1 : 0);
                    int bufferHeight = module.Shape.height + (useBuffer.bottom ? 1 : 0);

                    if (!rectangle.DoesRectangleFitInside(bufferWidth, bufferHeight))
                    {
                        continue;
                    }

                    var bufferRectangles = CreateBufferedModuleRectangles(module, rectangle, useBuffer.left, useBuffer.bottom);
                    if (IsBlockingRouteToModuleOrEmptyRectangle(rectangle, bufferRectangles.all, bufferRectangles.center))
                    {
                        continue;
                    }

                    if (IsBlockingOutputLayoutModules(rectangle, bufferRectangles.all, bufferRectangles.center, module))
                    {
                        continue;
                    }

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

            return false;
        }

        private static (Rectangle[] all, Rectangle center) CreateBufferedModuleRectangles(Module module, Rectangle bigRectangle, bool leftBuffer, bool bottomBuffer)
        {
            int bufferWidth  = module.Shape.width  + (leftBuffer   ? 1 : 0);
            int bufferHeight = module.Shape.height + (bottomBuffer ? 1 : 0);

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
                rectangles.Add(new Rectangle(1, bufferHeight - (bottomBuffer ? 1 : 0), splittedRectangle.newSmaller.x, splittedRectangle.newSmaller.y + (bottomBuffer ? 1 : 0)));
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

        private bool IsBlockingRouteToModuleOrEmptyRectangle(Rectangle rectangle, Rectangle[] newRectangles, Rectangle newModuleRectangle)
        {
            Rectangle.ReplaceRectangles(rectangle, newRectangles);

            //The source empty rectangle for the search does not matter, as paths are symmetric:
            Rectangle randomEmptyRectangle = newModuleRectangle.AdjacentRectangles.FirstOrDefault(x => x.isEmpty);
            if (randomEmptyRectangle == null)
            {
                //Revert back to the original board
                Rectangle.ReplaceRectangles(newRectangles, rectangle);
                return true;
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
            return foundRectangles.Count != newRectangles.Length + (EmptyRectangles.Count - 1) + PlacedModules.Count;
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

        private bool IsBlockingOutputLayoutModules(Rectangle rectangle, Rectangle[] otherRectangles, Rectangle newModuleRectangle, Module module)
        {
            if (!module.HasOutputLayout())
            {
                return false;
            }

            Rectangle[] moduleResult = module.GetOutputLayout().GetAllRectanglesIncludingDroplets();
            moduleResult = moduleResult.Select(x => Rectangle.Translocate(x, newModuleRectangle.x, newModuleRectangle.y)).ToArray();

            Rectangle[] allRectangles = otherRectangles.Union(moduleResult).ToArray();

            foreach (var moduleRectangle in moduleResult.Where(x => !x.isEmpty))
            {
                if (IsBlockingRouteToModuleOrEmptyRectangle(rectangle, allRectangles, moduleRectangle))
                {
                    return true;
                }
            }

            return false;
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
                    ModuleGrid[i + emptyRectangle.x, j + emptyRectangle.y] = null;
                }
            }
        }

        public void UpdateGridAtGivenLocation(Module module, Rectangle rectangleToPlaceAt)
        {
            for (int i = 0; i < module.Shape.width; i++)
            {
                for (int j = 0; j < module.Shape.height; j++)
                {
                    ModuleGrid[i + rectangleToPlaceAt.x, j + rectangleToPlaceAt.y] = module;
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
            for (int j = Heigth - 1; j >= 0; j--)
            {
                for (int i = 0; i < Width; i++)
                {
                    if (ModuleGrid[i, j] == null) printedBoard.Append(String.Format("{0,3}", "O"));
                    else {
                        int index = allPlacedModules.IndexOf(ModuleGrid[i,j]);
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

        public Rectangle[] CopyAllRectangles()
        {
            Rectangle[] allRectangles = new Rectangle[EmptyRectangles.Count + PlacedModules.Count];

            int index = 0;
            foreach (var empty in EmptyRectangles)
            {
                allRectangles[index++] = new Rectangle(empty.Key);
            }
            foreach (var module in PlacedModules)
            {
                allRectangles[index++] = new Rectangle(module.Key.Shape);
            }

            return allRectangles;
        }
    }
}
