using System;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;

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
            Block[] blocks = new Block[2];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new SetArrayFluid((VariableBlock)blocks[0], "cake", new List<FluidInput>() { new BasicInput("", "qq", 10, false) }, "");

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { 0 }
            };

            CheckCopyMultiBlock(blocks[1], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopySetArrayNumberBlock()
        {
            VariableBlock[] blocks = new VariableBlock[3];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new Constant(10, "b", "", false);
            blocks[2] = new SetArrayNumber(blocks[0], blocks[1], "cake", "", false);

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { },
                new int[] { 0, 1 }
            };

            CheckCopyMultiBlock(blocks[2], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyBoolOPBlock()
        {
            VariableBlock[] blocks = new VariableBlock[3];
            blocks[0] = new Constant(10, "a", "", false);
            blocks[1] = new Constant(3, "b", "", false);
            blocks[2] = new BoolOP(blocks[0], blocks[1], "cake", BoolOPTypes.EQ, "", false);

            int[][] dependencyGraph = new int[][]
            {
                new int[] { },
                new int[] { },
                new int[] { 0, 1 }
            };

            CheckCopyMultiBlock(blocks[2], blocks, dependencyGraph);
        }

        [TestMethod]
        public void TestCopyDropletDeclarationBlock()
        {
            CheckCopySingleBlock(new DropletDeclaration("a", ""));
        }

        [TestMethod]
        public void TestCopyHeaterDeclarationBlock()
        {
            CheckCopySingleBlock(new HeaterDeclaration("a", "z", ""));
        }

        [TestMethod]
        public void TestCopyInputDeclarationBlock()
        {
            CheckCopySingleBlock(new InputDeclaration("a", 3, ""));
        }

        [TestMethod]
        public void TestCopyOutputDeclarationBlock()
        {
            CheckCopySingleBlock(new OutputDeclaration("x", "a", ""));
        }

        [TestMethod]
        public void TestCopyWastetDeclarationBlock()
        {
            CheckCopySingleBlock(new WasteDeclaration("x", "a", ""));
        }

        [TestMethod]
        public void TestCopyHeaterUsageBlock()
        {
            CheckCopySingleBlock(new HeaterUsage("a", new List<FluidInput>() { new BasicInput("", "d", 2, false) }, "q", 23, 54, ""));
        }

        [TestMethod]
        public void TestCopyMixerBlock()
        {
            CheckCopySingleBlock(new Mixer(new List<FluidInput>()
                {
                    new BasicInput("", "d", 1, false),
                    new BasicInput("", "q", 1, false)
                }, "w", ""));
        }

        [TestMethod]
        public void TestCopyUnionBlock()
        {
            CheckCopySingleBlock(new Union(new List<FluidInput>()
                {
                    new BasicInput("", "d", 1, false),
                    new BasicInput("", "q", 1, false)
                }, "w", ""));
        }

        [TestMethod]
        public void TestCopyFluidBlock()
        {
            CheckCopySingleBlock(new Fluid(new List<FluidInput>() { new BasicInput("", "d", 2, false) }, "a", ""));
        }

        [TestMethod]
        public void TestCopyFluidRefBlock()
        {
            CheckCopySingleBlock(new FluidRef("a", "b"));
        }

        [TestMethod]
        public void TestCopyGetDropletCountBlock()
        {
            CheckCopySingleBlock(new GetDropletCount("w", "a", "", false));
        }

        [TestMethod]
        public void TestCopyOutputUsageBlock()
        {
            CheckCopySingleBlock(new OutputUsage("a", new List<FluidInput>() { new BasicInput("", "d", 2, false) }, "z", ""));
        }

        [TestMethod]
        public void TestCopyWasteUsageBlock()
        {
            CheckCopySingleBlock(new WasteUsage("a", new List<FluidInput>() { new BasicInput("", "d", 2, false) }, "z", ""));
        }

        private void CheckCopyMultiBlock(Block original, Block[] blocks, int[][] dependencyGraph)
        {
            DFG<Block> dfg = new DFG<Block>();
            Block copy = original.TrueCopy(dfg);
            dfg.AddNode(copy);
            CompareToOriginal(original, copy, blocks.Length);

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
            CompareToOriginal(original, copy, 1);

            Node<Block> oCopy = dfg.Nodes.Single(x => x.value is T);

            Assert.AreEqual(1, dfg.Nodes.Count);
            Assert.AreEqual(0, oCopy.GetIngoingEdges().Count);
        }

        private void CompareToOriginal(Block original, Block copy, int blockCount)
        {
            Assert.AreEqual(original.GetType(), copy.GetType());

            List<Block> v1TreeList = original.GetBlockTreeList(new List<Block>());
            List<Block> v2TreeList = copy    .GetBlockTreeList(new List<Block>());

            Assert.AreEqual(blockCount, v1TreeList.Count);
            Assert.AreEqual(blockCount, v2TreeList.Count);
            for (int i = 0; i < blockCount; i++)
            {
                CompareBlocksBaseValues(v1TreeList[i], v2TreeList[i]);
                Assert.AreNotSame(v1TreeList[i], v2TreeList[i]);
                Assert.AreNotEqual(v1TreeList[i].OutputVariable, Block.DEFAULT_NAME);
                Assert.AreNotEqual(v2TreeList[i].OutputVariable, Block.DEFAULT_NAME);
            }
        }

        private void CompareBlocksBaseValues(Block a, Block b)
        {
            Assert.AreEqual(a.GetType(), b.GetType());
            Assert.AreEqual(a.BlockID, b.BlockID);
            Assert.AreEqual(a.CanBeOutput, b.CanBeOutput);
            Assert.AreEqual(a.EndTime, b.EndTime);
            CollectionAssert.AreEqual(a.InputFluids.ToList(), b.InputFluids.ToList());
            CollectionAssert.AreEquivalent(a.InputNumbers, b.InputNumbers);
            Assert.AreEqual(a.IsDone, b.IsDone);
            Assert.AreEqual(a.OutputVariable, b.OutputVariable);
            Assert.AreEqual(a.priority, b.priority);
            Assert.AreEqual(a.StartTime, b.StartTime);
        }
    }
}
