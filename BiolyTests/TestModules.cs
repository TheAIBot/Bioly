using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests.ModuleTests
{
    [TestClass]
    public class TestModules
    {
        [TestMethod]
        public void TestSplitIntoSmallerRectanglesSmallVerticalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 8, moduleWidth = 4;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);
            
            Module module = new MixerModule(moduleWidth, moduleHeight, 1000);
            Tuple<Rectangle, Rectangle> splitRectangles = rectangle.SplitIntoSmallerRectangles(module);
            
            Assert.AreEqual(x, splitRectangles.Item1.x);
            Assert.AreEqual(x + moduleWidth, splitRectangles.Item2.x);

            Assert.AreEqual(y + moduleHeight, splitRectangles.Item1.y);
            Assert.AreEqual(y, splitRectangles.Item2.y);
            
            Assert.AreEqual(rectangleHeight - moduleHeight, splitRectangles.Item1.height);
            Assert.AreEqual(rectangleHeight, splitRectangles.Item2.height);

            Assert.AreEqual(moduleWidth, splitRectangles.Item1.width);
            Assert.AreEqual(rectangleWidth - moduleWidth, splitRectangles.Item2.width);
        }


        [TestMethod]
        public void TestSplitIntoSmallerRectanglesSmallHorizontalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 8;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new MixerModule(moduleWidth, moduleHeight, 1000);
            Tuple<Rectangle, Rectangle> splitRectangles = rectangle.SplitIntoSmallerRectangles(module);

            Assert.AreEqual(x, splitRectangles.Item1.x);
            Assert.AreEqual(x + moduleWidth, splitRectangles.Item2.x);

            Assert.AreEqual(y + moduleHeight, splitRectangles.Item1.y);
            Assert.AreEqual(y, splitRectangles.Item2.y);

            Assert.AreEqual(rectangleHeight - moduleHeight, splitRectangles.Item1.height);
            Assert.AreEqual(moduleHeight, splitRectangles.Item2.height);

            Assert.AreEqual(rectangleWidth, splitRectangles.Item1.width);
            Assert.AreEqual(rectangleWidth - moduleWidth, splitRectangles.Item2.width);
        }

        [TestMethod]
        public void TestSplitIntoSmallerRectanglesNoHorizontalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 10;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new MixerModule(moduleWidth, moduleHeight, 1000);
            Tuple<Rectangle, Rectangle> splitRectangles = rectangle.SplitIntoSmallerRectangles(module);

            Assert.AreEqual(null, splitRectangles.Item2);

            Assert.AreEqual(x, splitRectangles.Item1.x);
            Assert.AreEqual(y + moduleHeight, splitRectangles.Item1.y);
            Assert.AreEqual(rectangleHeight - moduleHeight, splitRectangles.Item1.height);
            Assert.AreEqual(rectangleWidth, splitRectangles.Item1.width);
        }


        [TestMethod]
        public void TestSplitIntoSmallerRectanglesNoVerticalSegment()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 10, moduleWidth = 4;
            Rectangle rectangle = new Rectangle(rectangleWidth, rectangleHeight);
            rectangle.PlaceAt(x, y);

            Module module = new MixerModule(moduleWidth, moduleHeight, 1000);
            Tuple<Rectangle, Rectangle> splitRectangles = rectangle.SplitIntoSmallerRectangles(module);

            Assert.AreEqual(null, splitRectangles.Item1);

            Assert.AreEqual(x + moduleWidth, splitRectangles.Item2.x);
            Assert.AreEqual(y, splitRectangles.Item2.y);
            Assert.AreEqual(rectangleHeight, splitRectangles.Item2.height);
            Assert.AreEqual(rectangleWidth - moduleWidth, splitRectangles.Item2.width);
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

        [TestMethod]
        public void TestIsAdjacentRightRectangle() {
            Assert.Fail("Has not been implemented yet");
        }

        [TestMethod]
        public void TestIsAdjacentTopRectangle()
        {
            Assert.Fail("Has not been implemented yet");
        }

        [TestMethod]
        public void TestIsAdjacentLowerRectangle()
        {
            Assert.Fail("Has not been implemented yet");
        }

        [TestMethod]
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
            Module module = new MixerModule(moduleWidth, moduleHeight, 1000);

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
                Tuple<Rectangle, Rectangle> splitRectangles = emptyRectangle.SplitIntoSmallerRectangles(module);
                Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(emptyRectangle));
                Assert.IsTrue(module.shape.AdjacentRectangles.Contains(splitRectangles.Item1));
                Assert.IsTrue(module.shape.AdjacentRectangles.Contains(splitRectangles.Item2));
                Assert.IsTrue(splitRectangles.Item1.AdjacentRectangles.Contains(module.shape));
                Assert.IsTrue(splitRectangles.Item2.AdjacentRectangles.Contains(module.shape));
                Assert.IsTrue(splitRectangles.Item1.AdjacentRectangles.Contains(splitRectangles.Item2));
                Assert.IsTrue(splitRectangles.Item2.AdjacentRectangles.Contains(splitRectangles.Item1));

                if (neighborRectangle.y <= module.shape.getTopmostYPosition()) //They are adjacent
                {
                    Assert.IsTrue(neighborRectangle.AdjacentRectangles.Contains(module.shape));
                    Assert.IsTrue(module.shape.AdjacentRectangles.Contains(neighborRectangle));
                } else
                {
                    Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(module.shape));
                    Assert.IsFalse(module.shape.AdjacentRectangles.Contains(neighborRectangle));
                }

                if (splitRectangles.Item1.y <= neighborRectangle.getTopmostYPosition() && 
                    neighborRectangle.y <= splitRectangles.Item1.getTopmostYPosition()) //They are adjacent
                {
                    Assert.IsTrue(neighborRectangle.AdjacentRectangles.Contains(splitRectangles.Item1));
                    Assert.IsTrue(splitRectangles.Item1.AdjacentRectangles.Contains(neighborRectangle));
                } else
                {
                    Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(splitRectangles.Item1));
                    Assert.IsFalse(splitRectangles.Item1.AdjacentRectangles.Contains(neighborRectangle));
                }

                //They cannot be adjacent:

                Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(splitRectangles.Item2));
                Assert.IsFalse(splitRectangles.Item2.AdjacentRectangles.Contains(neighborRectangle));
            }
            
        }

    }
}
