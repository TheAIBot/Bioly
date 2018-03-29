using System;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
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
            MixerModule mixer = new MixerModule(3,3,2000);
            Assert.IsTrue(board.FastTemplatePlace(mixer));
            Assert.AreEqual(2, board.EmptyRectangles.Count);
            Assert.AreEqual(2, mixer.Shape.AdjacentRectangles.Count);
            foreach (var rectangle in board.EmptyRectangles) {
                mixer.Shape.AdjacentRectangles.Contains(rectangle);
            }            
        }

        [TestMethod]
        public void TestFastTemplatePlaceNonEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Module module1 = new MixerModule(3, 8, 2000);
            Module module2 = new MixerModule(4, 3, 2000);
            Module module3 = new MixerModule(3, 3, 2000);

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

        }

        [TestMethod]
        public void TestFastTemplateReplace()
        {
            Assert.Fail("Not implemented yet");
        }


        [TestMethod]
        public void TestFastTemplateRemoveSplitMerge()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestFastTemplateRemoveAddAndBacktrack()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Module module1 = new MixerModule(3, 8, 2000);
            Module module2 = new MixerModule(4, 3, 2000);
            Module module3 = new MixerModule(3, 3, 2000);
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
            
        }

        [TestMethod]
        public void TestMergeWithOtherRectangles()
        {
            
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestMergeWithRectangleLeftSide()
        {
            int x = 43, y = 22, width = 6, height = 10;
            Rectangle staticEmptyRectangle = new Rectangle(width, height, x, y);
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < 2*(y+height); j++)
                {
                    for (int recWidth = 0; recWidth < 1.5*width; recWidth++)
                    {
                        for (int recHeigth = 0; recHeigth < 1.5*height; recHeigth++)
                        {
                            Rectangle nonStaticEmptyRectangle = new Rectangle(recWidth, recHeigth, i, j);
                            (RectangleSide side, bool canMerge) = staticEmptyRectangle.CanMerge(nonStaticEmptyRectangle);
                            if (nonStaticEmptyRectangle.getRightmostXPosition() + 1 == x && y == j && height == recHeigth)
                            {
                                Assert.AreEqual(RectangleSide.Left, side);
                                Assert.IsTrue(canMerge);
                                Rectangle mergedRectangle = nonStaticEmptyRectangle.MergeWithRectangle(side, staticEmptyRectangle);
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

        [TestMethod]
        public void TestFastTemplateAddRemoveRandom()
        {
            //After adding a lot of modules, removing them all should give the empty rectangle again.
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Module[] modules = new MixerModule[10];
            Assert.Fail("Not implemented yet");
        }



    }
}
