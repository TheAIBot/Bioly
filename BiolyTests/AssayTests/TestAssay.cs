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



        [TestMethod]
        public void TestCreateEmptyAssay()
        {
            DFG<Block> dfg  = new DFG<Block>();

            Sensor sensor1  = new Sensor(null, null, null);
            Sensor sensor2  = new Sensor(null, null, null);
            Mixer mixer1    = new Mixer(null, null, null);
            Mixer mixer2    = new Mixer(null, null, null);

            Node<Block> sensor1Node = new Node<Block>(sensor1);
            Node<Block> sensor2Node = new Node<Block>(sensor2);
            Node<Block> mixer1Node  = new Node<Block>(mixer1);
            Node<Block> mixer2Node  = new Node<Block>(mixer2);

            dfg.AddNode(sensor1Node);
            dfg.AddNode(sensor2Node);
            dfg.AddNode(mixer1Node);
            dfg.AddNode(mixer2Node);
            
            Assay assay = new Assay(dfg);
            Assert.Fail();
        }
    }
}
