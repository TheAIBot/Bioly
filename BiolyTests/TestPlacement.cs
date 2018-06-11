using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
using BiolyTests.TestObjects;
using BiolyCompiler;
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
            Assert.AreEqual(board.EmptyRectangles.Count, 1);
            TestModule testModule = new TestModule(3,3,2000);
            Assert.IsTrue(board.FastTemplatePlace(testModule));
            Assert.AreEqual(2, board.EmptyRectangles.Count);
            Assert.AreEqual(2, testModule.Shape.AdjacentRectangles.Count);
            foreach (var rectangle in board.EmptyRectangles) {
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
            Assert.AreEqual(2, board.EmptyRectangles.Count);
            Assert.AreEqual(0, module1.Shape.x);
            Assert.AreEqual(0, module1.Shape.y);
            //It should have split vertically, and module 2 should only fit to the right rectangle, though the other is smaller:
            Assert.IsTrue(board.FastTemplatePlace(module2));
            Assert.AreEqual(3, board.EmptyRectangles.Count);
            Assert.AreEqual(module1.Shape.getRightmostXPosition() + 1, module2.Shape.x);
            Assert.AreEqual(0, module2.Shape.y);
            //The top rectangle should be the smallest:
            Assert.IsTrue(board.FastTemplatePlace(module3));
            Assert.AreEqual(3, board.EmptyRectangles.Count); //No new right empty rectangle
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
        public void TestPlaceModuleWithCompleteBufferEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            int width = 4, heigth = 4;
            Module module = new TestModule(width, heigth, 2000);
            Assert.IsTrue(board.PlaceCompletlyBufferedModuleInRectangle(module, board.EmptyRectangles.First()));

            //The division of the empty rectangles should be very specific:

            Assert.AreEqual(4+2, board.EmptyRectangles.Count);
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(width + 2, 1, 0, 0)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(1, heigth + 1, 0, 1)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(width + 1, 1, 1, heigth + 1)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(1, heigth, width + 1, 1)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth, boardHeight - heigth - 2, 0, heigth + 2)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth - width - 2, heigth + 2, width + 2, 0)));

            for (int x = module.Shape.x; x <= module.Shape.getRightmostXPosition(); x++)
            {
                for (int y = module.Shape.y; y <= module.Shape.getTopmostYPosition(); y++)
                {
                    Assert.AreEqual(module, board.grid[x, y]);
                }
            }
            foreach (var rectangle in board.EmptyRectangles)
            {
                foreach (var rectangle2 in board.EmptyRectangles)
                {
                    if (rectangle.IsAdjacent(rectangle2)) {
                        Assert.IsTrue(rectangle.AdjacentRectangles.Contains(rectangle2));
                        Assert.IsTrue(rectangle2.AdjacentRectangles.Contains(rectangle));
                    } else {

                        Assert.IsFalse(rectangle.AdjacentRectangles.Contains(rectangle2));
                        Assert.IsFalse(rectangle2.AdjacentRectangles.Contains(rectangle));
                    }
                }
            }
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
            //When it has been deleted, everything should return to the state before:
            board.FastTemplateRemove(module);
            Assert.AreEqual(1, board.EmptyRectangles.Count);
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth, boardHeight, 0, 0)));
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
        }

        [TestMethod]
        public void TestPlaceModuleWithBufferNonEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            board.FastTemplatePlace(new Droplet(new BoardFluid("test1")));
            board.FastTemplatePlace(new Droplet(new BoardFluid("test2")));
            int width = 4, heigth = 4;
            Module module = new TestModule(width, heigth, 2000);
            board.FastTemplatePlace(module); //It only fits in one rectangle, where it needs to be buffered.

            //The division of the empty rectangles should be very specific:
            DebugTools.checkAdjacencyMatrixCorrectness(board);
            Assert.AreEqual((4 + 2) + 1, board.EmptyRectangles.Count);
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(width + 2, 1, 0, 3)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(1, heigth + 1, 0, 4)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(width + 1, 1, 1, heigth + 1 + 3)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(1, heigth, width + 1, 4)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth - width - 2, boardHeight - Droplet.DROPLET_HEIGHT, width + 2, Droplet.DROPLET_HEIGHT)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth - 2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 2 * Droplet.DROPLET_WIDTH, 0)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(width + 2, boardHeight - Droplet.DROPLET_HEIGHT - heigth - 2, 0, Droplet.DROPLET_HEIGHT + heigth + 2)));
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
            Assert.AreEqual(2, board.EmptyRectangles.Count);
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth - 2*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 2 * Droplet.DROPLET_WIDTH, 0)));
            Assert.IsTrue(board.EmptyRectangles.Contains(new Rectangle(boardWidth, boardHeight - Droplet.DROPLET_HEIGHT, 0, Droplet.DROPLET_HEIGHT)));
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
            board.EmptyRectangles.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module1));
            listEmptyRectangles.Add(new HashSet<Rectangle>());
            board.EmptyRectangles.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module2));
            listEmptyRectangles.Add(new HashSet<Rectangle>());
            board.EmptyRectangles.ForEach(rectangle => listEmptyRectangles[listEmptyRectangles.Count - 1].Add(new Rectangle(rectangle)));
            Assert.IsTrue(board.FastTemplatePlace(module3));

            board.FastTemplateRemove(module3);
            bool sameElements   = listEmptyRectangles[2].Where(rec => !board.EmptyRectangles.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            board.FastTemplateRemove(module2);
            sameElements        = listEmptyRectangles[1].Where(rec => !board.EmptyRectangles.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            board.FastTemplateRemove(module1);
            sameElements        = listEmptyRectangles[0].Where(rec => !board.EmptyRectangles.Contains(rec)).ToList().Count == 0;
            Assert.IsTrue(sameElements);
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
            
        }

        [TestMethod]
        public void TestMergeWithOtherRectangles()
        {
            int boardWidth = 20;
            int boardHeigth = 20;
            Board board = new Board(boardWidth, boardHeigth);
            //Based on an example given in the original article, page 16
            Rectangle lowerLeft     = new Rectangle(10, 10,  0, 0);
            Rectangle lowerRight    = new Rectangle(10, 15, 10, 0);
            Rectangle topLeft       = new Rectangle( 5, 10,  0, 10);
            Rectangle topRight      = new Rectangle(15,  5,  5, 15);
            Rectangle middle        = new Rectangle( 5,  5,  5, 10);

            board.EmptyRectangles.Clear();
            board.EmptyRectangles.Add(lowerLeft);
            board.EmptyRectangles.Add(lowerRight);
            board.EmptyRectangles.Add(topLeft);
            board.EmptyRectangles.Add(topRight);
            board.EmptyRectangles.Add(middle);
            foreach (var rectangle1 in board.EmptyRectangles)
                foreach (var rectangle2 in board.EmptyRectangles)
                    rectangle1.ConnectIfAdjacent(rectangle2);
            lowerRight.MergeWithOtherRectangles(board);
            Assert.AreEqual(1, board.EmptyRectangles.Count);
            Rectangle rectangle = board.EmptyRectangles.First();
            Assert.AreEqual(boardWidth, rectangle.width);
            Assert.AreEqual(boardHeigth, rectangle.height);
            Assert.AreEqual(0, rectangle.x);
            Assert.AreEqual(0, rectangle.y);
            Assert.AreEqual(0, rectangle.AdjacentRectangles.Count);
        }



        [TestMethod]
        public void TestMergeWithRectangleLeftSide()
        {
            int x = 43, y = 22, width = 6, height = 10;
            Rectangle staticEmptyRectangle = new Rectangle(width, height, x, y);
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < 2 * (y + height); j++)
                {
                    for (int recWidth = 0; recWidth < 1.5 * width; recWidth++)
                    {
                        for (int recHeigth = 0; recHeigth < 1.5 * height; recHeigth++)
                        {
                            Rectangle nonStaticEmptyRectangle = new Rectangle(recWidth, recHeigth, i, j);
                            (RectangleSide side, bool canMerge) = staticEmptyRectangle.CanMerge(nonStaticEmptyRectangle);
                            if (nonStaticEmptyRectangle.getRightmostXPosition() + 1 == x && y == j && height == recHeigth)
                            {
                                Assert.AreEqual(RectangleSide.Left, side);
                                Assert.IsTrue(canMerge);
                                nonStaticEmptyRectangle.MergeWithRectangle(side, staticEmptyRectangle);
                                Rectangle mergedRectangle = nonStaticEmptyRectangle;
                                Assert.AreEqual(height, mergedRectangle.height);
                                Assert.AreEqual(width + recWidth, mergedRectangle.width);
                                Assert.AreEqual(x, mergedRectangle.x);
                                Assert.AreEqual(y, mergedRectangle.y);
                            }
                            else Assert.IsFalse(canMerge, "Error at i = " + i + ", j = " + j + ", recWidth = " + recWidth + ", recHeight = " + recHeigth);
                        }
                    }
                }
            }
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

            HashSet<Rectangle> placedModuleRectangles = new HashSet<Rectangle>(board.PlacedModules.Select(module => module.Shape));


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
