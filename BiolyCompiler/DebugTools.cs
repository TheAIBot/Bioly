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
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler
{
    public static class DebugTools
    {
        public static void makeDebugCorrectnessChecks(Board board, SimplePriorityQueue<FluidBlock> runningOperations, List<Module> usedModules)
        {
            #if DEBUG
                        Debug.WriteLine(board.print(usedModules));
            #endif
            checkAdjacencyMatrixCorrectness(board);
            checkIsBoardPerfectlyPartitioned(board);
        }

        private static void checkIsBoardPerfectlyPartitioned(Board board)
        {
            bool[,] grid = new bool[board.width,board.heigth];
            HashSet<Rectangle> allRectangles = new HashSet<Rectangle>();
            allRectangles.UnionWith(board.EmptyRectangles.Values);
            allRectangles.UnionWith(board.PlacedModules.Values.Select(module => module.Shape));
            foreach (var rectangle in allRectangles)
            {
                for (int i = 0; i < rectangle.width; i++)
                {
                    for (int j = 0; j < rectangle.height; j++)
                    {
                        if (board.width <= rectangle.x + i || board.heigth <= rectangle.y+j )
                        {
                            Console.Write("");
                            continue;
                        }
                        if (grid[rectangle.x + i, rectangle.y + j])
                            throw new InternalRuntimeException("The board is not perfectly partitioned by its rectangles: more than one rectangle is overlapping.");
                        else grid[rectangle.x + i, rectangle.y + j] = true;
                    }
                }
            }

            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.heigth; y++)
                {
                    if (!grid[x, y])
                        throw new InternalRuntimeException("The board is not perfectly partitioned by its rectangles: a part of the board is not inside a rectangle.");
                }
            }
        }

        public static void checkAdjacencyMatrixCorrectness(Board board)
        {
            if (!doAdjacencyGraphContainTheCorrectNodes(board))
                throw new InternalRuntimeException("The boards adjacency graph does not match up with the placed modules and empty rectangles.");
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

            HashSet<Rectangle> placedModuleRectangles = new HashSet<Rectangle>(board.PlacedModules.Values.Select(module => module.Shape));


            return isSameSet(emptyVisitedRectangles, board.EmptyRectangles.Values.ToHashSet()) && isSameSet(moduleVisitedRectangles, placedModuleRectangles);
        }

        private static bool isSameSet(HashSet<Rectangle> set1, HashSet<Rectangle> set2)
        {
            return set1.Count == set2.Count && set1.All(rectangle => set2.Contains(rectangle));
        }

        private static Rectangle GetRandomRectangle(Dictionary<Rectangle, Rectangle> set)
        {
            foreach (var pair in set)
                return pair.Value;
            return null;
        }
    }
}