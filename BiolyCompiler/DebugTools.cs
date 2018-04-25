using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using MoreLinq;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BiolyCompiler.Modules;
using System.Xml;

namespace BiolyCompiler
{
    public static class DebugTools
    {
        public static void makeDebugCorrectnessChecks(Board board, SimplePriorityQueue<FluidBlock> runningOperations, List<Module> usedModules)
        {
            Debug.WriteLine(board.print(usedModules));
            runningOperations.ToList()
                             .OrderBy(element => element.startTime)
                             .ForEach(element => Debug.WriteLine(element.OutputVariable + ", " + element.startTime + ", " + element.endTime));
            checkAdjacencyMatrixCorrectness(board);
        }

        public static void checkAdjacencyMatrixCorrectness(Board board)
        {
            if (!doAdjacencyGraphContainTheCorrectNodes(board))
                throw new Exception("The boards adjacency graph does not match up with the placed modules and empty rectangles.");
        }


        public static bool doAdjacencyGraphContainTheCorrectNodes(Board board)
        {
            //It visits all the modules and rectangles in the graph, 
            //and checks if they are in board.PlacedModules and board.EmptyRectangles respectivly.
            HashSet<Rectangle> emptyVisitedRectangles = new HashSet<Rectangle>();
            HashSet<Rectangle> moduleVisitedRectangles = new HashSet<Rectangle>();

            Rectangle initialRectangle = GetRandomRectangle(board.EmptyRectangles);
            emptyVisitedRectangles.Add(initialRectangle);
            Queue<Rectangle> rectanglesToVisit = new Queue<Rectangle>();
            rectanglesToVisit.Enqueue(initialRectangle);

            while (rectanglesToVisit.Count > 0)
            {
                Rectangle currentRectangle = rectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in currentRectangle.AdjacentRectangles)
                {
                    if (emptyVisitedRectangles.Contains(adjacentRectangle) || moduleVisitedRectangles.Contains(adjacentRectangle))
                        continue;
                    else
                    {
                        if (adjacentRectangle.isEmpty)
                            emptyVisitedRectangles.Add(adjacentRectangle);
                        else
                            moduleVisitedRectangles.Add(adjacentRectangle);
                        rectanglesToVisit.Enqueue(adjacentRectangle);
                    }
                }
            }

            HashSet<Rectangle> placedModuleRectangles = new HashSet<Rectangle>(board.placedModules.Select(module => module.Shape));


            return isSameSet(emptyVisitedRectangles, board.EmptyRectangles) && isSameSet(moduleVisitedRectangles, placedModuleRectangles);
        }

        private static bool isSameSet(HashSet<Rectangle> set1, HashSet<Rectangle> set2)
        {
            return set1.Count == set2.Count && set1.All(rectangle => set2.Contains(rectangle));
        }

        private static Rectangle GetRandomRectangle(HashSet<Rectangle> set)
        {
            foreach (var rectangle in set)
                return rectangle;
            return null;
        }
    }
}