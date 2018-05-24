using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler;
using BiolyTests.TestObjects;
using System.Collections.Generic;

namespace BiolyTests.AssayTests
{
    [TestClass]
    public class TestAssay
    {
        [TestMethod]
        public void TestCreateEmptyAssay()
        {
            DFG<Block> dfg = new DFG<Block>();
            Assay assay = new Assay(dfg);
        }

        [TestMethod]
        public void TestCreateNonEmptyAssay()
        {
            TestBlock operation1 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation2 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation3 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation4 = new TestBlock(new List<FluidBlock> { }, null, null);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.AddNode(operation4);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
        }
        
        [TestMethod]
        public void TestCorrectIntialReadyOperationsParallelDFG()
        {
            TestBlock operation1 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation2 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation3 = new TestBlock(new List<FluidBlock> { }, null, null);
            TestBlock operation4 = new TestBlock(new List<FluidBlock> { }, null, null);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.AddNode(operation4);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.Nodes.Count);
            foreach (var node in dfg.Nodes)
            {
                //Even the pointers should be the same.
                Assert.IsTrue(assay.getReadyOperations().Contains(node.value));
            }
        }

        [TestMethod]
        public void TestCorrectIntialReadyOperationsNonParallelDFG()
        {
            TestBlock operation1 = new TestBlock(new List<FluidBlock> { }, "op1", null);
            TestBlock operation2 = new TestBlock(new List<FluidBlock> { operation1 }, "op2", null);
            TestBlock operation3 = new TestBlock(new List<FluidBlock> { }, "op3", null);
            TestBlock operation4 = new TestBlock(new List<FluidBlock> { }, "op4", null);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.AddNode(operation4);
            dfg.FinishDFG();
            
            //Now the operations associated with node 1,
            //should wait for the operation assocaited with node 0.

            Assay assay = new Assay(dfg);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.Nodes.Count - 1);
            for (int i = 0; i < dfg.Nodes.Count; i++)
            {
                bool containsOperation = assay.getReadyOperations().Contains(dfg.Nodes[i].value);
                if (i == 1)
                {
                    Assert.IsFalse(containsOperation);
                }
                else
                {
                    Assert.IsTrue(containsOperation);
                }
            }
        }

        [TestMethod]
        public void TestUpdateReadyOperations1Dependecy()
        {
            TestBlock operation1 = new TestBlock(new List<FluidBlock> { }, "op1", null);
            TestBlock operation2 = new TestBlock(new List<FluidBlock> { operation1 }, "op2", null);
            TestBlock operation3 = new TestBlock(new List<FluidBlock> { }, "op3", null);
            TestBlock operation4 = new TestBlock(new List<FluidBlock> { operation3 }, "op4", null);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.AddNode(operation4);
            dfg.FinishDFG();
            
            //Now the operations associated with node 1/3,
            //should wait for the operation assocaited with node 0/2.

            Assay assay = new Assay(dfg);

            assay.updateReadyOperations(dfg.Nodes[2].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.Nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[0].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[1].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[2].value));

            Assert.IsFalse(dfg.Nodes[0].value.isFinished);
            Assert.IsFalse(dfg.Nodes[1].value.isFinished);
            Assert.IsTrue (dfg.Nodes[2].value.isFinished);
            Assert.IsFalse(dfg.Nodes[3].value.isFinished);
        }

        [TestMethod]
        public void TestUpdateReadyOperationsMultiDependecy()
        {
            TestBlock operation1 = new TestBlock(new List<FluidBlock> { }, "op1", null);
            TestBlock operation3 = new TestBlock(new List<FluidBlock> { }, "op3", null);
            TestBlock operation2 = new TestBlock(new List<FluidBlock> { operation1, operation3 }, "op2", null);
            TestBlock operation4 = new TestBlock(new List<FluidBlock> { }, "op4", null);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.AddNode(operation4);
            dfg.FinishDFG();
            
            //Now the operations associated with node 1,
            //should wait for the operation assocaited with node 0 and 2.

            Assay assay = new Assay(dfg);

            //Remove first dependecy

            assay.updateReadyOperations(dfg.Nodes[2].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.Nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[0].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[1].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[2].value));

            Assert.IsFalse(dfg.Nodes[0].value.isFinished);
            Assert.IsFalse(dfg.Nodes[1].value.isFinished);
            Assert.IsTrue(dfg.Nodes[2].value.isFinished);
            Assert.IsFalse(dfg.Nodes[3].value.isFinished);

            //remove last dependecy
            assay.updateReadyOperations(dfg.Nodes[0].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.Nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[1].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.Nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[0].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.Nodes[2].value));

            Assert.IsTrue(dfg.Nodes[0].value.isFinished);
            Assert.IsFalse(dfg.Nodes[1].value.isFinished);
            Assert.IsTrue(dfg.Nodes[2].value.isFinished);
            Assert.IsFalse(dfg.Nodes[3].value.isFinished);
        }
        
        //[TestMethod]
        public void TestCalculateCriticalPath()
        {
            Assert.Fail("Have not been implemented yet.");
        }



    }
}
