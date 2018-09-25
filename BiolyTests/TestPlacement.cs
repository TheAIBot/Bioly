using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using BiolyTests.TestObjects;
using BiolyCompiler;
using MoreLinq;
using BiolyCompiler.Modules.HelperObjects;
//using MoreLinq;

namespace BiolyTests.PlacementTests
{
    [TestClass]
    public class TestBoard
    {
        [TestMethod]
        public void TestFastTemplatePlaceSingleModuleBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Assert.AreEqual(board.EmptyRectangles.Values.Count, 1);
            TestModule testModule = new TestModule(3,3,2000);
            Assert.IsTrue(board.FastTemplatePlace(testModule));
            Assert.AreEqual(2, board.EmptyRectangles.Values.Count);
            Assert.AreEqual(2, testModule.Shape.AdjacentRectangles.Count);
            foreach (var rectangle in board.EmptyRectangles.Values) {
                testModule.Shape.AdjacentRectangles.Contains(rectangle);
            }
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
        }

        [TestMethod]
        public void TestFastTemplatePlaceNonEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Module module1 = new TestModule(3, 8, 2000);
            Module module2 = new TestModule(4, 3, 2000);
            Module module3 = new TestModule(3, 3, 2000);

            //Module 1 should go in the lower left corner
            Assert.IsTrue(board.FastTemplatePlace(module1));
            Assert.AreEqual(2, board.EmptyRectangles.Values.Count);
            Assert.AreEqual(0, module1.Shape.x);
            Assert.AreEqual(0, module1.Shape.y);
            //It should have split vertically, and module 2 should only fit to the right rectangle, though the other is smaller:
            Assert.IsTrue(board.FastTemplatePlace(module2));
            Assert.AreEqual(3, board.EmptyRectangles.Values.Count);
            Assert.AreEqual(module1.Shape.getRightmostXPosition() + 1, module2.Shape.x);
            Assert.AreEqual(0, module2.Shape.y);
            //The top rectangle should be the smallest:
            Assert.IsTrue(board.FastTemplatePlace(module3));
            Assert.AreEqual(3, board.EmptyRectangles.Values.Count); //No new right empty rectangle
            Assert.AreEqual(0, module3.Shape.x);
            Assert.AreEqual(module1.Shape.getTopmostYPosition() + 1, module3.Shape.y);
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));

        }
        


        //[TestMethod]
        public void TestFastTemplateRemoveSplitMerge()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestPlaceModuleWithBufferNonEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            board.FastTemplatePlace(new Droplet(new BoardFluid("test1")));
            DebugTools.checkAdjacencyMatrixCorrectness(board);
            board.FastTemplatePlace(new Droplet(new BoardFluid("test2")));
            DebugTools.checkAdjacencyMatrixCorrectness(board);
            int width = 4, heigth = 4;
            Module module = new TestModule(width, heigth, 2000);
            board.FastTemplatePlace(module); //It only fits in one rectangle, where it needs to be buffered.

            //The division of the empty rectangles should be very specific:
            DebugTools.checkAdjacencyMatrixCorrectness(board);
            Assert.AreEqual((4 + 2) + 1, board.EmptyRectangles.Values.Count);
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(width + 2, 1, 0, 3)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(1, heigth + 1, 0, 4)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(width + 1, 1, 1, heigth + 1 + 3)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(1, heigth, width + 1, 4)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth - width - 2, boardHeight - Droplet.DROPLET_HEIGHT, width + 2, Droplet.DROPLET_HEIGHT)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth - 2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 2 * Droplet.DROPLET_WIDTH, 0)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(width + 2, boardHeight - Droplet.DROPLET_HEIGHT - heigth - 2, 0, Droplet.DROPLET_HEIGHT + heigth + 2)));
            Assert.AreEqual(3, board.PlacedModules.Count);

            for (int x = module.Shape.x; x <= module.Shape.getRightmostXPosition(); x++)
            {
                for (int y = module.Shape.y; y <= module.Shape.getTopmostYPosition(); y++)
                {
                    Assert.AreEqual(module, board.grid[x, y]);
                }
            }
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
            //When it has been deleted, everything should return to the state before:
            board.FastTemplateRemove(module);
            Assert.AreEqual(2, board.EmptyRectangles.Values.Count);
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth - 2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 2 * Droplet.DROPLET_WIDTH, 0)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth, boardHeight - Droplet.DROPLET_HEIGHT, 0, Droplet.DROPLET_HEIGHT)));
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
        }

        [TestMethod]
        public void TestFastTemplateRemoveAddAndBacktrack()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Module module1 = new TestModule(3, 8, 2000);
            Module module2 = new TestModule(4, 3, 2000);
            Module module3 = new TestModule(3, 3, 2000);
            //Placing some components:
            List<HashSet<Rectangle>> listEmptyRectangles = new List<HashSet<Rectangle>>();
            listEmptyRectangles.Add(new HashSet<Rectangle>());
            board.EmptyRectangles.Values.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module1));
            listEmptyRectangles.Add(new HashSet<Rectangle>());
            board.EmptyRectangles.Values.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module2));
            listEmptyRectangles.Add(new HashSet<Rectangle>());
            board.EmptyRectangles.Values.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module3));

            board.FastTemplateRemove(module3);
            bool sameElements   = listEmptyRectangles[2].Where(rec => !board.EmptyRectangles.Values.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            board.FastTemplateRemove(module2);
            sameElements        = listEmptyRectangles[1].Where(rec => !board.EmptyRectangles.Values.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            board.FastTemplateRemove(module1);
            sameElements        = listEmptyRectangles[0].Where(rec => !board.EmptyRectangles.Values.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
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
                    else {
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
            foreach (var rectangle in set.Values)
                return rectangle;
            return null;
        }
        
    }
}
