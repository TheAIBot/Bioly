using System;
using System.Collections.Generic;
using MoreLinq;
using BiolyTests.AssayTests;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.Architechtures;
using BiolyTests.TestObjects;

namespace BiolyTests.ScheduleTests
{
    [TestClass]
    public class TestSchedule
    {
        [TestMethod]
        public void TestRemoveOperation()
        {
            List<Block> Operations = new List<Block>() { };
            int OperationsToAdd = 5;
            for (int i = 0; i < OperationsToAdd; i++)
            {
                Block Operation1 = new Sensor(null, null, null);
                Block Operation2 = new Mixer(null, null, null);
                Operation1.priority = i;
                Operation2.priority = i;
                Operations.Add(Operation1);
                Operations.Add(Operation2);
            }

            Operations[4].priority = 10;
            Block OperationThatShouldBeRemoved = Operations[4];

            Block RemovedOperation = Schedule.removeOperation(Operations);
            Assert.AreEqual(OperationsToAdd * 2 - 1, Operations.Count);
            Assert.AreEqual(OperationThatShouldBeRemoved, RemovedOperation);
        }

        

        [TestMethod]
        public void TestListSchedulingFullyParallelAssay()
        {
            String inputDroplet1 = "N1";
            String inputDroplet2 = "N2";
            DFG<Block> dfg = new DFG<Block>();

            TestBlock operation1 = new TestBlock(new List<string>() {inputDroplet1}, null, null);
            TestBlock operation2 = new TestBlock(new List<string>() {inputDroplet2}, null, null);

            Node<Block> operation1Node = new Node<Block>(operation1);
            Node<Block> operation2Node = new Node<Block>(operation2);

            dfg.AddNode(operation1Node);
            dfg.AddNode(operation2Node);

            Assay assay = new Assay(dfg);

            Board   board    = new Board(20,20);
            Droplet droplet1 = new Droplet();
            Droplet droplet2 = new Droplet();
            board.FastTemplatePlace(droplet1);
            board.FastTemplatePlace(droplet2);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputDroplet1, droplet1);
            kage.Add(inputDroplet2, droplet2);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //It should be able to schedule everything to run in parallel, so the time taken by the schedule,
            //should only be a bit bigger than the operation times. At the same time it must be larger:
            //Assert.IsTrue(<= completionTime)
            //Assert.IsTrue(completionTime <=)


            Assert.Fail();
        }

    }
}
