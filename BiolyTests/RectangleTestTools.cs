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

            return String.Join(Environment.NewLine, map.Select(x => String.Join(", ", x)));
        }

        public static void CompareRectangles(int[] before, int[] after, int width, int merger)
        {
            (Board beforeBoard, List<(Rectangle rect, int id)> beforeRectangles, _) = ArrayToRectangles(before, width);
            (Board afterBoard, List<(Rectangle rect, int id)> afterRectangles, _) = ArrayToRectangles(after, width);

            Rectangle mergerRectangle = beforeRectangles.Single(x => x.id == merger).rect;
            RectangleOptimizations.OptimizeRectangle(beforeBoard, mergerRectangle);

            List<Rectangle> expectedRectangles = afterRectangles.Select(x => x.rect).ToList();
            List<Rectangle> actualRectangles = GetAllRectanglesInGraph(beforeBoard.EmptyRectangles.First().Key);

            string errorMessage = Environment.NewLine + merger.ToString() + Environment.NewLine + RectanglesToString(actualRectangles, width, before.Length / width);
            Assert.AreEqual(0, actualRectangles.Except(beforeBoard.EmptyRectangles.Values).Count(), errorMessage);
            Assert.AreEqual(0, expectedRectangles.Except(actualRectangles).ToArray().Count(), errorMessage);
        }
    }
}
