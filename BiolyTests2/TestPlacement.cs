using System;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.BlocklyParts.Blocks.Sensors;
using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests.PlacementTests
{
    [TestClass]
    public class TestBoard
    {
        [TestMethod]
        public void TestFastTemplatePlaceEmptyBoard()
        {
            int boardHeight = 20, boardWidth = 20;
            Board board = new Board(boardWidth, boardHeight);
            Assert.AreEqual(board.EmptyRectangles.Count, 1);
            MixerModule mixer = new MixerModule(3,3,2000);
            Assert.IsTrue(board.FastTemplatePlace(mixer));
            Assert.AreEqual(2, board.EmptyRectangles.Count);
            Assert.AreEqual(2, mixer.shape.AdjacentRectangles.Count);
            foreach (var rectangle in board.EmptyRectangles) {
                mixer.shape.AdjacentRectangles.Contains(rectangle);
            }            
        }

        [TestMethod]
        public void TestFastTemplatePlaceNonEmptyBoard()
        {
            Assert.Fail();
        }


    }
}
