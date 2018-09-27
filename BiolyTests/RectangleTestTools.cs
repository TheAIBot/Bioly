using BiolyCompiler.Architechtures;
using BiolyCompiler.Modules;
using BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations;
using BiolyTests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    public static class RectangleTestTools
    {
        public static (Board board, List<(Rectangle, int)> rectangles, List<(Module, int)> modules) ArrayToRectangles(int[] array, int arrayWidth, string fluidName = "random")
        {
            Assert.AreEqual(0, array.Length % arrayWidth);

            Board board = new Board(arrayWidth, array.Length / arrayWidth);
            board.EmptyRectangles.Clear();

            Dictionary<int, List<(int x, int y)>> rectangleData = new Dictionary<int, List<(int x, int y)>>();
            array.Distinct().Where(x => x != 0).ForEach(x => rectangleData.Add(x, new List<(int, int)>()));

            for (int y = 0; y < array.Length / arrayWidth; y++)
            {
                for (int x = 0; x < arrayWidth; x++)
                {
                    int value = array[y * arrayWidth + x];
                    if (value != 0)
                    {
                        rectangleData[value].Add((x, y));
                    }
                }
            }
            BoardFluid fluid = new BoardFluid(fluidName);
            List<(Rectangle rect, int id)> rectangles = new List<(Rectangle, int)>(rectangleData.Count);
            List<(Module rect, int id)> modules = new List<(Module, int)>(rectangleData.Count);
            foreach (var data in rectangleData.OrderBy(x => x.Key).Select(x => x))
            {
                int minX = data.Value.Min(d => d.x);
                int minY = data.Value.Min(d => d.y);
                int maxX = data.Value.Max(d => d.x);
                int maxY = data.Value.Max(d => d.y);

                int x = minX;
                int y = minY;
                int width = maxX - minX + 1;
                int height = maxY - minY + 1;

                Rectangle newRectangle = new Rectangle(width, height, x, y);
                rectangles.Add((newRectangle, data.Key));

                if (data.Key < 0)
                {
                    Droplet module = new Droplet(fluid);
                    module.Shape = newRectangle;
                    newRectangle.isEmpty = false;
                    board.PlacedModules.Add(module, module);
                    modules.Add((module, data.Key));
                    board.UpdateGridAtGivenLocation(module, newRectangle);
                }
                else
                {
                    board.EmptyRectangles.Add(newRectangle, newRectangle);
                }
            }

            //Make adjacencies between all the rectangles
            List<Rectangle> onlyRectangles = rectangles.Select(x => x.rect).ToList();
            rectangles.ForEach(x => x.rect.Connect(onlyRectangles));

            return (board, rectangles, modules);
        }

        private static List<Rectangle> GetAllRectanglesInGraph(Rectangle rectangle)
        {
            List<Rectangle> foundRectangles = new List<Rectangle>();
            HashSet<Rectangle> seenRectangles = new HashSet<Rectangle>();
            Queue<Rectangle> toSearchIn = new Queue<Rectangle>();

            toSearchIn.Enqueue(rectangle);
            while (toSearchIn.Count > 0)
            {
                Rectangle toSearch = toSearchIn.Dequeue();

                if (seenRectangles.Contains(toSearch))
                {
                    continue;
                }

                seenRectangles.Add(toSearch);
                foundRectangles.Add(toSearch);
                toSearch.AdjacentRectangles.ForEach(x => toSearchIn.Enqueue(x));
            }

            return foundRectangles;
        }

        public static int[][] RectangleIntMap(List<Rectangle> rectangles, int width, int height)
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

            return map;
        }

        private static string RectanglesToString(List<Rectangle> rectangles, int width, int height)
        {
            int[][] map = RectangleIntMap(rectangles, width, height);

            return String.Join(Environment.NewLine, map.Select(x => String.Join(", ", x.Select(z => String.Format("{0,2}", z)))));
        }

        public static void CompareBoardsAfterOptimized(int[] before, int[] after, int width, int merger)
        {
            (Board beforeBoard, List<(Rectangle rect, int id)> beforeRectangles, _) = ArrayToRectangles(before, width);
            (Board afterBoard, _, _) = ArrayToRectangles(after, width);

            Rectangle mergerRectangle = beforeRectangles.Single(x => x.id == merger).rect;
            RectangleOptimizations.OptimizeRectangle(beforeBoard, mergerRectangle);

            CompareBoards(beforeBoard, afterBoard, merger.ToString());
        }

        public static void CompareBoardsAfterPlacement(int[] before, int[] after, int width, Module toPlace)
        {
            (Board beforeBoard, _, _) = ArrayToRectangles(before, width);
            (Board afterBoard, _, _) = ArrayToRectangles(after, width);

            beforeBoard.FastTemplatePlace(toPlace);

            CompareBoards(beforeBoard, afterBoard);
        }

        public static void CompareBoards(Board actualBoard, Board expectedBoard, string errorMessage = "")
        {
            List<Rectangle> actualRectangles = new List<Rectangle>();
            actualBoard.EmptyRectangles.ForEach(x => actualRectangles.Add(x.Key));
            actualBoard.PlacedModules.ForEach(x => actualRectangles.Add(x.Key.Shape));

            errorMessage = Environment.NewLine + errorMessage + Environment.NewLine + RectanglesToString(actualRectangles, expectedBoard.width, expectedBoard.heigth);

            Assert.AreEqual(expectedBoard.width, actualBoard.width);
            Assert.AreEqual(expectedBoard.heigth, actualBoard.heigth);
            CollectionAssert.AreEquivalent(expectedBoard.EmptyRectangles.ToList(), actualBoard.EmptyRectangles.ToList(), errorMessage);
            CollectionAssert.AreEquivalent(expectedBoard.PlacedModules.Select(x => x.Key.Shape).ToList(), actualBoard.PlacedModules.Select(x => x.Key.Shape).ToList(), errorMessage);

            Assert.IsFalse(DoesRectanglesOverlap(actualBoard), errorMessage);
            Assert.IsTrue(CompareAdjacencyGraphs(actualBoard, expectedBoard), errorMessage);

            Assert.IsTrue(CoversWholeExpectedMap(actualBoard, expectedBoard));
            Assert.IsTrue(HasNessesaryConnections(actualBoard));
            Assert.IsTrue(HasNessesaryConnections(expectedBoard));
        }

        private static bool DoesRectanglesOverlap(Board board)
        {
            int[,] overlapMap = MakeOverlapMap(board);
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.heigth; y++)
                {
                    if (overlapMap[x,y] > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int[,] MakeOverlapMap(Board board)
        {
            return MakeOverlapMap(board.CopyAllRectangles(), board.width, board.heigth);
        }

        private static int[,] MakeOverlapMap(Rectangle[] boardLayout, int width, int height)
        {
            int[,] overlapMap = new int[width, height];

            foreach (Rectangle rect in boardLayout)
            {
                for (int x = rect.x; x < rect.x + rect.width; x++)
                {
                    for (int y = rect.y; y < rect.y + rect.height; y++)
                    {
                        overlapMap[x, y]++;
                    }
                }
            }

            return overlapMap;
        }

        private static bool CompareAdjacencyGraphs(Board actual, Board expected)
        {
            bool CompareAdjacencies(Rectangle actualRect, Rectangle expectedRect)
            {
                foreach (var adjacency in actualRect.AdjacentRectangles)
                {
                    if (!expectedRect.AdjacentRectangles.Contains(adjacency))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var beforeEmpty in actual.EmptyRectangles)
            {
                Rectangle afterEmpty = expected.EmptyRectangles.Single(x => x.Key.Equals(beforeEmpty.Key)).Key;
                if (!CompareAdjacencies(beforeEmpty.Key, afterEmpty))
                {
                    return false;
                }
            }

            foreach (var beforeEmpty in actual.PlacedModules)
            {
                Rectangle afterEmpty = expected.PlacedModules.Single(x => x.Key.Shape.Equals(beforeEmpty.Key.Shape)).Key.Shape;
                if (!CompareAdjacencies(beforeEmpty.Key.Shape, afterEmpty))
                {
                    return false;
                }
            }

            return true;
        }

        public static void VerifyBoards(List<Rectangle[]> boardLayouts, int width, int height)
        {
            boardLayouts.ForEach(x => VerifyBoard(x, width, height));
        }

        public static void VerifyBoard(Rectangle[] boardLayout, int width, int height)
        {
            Assert.IsTrue(CoversWholeMap(boardLayout, width, height));
            //Assert.IsTrue(HasNessesaryConnections(board));
        }

        private static bool CoversWholeMap(Rectangle[] boardLayout, int width, int height)
        {
            int[,] overlapMap = MakeOverlapMap(boardLayout, width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (overlapMap[x, y] != 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CoversWholeExpectedMap(Board actual, Board expected)
        {
            int[,] actualOverlapMap = MakeOverlapMap(actual);
            int[,] expectedOverlapMap = MakeOverlapMap(expected);
            for (int x = 0; x < actual.width; x++)
            {
                for (int y = 0; y < actual.heigth; y++)
                {
                    if (actualOverlapMap[x, y] != expectedOverlapMap[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool HasNessesaryConnections(Board board)
        {
            List<(Rectangle, int)> allRectangles = new List<(Rectangle, int)>();
            board.EmptyRectangles.ForEach(x => allRectangles.Add((x.Key, x.Key.AdjacentRectangles.Count)));
            board.PlacedModules.ForEach(x => allRectangles.Add((x.Key.Shape, x.Key.Shape.AdjacentRectangles.Count)));

            List<Rectangle> onlyRectangles = allRectangles.Select(x => x.Item1).ToList();
            allRectangles.ForEach(x => x.Item1.Connect(onlyRectangles));

            foreach (var rectangleData in allRectangles)
            {
                if (rectangleData.Item1.AdjacentRectangles.Count != rectangleData.Item2)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
