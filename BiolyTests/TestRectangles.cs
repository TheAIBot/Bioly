using System;
using System.Collections.Generic;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyTests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.Architechtures;
using System.Text;
using BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations;

namespace BiolyTests.RectanglesWithModulesTests
{
    [TestClass]
    public class TestRectangles
    {
        [TestMethod]
        public void TestSplitIntoSmallerRectanglesSmallVerticalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 8, moduleWidth = 4;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);
            
            Module module = new TestModule(moduleWidth, moduleHeight, 1000);
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(module.Shape);
            
            Assert.AreEqual(x, TopRectangle.x);
            Assert.AreEqual(x + moduleWidth, RightRectangle.x);

            Assert.AreEqual(y + moduleHeight, TopRectangle.y);
            Assert.AreEqual(y, RightRectangle.y);
            
            Assert.AreEqual(rectangleHeight - moduleHeight, TopRectangle.height);
            Assert.AreEqual(rectangleHeight, RightRectangle.height);

            Assert.AreEqual(moduleWidth, TopRectangle.width);
            Assert.AreEqual(rectangleWidth - moduleWidth, RightRectangle.width);
        }


        [TestMethod]
        public void TestSplitIntoSmallerRectanglesSmallHorizontalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 8;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new TestModule(moduleWidth, moduleHeight, 1000);
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(module.Shape);

            Assert.AreEqual(x, TopRectangle.x);
            Assert.AreEqual(x + moduleWidth, RightRectangle.x);

            Assert.AreEqual(y + moduleHeight, TopRectangle.y);
            Assert.AreEqual(y, RightRectangle.y);

            Assert.AreEqual(rectangleHeight - moduleHeight, TopRectangle.height);
            Assert.AreEqual(moduleHeight, RightRectangle.height);

            Assert.AreEqual(rectangleWidth, TopRectangle.width);
            Assert.AreEqual(rectangleWidth - moduleWidth, RightRectangle.width);
        }

        [TestMethod]
        public void TestSplitIntoSmallerRectanglesNoHorizontalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 10;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new TestModule(moduleWidth, moduleHeight, 1000);
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(module.Shape);

            Assert.AreEqual(null, RightRectangle);

            Assert.AreEqual(x, TopRectangle.x);
            Assert.AreEqual(y + moduleHeight, TopRectangle.y);
            Assert.AreEqual(rectangleHeight - moduleHeight, TopRectangle.height);
            Assert.AreEqual(rectangleWidth, TopRectangle.width);
        }


        [TestMethod]
        public void TestSplitIntoSmallerRectanglesNoVerticalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 10, moduleWidth = 4;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new TestModule(moduleWidth, moduleHeight, 1000);
            (Rectangle TopRectangle, Rectangle RightRectangle) = rectangle.SplitIntoSmallerRectangles(module.Shape);

            Assert.AreEqual(null, TopRectangle);

            Assert.AreEqual(x + moduleWidth, RightRectangle.x);
            Assert.AreEqual(y, RightRectangle.y);
            Assert.AreEqual(rectangleHeight, RightRectangle.height);
            Assert.AreEqual(rectangleWidth - moduleWidth, RightRectangle.width);
        }

        [TestMethod]
        public void TestIsAdjacentLeftRectangle()
        {
            int rectangleWidth = 10, rectangleHeight = 20;
            int rectangleXPos =  54, rectangleYPos = 64;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(rectangleXPos, rectangleYPos);

            int adjacentRectangleWidth = 13, adjacentRectangleHeight = 15;
            Rectangle adjacentRectangle = new Rectangle(adjacentRectangleWidth, adjacentRectangleHeight);

            for (int x = 0; x < rectangleXPos - adjacentRectangleWidth; x++)
            {
                for (int y = 0; y < 2*(rectangleYPos + rectangleHeight); y++)
                {
                    adjacentRectangle.PlaceAt(x, y);
                    Boolean isAdjacent = rectangle.IsAdjacent(adjacentRectangle);
                    if (x + adjacentRectangleWidth != rectangleXPos)
                    {
                        Assert.IsFalse(isAdjacent, "Error at x = " + x + ", y = " + y);
                    } else if (rectangleYPos <= adjacentRectangle.getTopmostYPosition() && y <= rectangle.getTopmostYPosition()) { 
                        Assert.IsTrue(isAdjacent, "Error at x = " + x + ", y = " + y);
                    } else
                    {
                        Assert.IsFalse(isAdjacent, "Error at x = " + x + ", y = " + y);
                    }
                }
            }
        }

        //[TestMethod]
        public void TestIsAdjacentRightRectangle() {
            Assert.Fail("Has not been implemented yet");
        }

        //[TestMethod]
        public void TestIsAdjacentTopRectangle()
        {
            Assert.Fail("Has not been implemented yet");
        }

        //[TestMethod]
        public void TestIsAdjacentLowerRectangle()
        {
            Assert.Fail("Has not been implemented yet");
        }

        //[TestMethod]
        public void TestSplitIntoSmallerRectanglesCorrectRightRectanglesAdjacencies()
        {
            Assert.Fail("Has not been implemented yet");
        }

        [TestMethod]
        public void TestSplitIntoSmallerRectanglesCorrectLeftRectanglesAdjacencies()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 6;
            Rectangle emptyRectangle = new Rectangle(rectangleWidth, rectangleHeight, x, y);
            Module module = new TestModule(moduleWidth, moduleHeight, 1000);

            for (int i = 0; i < 100; i++)
            {
                int neighborWidth = 5;
                Rectangle neighborRectangle = new Rectangle(neighborWidth, rectangleHeight/3, emptyRectangle.x - neighborWidth, y + i);
                if (neighborRectangle.y <= emptyRectangle.getTopmostYPosition()) //They are adjacent:
                {
                    neighborRectangle.AdjacentRectangles.Add(emptyRectangle);
                    emptyRectangle.AdjacentRectangles.Add(neighborRectangle);

                }
                //It is a horizontal split.
                (Rectangle TopRectangle, Rectangle RightRectangle) = emptyRectangle.SplitIntoSmallerRectangles(module.Shape);
                Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(emptyRectangle));
                Assert.IsTrue(module.Shape.AdjacentRectangles.Contains(TopRectangle));
                Assert.IsTrue(module.Shape.AdjacentRectangles.Contains(RightRectangle));
                Assert.IsTrue(TopRectangle.AdjacentRectangles.Contains(module.Shape));
                Assert.IsTrue(RightRectangle.AdjacentRectangles.Contains(module.Shape));
                Assert.IsTrue(TopRectangle.AdjacentRectangles.Contains(RightRectangle));
                Assert.IsTrue(RightRectangle.AdjacentRectangles.Contains(TopRectangle));

                if (neighborRectangle.y <= module.Shape.getTopmostYPosition()) //They are adjacent
                {
                    Assert.IsTrue(neighborRectangle.AdjacentRectangles.Contains(module.Shape));
                    Assert.IsTrue(module.Shape.AdjacentRectangles.Contains(neighborRectangle));
                } else
                {
                    Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(module.Shape));
                    Assert.IsFalse(module.Shape.AdjacentRectangles.Contains(neighborRectangle));
                }

                if (TopRectangle.y <= neighborRectangle.getTopmostYPosition() && 
                    neighborRectangle.y <= TopRectangle.getTopmostYPosition()) //They are adjacent
                {
                    Assert.IsTrue(neighborRectangle.AdjacentRectangles.Contains(TopRectangle));
                    Assert.IsTrue(TopRectangle.AdjacentRectangles.Contains(neighborRectangle));
                } else
                {
                    Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(TopRectangle));
                    Assert.IsFalse(TopRectangle.AdjacentRectangles.Contains(neighborRectangle));
                }

                //They cannot be adjacent:

                Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(RightRectangle));
                Assert.IsFalse(RightRectangle.AdjacentRectangles.Contains(neighborRectangle));
            }
            
        }


        [TestMethod]
        public void TestSideMerge2Horizontal()
        {
            int[] before = new int[]
            {
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 1);
            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestSideMerge2Vertical()
        {
            int[] before = new int[]
            {
                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                2, 2, 2,
                2, 2, 2,
                2, 2, 2
            };
            int[] after = new int[]
            {
                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 3, 1);
            RectangleTestTools.CompareRectangles(before, after, 3, 2);
        }

        [TestMethod]
        public void TestSideMerge3HorizontalBottom()
        {
            int[] before = new int[]
            {
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2,
                3, 3, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 1);
            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestSideMerge3HorizontalTop()
        {
            int[] before = new int[]
            {
                3, 3, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3,
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2,
                1, 1, 1, 2, 2, 2
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 1);
            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestSideMerge3VerticalRight()
        {
            int[] before = new int[]
            {
                1, 1, 1, 3, 3,
                1, 1, 1, 3, 3,
                1, 1, 1, 3, 3,
                2, 2, 2, 3, 3,
                2, 2, 2, 3, 3,
                2, 2, 2, 3, 3
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 1);
            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestSideMerge3VerticalLeft()
        {
            int[] before = new int[]
            {
                3, 3, 1, 1, 1,
                3, 3, 1, 1, 1,
                3, 3, 1, 1, 1,
                3, 3, 2, 2, 2,
                3, 3, 2, 2, 2,
                3, 3, 2, 2, 2
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 1);
            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestLMergeVerticalBottomRight()
        {
            int[] before = new int[]
            {
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
            };
            int[] after = new int[]
            {
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestLMergeVerticalBottomLeft()
        {
            int[] before = new int[]
            {
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
            };
            int[] after = new int[]
            {
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestLMergeVerticalTopRight()
        {
            int[] before = new int[]
            {
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
                1, 1, 2, 2, 2,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestLMergeVerticalTopLeft()
        {
            int[] before = new int[]
            {
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
                2, 2, 2, 1, 1,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                2, 2, 2, 2, 2,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1,
            };

            RectangleTestTools.CompareRectangles(before, after, 5, 2);
        }

        [TestMethod]
        public void TestLMergeHorizontalBottomLeft()
        {
            int[] before = new int[]
            {
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestLMergeHorizontalBottomRight()
        {
            int[] before = new int[]
            {
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
            };
            int[] after = new int[]
            {
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestLMergeHorizontalTopLeft()
        {
            int[] before = new int[]
            {
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestLMergeHorizontalTopRight()
        {
            int[] before = new int[]
            {
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
            };
            int[] after = new int[]
            {
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
        }

        [TestMethod]
        public void TestLAndSideMergeVerticalBottomRight()
        {
            int[] before = new int[]
            {
                3, 3, 1, 1, 0, 0, 0,
                3, 3, 1, 1, 0, 0, 0,
                3, 3, 1, 1, 0, 0, 0,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
            };
            int[] after = new int[]
            {
                1, 1, 1, 1, 0, 0, 0,
                1, 1, 1, 1, 0, 0, 0,
                1, 1, 1, 1, 0, 0, 0,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 7, 2);
            RectangleTestTools.CompareRectangles(before, after, 7, 3);
            RectangleTestTools.CompareRectangles(before, after, 7, 4);
            RectangleTestTools.CompareRectangles(before, after, 7, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeVerticalBottomLeft()
        {
            int[] before = new int[]
            {
                0, 0, 0, 1, 1, 3, 3,
                0, 0, 0, 1, 1, 3, 3,
                0, 0, 0, 1, 1, 3, 3,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
            };
            int[] after = new int[]
            {
                0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 7, 2);
            RectangleTestTools.CompareRectangles(before, after, 7, 3);
            RectangleTestTools.CompareRectangles(before, after, 7, 4);
            RectangleTestTools.CompareRectangles(before, after, 7, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeVerticalTopRight()
        {
            int[] before = new int[]
            {
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                4, 4, 1, 1, 2, 2, 2,
                3, 3, 1, 1, 0, 0, 0,
                3, 3, 1, 1, 0, 0, 0,
                3, 3, 1, 1, 0, 0, 0,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                1, 1, 1, 1, 0, 0, 0,
                1, 1, 1, 1, 0, 0, 0,
                1, 1, 1, 1, 0, 0, 0,
            };

            RectangleTestTools.CompareRectangles(before, after, 7, 2);
            RectangleTestTools.CompareRectangles(before, after, 7, 3);
            RectangleTestTools.CompareRectangles(before, after, 7, 4);
            RectangleTestTools.CompareRectangles(before, after, 7, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeVerticalTopLeft()
        {
            int[] before = new int[]
            {
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                2, 2, 2, 1, 1, 4, 4,
                0, 0, 0, 1, 1, 3, 3,
                0, 0, 0, 1, 1, 3, 3,
                0, 0, 0, 1, 1, 3, 3,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 2, 2, 2,
                0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 1, 1, 1, 1,
            };

            RectangleTestTools.CompareRectangles(before, after, 7, 2);
            RectangleTestTools.CompareRectangles(before, after, 7, 3);
            RectangleTestTools.CompareRectangles(before, after, 7, 4);
            RectangleTestTools.CompareRectangles(before, after, 7, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeHorizontalBottomLeft()
        {
            int[] before = new int[]
            {
                4, 4, 4, 4, 3, 3,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
            RectangleTestTools.CompareRectangles(before, after, 6, 3);
            RectangleTestTools.CompareRectangles(before, after, 6, 4);
            RectangleTestTools.CompareRectangles(before, after, 6, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeHorizontalBottomRight()
        {
            int[] before = new int[]
            {
                3, 3, 4, 4, 4, 4,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
            };
            int[] after = new int[]
            {
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
            RectangleTestTools.CompareRectangles(before, after, 6, 3);
            RectangleTestTools.CompareRectangles(before, after, 6, 4);
            RectangleTestTools.CompareRectangles(before, after, 6, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeHorizontalTopLeft()
        {
            int[] before = new int[]
            {
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                4, 4, 4, 4, 3, 3,
            };
            int[] after = new int[]
            {
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 0, 0,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
                2, 2, 2, 2, 1, 1,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
            RectangleTestTools.CompareRectangles(before, after, 6, 3);
            RectangleTestTools.CompareRectangles(before, after, 6, 4);
            RectangleTestTools.CompareRectangles(before, after, 6, 1);
        }

        [TestMethod]
        public void TestLAndSideMergeHorizontalTopRight()
        {
            int[] before = new int[]
            {
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                3, 3, 4, 4, 4, 4,
            };
            int[] after = new int[]
            {
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                0, 0, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
                1, 1, 2, 2, 2, 2,
            };

            RectangleTestTools.CompareRectangles(before, after, 6, 2);
            RectangleTestTools.CompareRectangles(before, after, 6, 3);
            RectangleTestTools.CompareRectangles(before, after, 6, 4);
            RectangleTestTools.CompareRectangles(before, after, 6, 1);
        }
    }
}