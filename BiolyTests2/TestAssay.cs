using System;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.BlocklyParts.Blocks.Sensors;
using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests.AssayTests
{
    [TestClass]
    public class TestAssay
    {

        public static DFG<Block> GetEmptyDFG()
        {
            DFG<Block> dfg = new DFG<Block>();
            return dfg;
        }

        public static DFG<Block> GetTotallyParallelDFG()
        {
            DFG<Block> dfg = new DFG<Block>();

            Sensor sensor1 = new Sensor(null, null, null);
            Sensor sensor2 = new Sensor(null, null, null);
            Mixer mixer1 = new Mixer(null, null, null);
            Mixer mixer2 = new Mixer(null, null, null);

            Node<Block> sensor1Node = new Node<Block>(sensor1);
            Node<Block> sensor2Node = new Node<Block>(sensor2);
            Node<Block> mixer1Node = new Node<Block>(mixer1);
            Node<Block> mixer2Node = new Node<Block>(mixer2);

            dfg.AddNode(sensor1Node);
            dfg.AddNode(sensor2Node);
            dfg.AddNode(mixer1Node);
            dfg.AddNode(mixer2Node);

            return dfg;
        }

        public static DFG<Block> GetSemiParallelDFG()
        {
            DFG<Block> dfg = new DFG<Block>();

            Sensor sensor1  = new Sensor(null, null, null);
            Sensor sensor2  = new Sensor(null, null, null);
            Mixer mixer1    = new Mixer(null, null, null);
            Mixer mixer2    = new Mixer(null, null, null);

            Node<Block> sensor1Node = new Node<Block>(sensor1);
            Node<Block> sensor2Node = new Node<Block>(sensor2);
            Node<Block> mixer1Node = new Node<Block>(mixer1);
            Node<Block> mixer2Node = new Node<Block>(mixer2);

            dfg.AddNode(sensor1Node);
            dfg.AddNode(sensor2Node);
            dfg.AddNode(mixer1Node);
            dfg.AddNode(mixer2Node);

            dfg.AddEdge(sensor1Node, mixer2Node);
            dfg.AddEdge(mixer1Node , mixer2Node);

            return dfg;
        }

        [TestMethod]
        public void TestCreateEmptyAssay()
        {
            DFG<Block> dfg = GetEmptyDFG();
            Assay assay = new Assay(dfg);
        }

        [TestMethod]
        public void TestCreateNonEmptyAssay()
        {
            DFG<Block> dfg = GetTotallyParallelDFG();
            Assay assay = new Assay(dfg);
        }
        
        [TestMethod]
        public void TestCorrectIntialReadyOperationsParallelDFG()
        {
            DFG<Block> dfg = GetTotallyParallelDFG();
            Assay assay = new Assay(dfg);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.nodes.Count);
            foreach (var node in dfg.nodes)
            {
                //Even the pointers should be the same.
                Assert.IsTrue(assay.getReadyOperations().Contains(node.value));
            }
        }

        [TestMethod]
        public void TestCorrectIntialReadyOperationsNonParallelDFG()
        {
            DFG<Block> dfg = GetTotallyParallelDFG();

            dfg.AddEdge(dfg.nodes[0], dfg.nodes[1]);
            //Now the operations associated with node 1,
            //should wait for the operation assocaited with node 0.

            Assay assay = new Assay(dfg);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.nodes.Count - 1);
            for (int i = 0; i < dfg.nodes.Count; i++)
            {
                bool containsOperation = assay.getReadyOperations().Contains(dfg.nodes[i].value);
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
            DFG<Block> dfg = GetTotallyParallelDFG();

            dfg.AddEdge(dfg.nodes[0], dfg.nodes[1]);
            dfg.AddEdge(dfg.nodes[2], dfg.nodes[3]);
            //Now the operations associated with node 1/3,
            //should wait for the operation assocaited with node 0/2.

            Assay assay = new Assay(dfg);

            assay.updateReadyOperations(dfg.nodes[2].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[0].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[1].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[2].value));

            Assert.IsFalse(dfg.nodes[0].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[1].value.hasBeenScheduled);
            Assert.IsTrue (dfg.nodes[2].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[3].value.hasBeenScheduled);
        }

        [TestMethod]
        public void TestUpdateReadyOperationsMultiDependecy()
        {
            DFG<Block> dfg = GetTotallyParallelDFG();

            dfg.AddEdge(dfg.nodes[0], dfg.nodes[1]);
            dfg.AddEdge(dfg.nodes[2], dfg.nodes[1]);
            //Now the operations associated with node 1,
            //should wait for the operation assocaited with node 0 and 2.

            Assay assay = new Assay(dfg);

            //Remove first dependecy

            assay.updateReadyOperations(dfg.nodes[2].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[0].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[1].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[2].value));

            Assert.IsFalse(dfg.nodes[0].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[1].value.hasBeenScheduled);
            Assert.IsTrue(dfg.nodes[2].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[3].value.hasBeenScheduled);

            //remove last dependecy
            assay.updateReadyOperations(dfg.nodes[0].value);

            Assert.AreEqual(assay.getReadyOperations().Count, dfg.nodes.Count - 2);

            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[1].value));
            Assert.IsTrue(assay.getReadyOperations().Contains(dfg.nodes[3].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[0].value));
            Assert.IsFalse(assay.getReadyOperations().Contains(dfg.nodes[2].value));

            Assert.IsTrue(dfg.nodes[0].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[1].value.hasBeenScheduled);
            Assert.IsTrue(dfg.nodes[2].value.hasBeenScheduled);
            Assert.IsFalse(dfg.nodes[3].value.hasBeenScheduled);
        }
        
        [TestMethod]
        public void TestCalculateCriticalPath()
        {
            Assert.Fail("Have not been implemented yet.");
        }



    }
}
