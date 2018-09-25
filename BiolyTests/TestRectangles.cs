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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 3, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 3, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 1);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 5, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 7, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
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

            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 2);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 3);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 4);
            RectangleTestTools.CompareBoardsAfterOptimized(before, after, 6, 1);
        }
    }
}