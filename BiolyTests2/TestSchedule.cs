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
using BiolyTests2.TestObjects;

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

            TestModule module = new TestModule();
            TestBlock operation1 = new TestBlock(new List<string>() {inputDroplet1}, null, null, module);
            TestBlock operation2 = new TestBlock(new List<string>() {inputDroplet2}, null, null, module);

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

            Assert.IsTrue(module.operationTime < completionTime);
            Assert.IsTrue(completionTime <= module.operationTime + Schedule.DROP_MOVEMENT_TIME * 20);

            Assert.AreEqual(2, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.endTime), completionTime);
            //The operations should be able to run in parallel, and as such they should be completed/start at almost the same time:
            Assert.IsTrue(Math.Abs(schedule.ScheduledOperations[0].startTime - schedule.ScheduledOperations[1].startTime) <= Schedule.DROP_MOVEMENT_TIME * 20);
            Assert.IsTrue(Math.Abs(schedule.ScheduledOperations[0].endTime - schedule.ScheduledOperations[1].endTime) <= Schedule.DROP_MOVEMENT_TIME * 20);
            Assert.IsTrue(schedule.ScheduledOperations.Contains(operation1));
            Assert.IsTrue(schedule.ScheduledOperations.Contains(operation2));
        }


        [TestMethod]
        public void TestListSchedulingSequentialAssay()
        {
            //Construction of test assay:
            String inputDroplet1 = "Kage";
            DFG<Block> dfg = new DFG<Block>();

            TestModule module = new TestModule();
            TestBlock operation1 = new TestBlock(new List<string>() { inputDroplet1 }, null, null, module);
            TestBlock operation2 = new TestBlock(new List<string>() { operation1.OutputVariable }, null, null, module);
            TestBlock operation3 = new TestBlock(new List<string>() { operation2.OutputVariable }, null, null, module);

            Node<Block> operation1Node = new Node<Block>(operation1);
            Node<Block> operation2Node = new Node<Block>(operation2);
            Node<Block> operation3Node = new Node<Block>(operation3);

            dfg.AddNode(operation1Node);
            dfg.AddNode(operation2Node);
            dfg.AddNode(operation3Node);

            dfg.AddEdge(operation1Node, operation2Node);
            dfg.AddEdge(operation2Node, operation3Node);

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Droplet droplet1 = new Droplet();
            board.FastTemplatePlace(droplet1);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputDroplet1, droplet1);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            //Everything should be run sequentially, with a little overhead in the form of droplet movement
            Assert.IsTrue(module.operationTime*dfg.Nodes.Count < completionTime);
            Assert.IsTrue(completionTime <= module.operationTime* dfg.Nodes.Count + Schedule.DROP_MOVEMENT_TIME * dfg.Nodes.Count * 20);
            
            Assert.AreEqual(dfg.Nodes.Count, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.endTime), completionTime);

            for (int i = 0; i < schedule.ScheduledOperations.Count - 1; i++){
                Block operation = schedule.ScheduledOperations[i];
                Block nextOperation = schedule.ScheduledOperations[i+1];
                Assert.IsTrue(Math.Abs(nextOperation.startTime - operation.endTime) <= Schedule.DROP_MOVEMENT_TIME * 20);
                Assert.IsTrue(operation.endTime < nextOperation.startTime);
            }

            Assert.AreEqual(operation1, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation2, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation3, schedule.ScheduledOperations[2]);

            Assert.IsTrue(schedule.boardAtDifferentTimes.All(pair => pair.Value.placedModules.Count == 1));
            
        }

        [TestMethod]
        public void TestMultipleJobsFinishesAtTheSameTime()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestListSchedulingSemiParallelAssay()
        {
            Assert.Fail("Not implemented yet. Cannot be implemented before the scheduler can take at least 2 inputs.");
        }

    }
}
