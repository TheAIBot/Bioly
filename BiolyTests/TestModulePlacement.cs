using System;
using BiolyTests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests
{
    [TestClass]
    public class TestModulePlacement
    {
        [TestMethod]
        public void TestPlaceBufferedModuleNoBuffer()
        {
            int[] before = new int[]
            {
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
                1, 1, 1, 1, 1,
            };

            int[] after = new int[]
            {
                -1, -1, -1, 1,  1,
                -1, -1, -1, 1,  1,
                -1, -1, -1, 1,  1,
                 2,  2,  2, 1,  1,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleBottomBuffer()
        {
            int[] before = new int[]
            {
                -1, -1, -1, -1, -1,
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
            };

            int[] after = new int[]
            {
                -1, -1, -1, -1, -1,
                -2,  1,  1,  1, -3,
                -2, -4, -4, -4, -3,
                -2, -4, -4, -4, -3,
                -2, -4, -4, -4, -3,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleLeftBuffer()
        {
            int[] before = new int[]
            {
                -3, -2, -2, -2, -2,
                -3,  1,  1,  1,  1,
                -3,  1,  1,  1,  1,
                -3,  1,  1,  1,  1,
                -3, -1, -1, -1, -1,
            };

            int[] after = new int[]
            {
                -3, -2, -2, -2, -2,
                -3,  1, -4, -4, -4,
                -3,  1, -4, -4, -4,
                -3,  1, -4, -4, -4,
                -3, -1, -1, -1, -1,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleRightBuffer()
        {
            int[] before = new int[]
            {
                -2, -2, -2, -2, -3,
                 1,  1,  1,  1, -3,
                 1,  1,  1,  1, -3,
                 1,  1,  1,  1, -3,
                -1, -1, -1, -1, -3,
            };

            int[] after = new int[]
            {
                -2, -2, -2, -2, -3,
                -4, -4, -4,  1, -3,
                -4, -4, -4,  1, -3,
                -4, -4, -4,  1, -3,
                -1, -1, -1, -1, -3,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleTopBuffer()
        {
            int[] before = new int[]
            {
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
                -2,  1,  1,  1, -3,
                -1, -1, -1, -1, -1,
            };

            int[] after = new int[]
            {
                -2, -4, -4, -4, -3,
                -2, -4, -4, -4, -3,
                -2, -4, -4, -4, -3,
                -2,  1,  1,  1, -3,
                -1, -1, -1, -1, -1,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleLeftBottomBuffers()
        {
            int[] before = new int[]
            {
                 0,  0, -1, -1,  0,  0,
                 0,  1,  1,  1,  1,  0,
                -2,  1,  1,  1,  1,  0,
                -2,  1,  1,  1,  1,  0,
                 0,  1,  1,  1,  1,  0,
                 0,  0,  0,  0,  0,  0,
            };

            int[] after = new int[]
            {
                 0,  0, -1, -1,  0,  0,
                 0,  3,  3,  3,  3,  0,
                -2,  2, -3, -3, -3,  0,
                -2,  2, -3, -3, -3,  0,
                 0,  2, -3, -3, -3,  0,
                 0,  0,  0,  0,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 6, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleRightBottomBuffers()
        {
            int[] before = new int[]
            {
                 0,  0, -1, -1,  0,  0,
                 0,  1,  1,  1,  1,  0,
                -2,  1,  1,  1,  1,  0,
                -2,  1,  1,  1,  1,  0,
                 0,  1,  1,  1,  1,  0,
                 0,  0,  0,  0,  0,  0,
            };

            int[] after = new int[]
            {
                 0,  0, -1, -1,  0,  0,
                 0,  3,  3,  3,  3,  0,
                -2,  2, -3, -3, -3,  0,
                -2,  2, -3, -3, -3,  0,
                 0,  2, -3, -3, -3,  0,
                 0,  0,  0,  0,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 6, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleTopBottomBuffers()
        {
            int[] before = new int[]
            {
                 0, -1,  0,  0,
                 1,  1,  1,  2,
                 1,  1,  1,  2,
                 1,  1,  1,  2,
                 1,  1,  1,  2,
                 1,  1,  1,  2,
                 0, -2,  0,  0,
            };

            int[] after = new int[]
            {
                  0, -1,  0,  0,
                  3,  3,  3,  2,
                 -3, -3, -3,  2,
                 -3, -3, -3,  2,
                 -3, -3, -3,  2,
                  4,  4,  4,  2,
                  0, -2,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 4, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleLeftRightBuffers()
        {
            int[] before = new int[]
            {
                 0,  1,  1,  1,  1,  1,  0,
                -1,  1,  1,  1,  1,  1, -2,
                 0,  1,  1,  1,  1,  1,  0,
                 0,  2,  2,  2,  2,  2,  0,
            };

            int[] after = new int[]
            {
                 0,  1, -3, -3, -3,  3,  0,
                -1,  1, -3, -3, -3,  3, -2,
                 0,  1, -3, -3, -3,  3,  0,
                 0,  2,  2,  2,  2,  2,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 7, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleLeftTopRightBuffers()
        {
            int[] before = new int[]
            {
                0,  0,  0, -1,  0,  0,  0,
                0,  1,  1,  1,  1,  1,  0,
                0,  1,  1,  1,  1,  1,  0,
               -4,  1,  1,  1,  1,  1, -2,
                0,  1,  1,  1,  1,  1,  0,
            };

            int[] after = new int[]
            {
                 0,  0,  0, -1,  0,  0,  0,
                 0,  1,  1,  1,  1,  3,  0,
                 0,  2, -5, -5, -5,  3,  0,
                -4,  2, -5, -5, -5,  3, -2,
                 0,  2, -5, -5, -5,  3,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 7, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleLeftBottomRightBuffers()
        {
            int[] before = new int[]
            {
                0,  1,  1,  1,  1,  1,  0,
               -4,  1,  1,  1,  1,  1, -2,
                0,  1,  1,  1,  1,  1,  0,
                0,  1,  1,  1,  1,  1,  0,
                0,  0,  0, -3,  0,  0,  0,
            };

            int[] after = new int[]
            {
                 0,  2, -5, -5, -5,  3,  0,
                -4,  2, -5, -5, -5,  3, -2,
                 0,  2, -5, -5, -5,  3,  0,
                 0,  4,  4,  4,  4,  4,  0,
                 0,  0,  0, -3,  0,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 7, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleTopLeftBottomBuffers()
        {
            int[] before = new int[]
            {
                0,  0,  0, -1,  0,
                0,  1,  1,  1,  1,
                0,  1,  1,  1,  1,
               -4,  1,  1,  1,  1,
                0,  1,  1,  1,  1,
                0,  1,  1,  1,  1,
                0,  0,  0, -3,  0,
            };

            int[] after = new int[]
            {
                 0,  0,  0, -1,  0,
                 0,  1,  1,  1,  1,
                 0,  2, -5, -5, -5,
                -4,  2, -5, -5, -5,
                 0,  2, -5, -5, -5,
                 0,  4,  4,  4,  4,
                 0,  0,  0, -3,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceBufferedModuleTopRightBottomBuffers()
        {
            int[] before = new int[]
            {
                0, -1,  0,  0,  0,
                1,  1,  1,  1,  0,
                1,  1,  1,  1,  0,
                1,  1,  1,  1, -2,
                1,  1,  1,  1,  0,
                1,  1,  1,  1,  0,
                0, -3,  0,  0,  0,
            };

            int[] after = new int[]
            {
                 0, -1,  0,  0,  0,
                 1,  1,  1,  3,  0,
                -5, -5, -5,  3,  0,
                -5, -5, -5,  3, -2,
                -5, -5, -5,  3,  0,
                 4,  4,  4,  4,  0,
                 0, -3,  0,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 5, new TestModule(3, 3, 0));
        }


        [TestMethod]
        public void TestPlaceBufferedModuleAllBuffer()
        {
            int[] before = new int[]
            {
                0,  0,  0, -1,  0,  0,  0,
                0,  1,  1,  1,  1,  1,  0,
                0,  1,  1,  1,  1,  1,  0,
               -4,  1,  1,  1,  1,  1, -2,
                0,  1,  1,  1,  1,  1,  0,
                0,  1,  1,  1,  1,  1,  0,
                0,  0,  0, -3,  0,  0,  0,
            };

            int[] after = new int[]
            {
                 0,  0,  0, -1,  0,  0,  0,
                 0,  1,  1,  1,  1,  3,  0,
                 0,  2, -5, -5, -5,  3,  0,
                -4,  2, -5, -5, -5,  3, -2,
                 0,  2, -5, -5, -5,  3,  0,
                 0,  4,  4,  4,  4,  4,  0,
                 0,  0,  0, -3,  0,  0,  0,
            };

            RectangleTestTools.CompareBoardsAfterPlacement(before, after, 7, new TestModule(3, 3, 0));
        }

        [TestMethod]
        public void TestPlaceLotsOf3x3Modules()
        {
            int[] before = new int[]
            {
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
            };

            int[] after = new int[]
            {
               -1, -1, -1, -2, -2, -2, -3, -3, -3,  2,  2,
               -1, -1, -1, -2, -2, -2, -3, -3, -3,  2,  2,
               -1, -1, -1, -2, -2, -2, -3, -3, -3,  2,  2,
                1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,
               -4, -4, -4, -5, -5, -5, -6, -6, -6,  2,  2,
               -4, -4, -4, -5, -5, -5, -6, -6, -6,  2,  2,
               -4, -4, -4, -5, -5, -5, -6, -6, -6,  2,  2,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,
            };

            var beforeBoardData = RectangleTestTools.ArrayToRectangles(before, 11);
            var afterBoardData = RectangleTestTools.ArrayToRectangles(after, 11);

            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));
            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));
            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));
            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));
            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));
            beforeBoardData.board.FastTemplatePlace(new TestModule(3, 3, 0));

            RectangleTestTools.CompareBoards(beforeBoardData.board, afterBoardData.board);
        }
    }
}
