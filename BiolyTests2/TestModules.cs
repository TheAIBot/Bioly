using System;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.BlocklyParts.Blocks.Sensors;
using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;
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
            
            Module module = new MixerModule(moduleHeight, moduleWidth, 1000);
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

            Module module = new MixerModule(moduleHeight, moduleWidth, 1000);
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

            Module module = new MixerModule(moduleHeight, moduleWidth, 1000);
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

            Module module = new MixerModule(moduleHeight, moduleWidth, 1000);
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
        public void TestSplitIntoSmallerRectanglesCorrectAdjacencies()
        {
            int rectangleHeight = 10, rectangleWidth = 10;
            int x = 5, y = 5;
            int moduleHeight = 4, moduleWidth = 6;
            Rectangle emptyRectangle = new Rectangle(rectangleWidth, rectangleHeight, x, y);
            Rectangle[] neighborRectangles = new Rectangle[3];

            int neighborWidth = 5;
            neighborRectangles[0] = new Rectangle(neighborWidth, rectangleHeight / 5, x - neighborWidth, y);
            neighborRectangles[1] = new Rectangle(neighborWidth, rectangleHeight / 6, x - neighborWidth, neighborRectangles[0].getTopmostYPosition() + 1);
            neighborRectangles[2] = new Rectangle(neighborWidth, rectangleHeight / 2, x - neighborWidth, neighborRectangles[1].getTopmostYPosition() + 1);

            for (int i = 0; i < neighborRectangles.Length; i++){
                if (i != 0) neighborRectangles[i].AdjacentRectangles.Add(neighborRectangles[i - 1]);
                if (i != neighborRectangles.Length - 1) neighborRectangles[i].AdjacentRectangles.Add(neighborRectangles[i + 1]);
                neighborRectangles[i].AdjacentRectangles.Add(emptyRectangle);
                emptyRectangle.AdjacentRectangles.Add(neighborRectangles[i]);
            }

            Assert.AreEqual(neighborRectangles.Length, emptyRectangle.AdjacentRectangles.Count); 

            Module module = new MixerModule(moduleHeight, moduleWidth, 1000);
            //It is a horizontal split.
            Tuple<Rectangle, Rectangle> splitRectangles = emptyRectangle.SplitIntoSmallerRectangles(module);
            foreach (var neighborRectangle in neighborRectangles) {
                Assert.IsFalse(neighborRectangle.AdjacentRectangles.Contains(emptyRectangle));
            }
            Assert.IsTrue(module.shape.AdjacentRectangles.Contains(splitRectangles.Item1));
            Assert.IsTrue(module.shape.AdjacentRectangles.Contains(splitRectangles.Item2));
            Assert.IsTrue(splitRectangles.Item1.AdjacentRectangles.Contains(module.shape));
            Assert.IsTrue(splitRectangles.Item2.AdjacentRectangles.Contains(module.shape));
            Assert.IsTrue(splitRectangles.Item1.AdjacentRectangles.Contains(splitRectangles.Item2));
            Assert.IsTrue(splitRectangles.Item2.AdjacentRectangles.Contains(splitRectangles.Item1));
            foreach (var neighborRectangle in neighborRectangles){
                if (neighborRectangle.y <= module.shape.getTopmostYPosition())
                {
                    Assert.IsTrue(module.shape.AdjacentRectangles.Contains(neighborRectangle));
                }   else Assert.IsFalse(module.shape.AdjacentRectangles.Contains(neighborRectangle));
            }

            //Also check removed adjacencies
            Assert.Fail("Has not been implemented yet.");
        }

    }
}
