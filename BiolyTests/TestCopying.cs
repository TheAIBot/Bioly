using System;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.BlocklyParts.Arrays;

namespace BiolyTests
{
    [TestClass]
    public class TestCopying
    {
        [TestMethod]
        public void TestCopyArithOPBlock()
        {
            VariableBlock[] blocks = new VariableBlock[3];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new Constant(3, "b", "", false);
            blocks[2] = new ArithOP(blocks[0], blocks[1], "cake", ArithOPTypes.DIV, "", false);

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { },
                new int[] { 0, 1 }
            };

            CheckCopyMultiBlock(blocks[2], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyConstantBlock()
        {
            CheckCopySingleBlock(new Constant(10, "a", "", false));
        }

        [TestMethod]
        public void TestCopyGetNumberVariableBlock()
        {
            CheckCopySingleBlock(new GetNumberVariable("x", "a", "", new List<string>() { "x" }, false));
        }

        [TestMethod]
        public void TestCopyImportVariableBlock()
        {
            CheckCopySingleBlock(new ImportVariable("x", "a", "", false));
        }

        [TestMethod]
        public void TestCopyRoundOPBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new RoundOP(blocks[0], "cake", RoundOPTypes.ROUND, "", false);

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopySetNumberVariableBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new SetNumberVariable(blocks[0], "cake", "");

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyFluidArrayBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new FluidArray("cake", blocks[0], "");

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyGetArrayLengthBlock()
        {
            CheckCopySingleBlock(new GetArrayLength("cake", "b", "", true));
        }

        [TestMethod]
        public void TestCopyGetArrayNumberBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new GetArrayNumber(blocks[0], "cake", "b", "", true);

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyNumberArrayBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new NumberArray("cake", blocks[0], "");

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopySetArrayFluidBlock()
        {
            VariableBlock[] blocks = new VariableBlock[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new SetArrayFluid("cake", blocks[0], "");

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        private void CheckCopyMultiBlock(Block original, Block[] blocks, int[][] dependencyGraph)
        {
            DFG<Block> dfg = new DFG<Block>();
            Block copy = original.TrueCopy(dfg);
            dfg.AddNode(copy);
            CompareToOriginal(original, copy);

            Assert.AreEqual(blocks.Length, dfg.Nodes.Count, "Not all blocks were copied.");
            Assert.AreEqual(blocks.Length, dependencyGraph.Length, "Dependency graph and blocks array need to match.");
            for (int i = 0; i < blocks.Length; i++)
            {
                Node<Block> node = dfg.Nodes.SingleOrDefault(x => x.value.Equals(blocks[i]));

                Assert.IsNotNull(node, $"Can't find a copie block that matches the block: {blocks[i].GetType()}");
                Assert.AreEqual(dependencyGraph[i].Length, node.GetIngoingEdges().Count, "DFG does not meatch the dependency graph.");
                for (int y = 0; y < dependencyGraph[i].Length; y++)
                {
                    Assert.IsTrue(node.GetIngoingEdges().Any(z => z.value.Equals(blocks[dependencyGraph[i][y]])),
                        $"Block {node.value.GetType()} does not depend on block {blocks[dependencyGraph[i][y]]}.");
                }
            }
        }

        private void CheckCopySingleBlock<T>(T original) where T : Block
        {
            DFG<Block> dfg = new DFG<Block>();
            Block copy = original.TrueCopy(dfg);
            dfg.AddNode(copy);
            CompareToOriginal(original, copy);

            Node<Block> oCopy = dfg.Nodes.Single(x => x.value is T);

            Assert.AreEqual(1, dfg.Nodes.Count);
            Assert.AreEqual(0, oCopy.GetIngoingEdges().Count);
        }

        private void CompareToOriginal(Block original, Block copy)
        {
            if (original is VariableBlock && copy is VariableBlock)
            {
                VariableBlock v1 = original as VariableBlock;
                VariableBlock v2 = copy as VariableBlock;

                List<VariableBlock> v1TreeList = v1.GetVariableTreeList(new List<VariableBlock>());
                List<VariableBlock> v2TreeList = v2.GetVariableTreeList(new List<VariableBlock>());

                Assert.AreEqual(v1TreeList.Count, v2TreeList.Count);
                for (int i = 0; i < v1TreeList.Count; i++)
                {
                    CompareBlocksBaseValues(v1TreeList[i], v2TreeList[i]);
                    Assert.AreNotSame(v1TreeList[i], v2TreeList[i]);
                    Assert.AreNotEqual(v1TreeList[i].OutputVariable, Block.DEFAULT_NAME);
                    Assert.AreNotEqual(v2TreeList[i].OutputVariable, Block.DEFAULT_NAME);
                }
            }
            else if (original is FluidBlock && copy is FluidBlock)
            {
                Assert.Fail("Not implemented way to compare fluid blocks yet.");
            }
            else
            {
                Assert.Fail($"Blocks are not the same type.{Environment.NewLine} Original is of type: {original.GetType()} and copy is of type {copy.GetType()}.");
            }
        }

        private void CompareBlocksBaseValues(Block a, Block b)
        {
            Assert.AreEqual(a.GetType(), b.GetType());
            Assert.AreEqual(a.BlockID, b.BlockID);
            Assert.AreEqual(a.CanBeOutput, b.CanBeOutput);
            Assert.AreEqual(a.EndTime, b.EndTime);
            CollectionAssert.AreEquivalent(a.InputFluids.ToList(), b.InputFluids.ToList());
            CollectionAssert.AreEquivalent(a.InputNumbers, b.InputNumbers);
            Assert.AreEqual(a.IsDone, b.IsDone);
            Assert.AreEqual(a.OutputVariable, b.OutputVariable);
            Assert.AreEqual(a.priority, b.priority);
            Assert.AreEqual(a.StartTime, b.StartTime);
        }
    }
}
