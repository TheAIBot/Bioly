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
        public void TestPlaceModuleWithCompleteBufferEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            int width = 4, heigth = 4;
            Module module = new TestModule(width, heigth, 2000);
            Assert.IsTrue(board.PlaceCompletlyBufferedModuleInRectangle(module, board.EmptyRectangles.Values.First()));

            //The division of the empty rectangles should be very specific:

            Assert.AreEqual(4+2, board.EmptyRectangles.Values.Count);
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(width + 2, 1, 0, 0)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(1, heigth + 1, 0, 1)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(width + 1, 1, 1, heigth + 1)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(1, heigth, width + 1, 1)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth, boardHeight - heigth - 2, 0, heigth + 2)));
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth - width - 2, heigth + 2, width + 2, 0)));

            for (int x = module.Shape.x; x <= module.Shape.getRightmostXPosition(); x++)
            {
                for (int y = module.Shape.y; y <= module.Shape.getTopmostYPosition(); y++)
                {
                    Assert.AreEqual(module, board.grid[x, y]);
                }
            }
            foreach (var rectangle in board.EmptyRectangles.Values)
            {
                foreach (var rectangle2 in board.EmptyRectangles.Values)
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
            Assert.AreEqual(1, board.EmptyRectangles.Values.Count);
            Assert.IsTrue(board.EmptyRectangles.Values.Contains(new Rectangle(boardWidth, boardHeight, 0, 0)));
            Assert.IsTrue(doAdjacencyGraphContainTheCorrectNodes(board));
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
            board.EmptyRectangles.Add(lowerLeft, lowerLeft);
            board.EmptyRectangles.Add(lowerRight, lowerRight);
            board.EmptyRectangles.Add(topLeft, topLeft);
            board.EmptyRectangles.Add(topRight, topRight);
            board.EmptyRectangles.Add(middle, middle);
            foreach (var rectangle1 in board.EmptyRectangles.Values)
                foreach (var rectangle2 in board.EmptyRectangles.Values)
                    rectangle1.ConnectIfAdjacent(rectangle2);
            lowerRight.MergeWithOtherRectangles(board);
            Assert.AreEqual(1, board.EmptyRectangles.Values.Count);
            Rectangle rectangle = board.EmptyRectangles.Values.First();
            Assert.AreEqual(boardWidth, rectangle.width);
            Assert.AreEqual(boardHeigth, rectangle.height);
            Assert.AreEqual(0, rectangle.x);
            Assert.AreEqual(0, rectangle.y);
            Assert.AreEqual(0, rectangle.AdjacentRectangles.Count);
        }

        [TestMethod]
        public void TestSplitMergeLeftTallerRectangle()
        {
            int boardWidth = 20;
            int boardHeigth = 20;
            Board board = new Board(boardWidth, boardHeigth);

            int rec1Width = boardWidth / 3, rec1Height = (3*boardHeigth) / 4, rec1x = boardWidth/2, rec1y = boardWidth / 2;
            int rec2Width = boardWidth / 4, rec2Height = boardHeigth/2;

            for (int x = 0; x <= (boardWidth/2)/2; x++)
            {
                for (int y = 10; y < boardHeigth + rec1Height + rec2Height + 5; y++)
                {
                    Rectangle rectangle1 = new Rectangle(rec1Width, rec1Height, rec1x, rec1y);
                    Rectangle rectangle2 = new Rectangle(rec2Width, rec2Height, x, y);
                    board.EmptyRectangles.Clear();
                    board.EmptyRectangles.Add(rectangle1, rectangle1);
                    board.EmptyRectangles.Add(rectangle2, rectangle2);
                    if (rectangle1.ConnectIfAdjacent(rectangle2))
                    {
                        bool didSplit = rectangle1.SplitMerge(board);
                        if (didSplit)
                        {
                            Assert.AreEqual(boardWidth / 4, x);
                            Assert.AreEqual(2, board.EmptyRectangles.Values.Count());
                            Assert.IsTrue(board.EmptyRectangles.Values.Contains(rectangle1));
                            Assert.IsTrue(board.EmptyRectangles.Values.Contains(rectangle2));
                            if ((y == rec1y))
                            {
                                Assert.AreEqual(rec1Height - rec2Height, rectangle1.height);
                                Assert.AreEqual(rec1Width, rectangle1.width);
                                Assert.AreEqual(rec1x, rectangle1.x);
                                Assert.AreEqual(rec1y + rec2Height, rectangle1.y);

                                Assert.AreEqual(rec2Height, rectangle2.height);
                                Assert.AreEqual(rec2Width + rec1Width, rectangle2.width);
                                Assert.AreEqual(x, rectangle2.x);
                                Assert.AreEqual(y, rectangle2.y);
                            }
                            else if (y + rec2Height == rec1y + rec1Height)
                            {
                                Assert.AreEqual(rec1Height - rec2Height, rectangle1.height);
                                Assert.AreEqual(rec1Width, rectangle1.width);
                                Assert.AreEqual(rec1x, rectangle1.x);
                                Assert.AreEqual(rec1y, rectangle1.x);

                                Assert.AreEqual(rec2Height, rectangle2.height);
                                Assert.AreEqual(rec2Width + rec1Width, rectangle2.width);
                                Assert.AreEqual(x, rectangle2.x);
                                Assert.AreEqual(y, rectangle2.y);
                            }
                            else Assert.Fail();
                        } else
                        {
                            Assert.IsTrue( !((boardWidth / 4 == x) && (y == boardWidth / 2 || y + rectangle2.height == rectangle1.y + rectangle1.height)) );
                        }
                    }
                }
            }
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
