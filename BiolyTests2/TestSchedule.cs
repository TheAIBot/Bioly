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
            String inputFluid1 = "Fluid1";
            String inputFluid2 = "Fluid2";
            DFG<Block> dfg = new DFG<Block>();

            TestModule module = new TestModule();
            TestBlock operation1 = new TestBlock(new List<string>() {inputFluid1}, null, null, module);
            TestBlock operation2 = new TestBlock(new List<string>() {inputFluid2}, null, null, module);

            Node<Block> operation1Node = new Node<Block>(operation1);
            Node<Block> operation2Node = new Node<Block>(operation2);

            dfg.AddNode(operation1Node);
            dfg.AddNode(operation2Node);

            Assay assay = new Assay(dfg);

            Board   board    = new Board(20,20);
            Droplet droplet1 = new Droplet(new BoardFluid(inputFluid1));
            Droplet droplet2 = new Droplet(new BoardFluid(inputFluid2));
            board.FastTemplatePlace(droplet1);
            board.FastTemplatePlace(droplet2);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputFluid1, droplet1);
            kage.Add(inputFluid2, droplet2);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //It should be able to schedule everything to run in parallel, so the time taken by the schedule,
            //should only be a bit bigger than the operation times. At the same time it must be larger:

            Assert.IsTrue(module.OperationTime < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime + Schedule.DROP_MOVEMENT_TIME * 20);

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
            String inputFluid = "Kage";
            DFG<Block> dfg = new DFG<Block>();

            TestModule module = new TestModule();
            TestBlock operation1 = new TestBlock(new List<string>() { inputFluid }, null, null, module);
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
            Droplet droplet1 = new Droplet(new BoardFluid("TestFluid"));
            board.FastTemplatePlace(droplet1);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputFluid, droplet1);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            //Everything should be run sequentially, with a little overhead in the form of droplet movement
            Assert.IsTrue(module.OperationTime*dfg.Nodes.Count < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime* dfg.Nodes.Count + Schedule.DROP_MOVEMENT_TIME * dfg.Nodes.Count * 20);
            
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
            String inputFluid1 = "Kage1";
            String inputFluid2 = "Kage2";
            DFG<Block> dfg = new DFG<Block>();

            TestModule module = new TestModule();
            TestBlock operation11 = new TestBlock(new List<string>() { inputFluid1 }, null, null, module);
            TestBlock operation21 = new TestBlock(new List<string>() { operation11.OutputVariable }, null, null, module);
            TestBlock operation31 = new TestBlock(new List<string>() { operation21.OutputVariable }, null, null, module);
            TestBlock operation12 = new TestBlock(new List<string>() { inputFluid2 }, null, null, module);
            TestBlock operation22 = new TestBlock(new List<string>() { operation12.OutputVariable }, null, null, module);
            TestBlock operation32 = new TestBlock(new List<string>() { operation22.OutputVariable }, null, null, module);

            Node<Block> operation11Node = new Node<Block>(operation11);
            Node<Block> operation21Node = new Node<Block>(operation21);
            Node<Block> operation31Node = new Node<Block>(operation31);

            Node<Block> operation12Node = new Node<Block>(operation12);
            Node<Block> operation22Node = new Node<Block>(operation22);
            Node<Block> operation32Node = new Node<Block>(operation32);

            dfg.AddNode(operation11Node);
            dfg.AddNode(operation21Node);
            dfg.AddNode(operation31Node);
            dfg.AddNode(operation12Node);
            dfg.AddNode(operation22Node);
            dfg.AddNode(operation32Node);

            dfg.AddEdge(operation11Node, operation21Node);
            dfg.AddEdge(operation21Node, operation31Node);

            dfg.AddEdge(operation12Node, operation22Node);
            dfg.AddEdge(operation22Node, operation32Node);

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Droplet droplet1 = new Droplet(new BoardFluid("TestFluid1"));
            Droplet droplet2 = new Droplet(new BoardFluid("TestFluid2"));
            board.FastTemplatePlace(droplet1);
            board.FastTemplatePlace(droplet2);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputFluid1, droplet1);
            kage.Add(inputFluid2, droplet2);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            Assert.IsTrue(module.OperationTime * dfg.Nodes.Count/2 < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime * dfg.Nodes.Count/2 + Schedule.DROP_MOVEMENT_TIME * dfg.Nodes.Count / 2 * 20);

            Assert.AreEqual(dfg.Nodes.Count, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.endTime), completionTime);

            for (int i = 0; i < schedule.ScheduledOperations.Count - 2; i++){
                Block operation = schedule.ScheduledOperations[i];
                //+2 instead of +1, as there are two parallel rows of sequentiel operations running
                Block nextOperation = schedule.ScheduledOperations[i + 2]; 
                Assert.IsTrue(Math.Abs(nextOperation.startTime - operation.endTime) <= Schedule.DROP_MOVEMENT_TIME * 30);
                Assert.IsTrue(operation.endTime < nextOperation.startTime);
            }


            Assert.AreEqual(operation11, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation12, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation21, schedule.ScheduledOperations[2]);
            Assert.AreEqual(operation22, schedule.ScheduledOperations[3]);
            Assert.AreEqual(operation31, schedule.ScheduledOperations[4]);
            Assert.AreEqual(operation32, schedule.ScheduledOperations[5]);

            Assert.IsTrue(schedule.boardAtDifferentTimes.All(pair => pair.Value.placedModules.Count == 2));
            
        }


        [TestMethod]
        public void TestListSchedulingSingleModuleMultiInputMultiOutputAssay()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestListSchedulingSingleModuleMultiInputOneOutputAssay() {
            //Construction of test assay:
            String inputFluid1 = "Fisk";
            String inputFluid2 = "Kage";
            DFG<Block> dfg = new DFG<Block>();
            TestModule module = new TestModule(2, 1);
            TestBlock operation1 = new TestBlock(new List<string>() { inputFluid1, inputFluid2 }, null, null, module);

            Node<Block> operation1Node = new Node<Block>(operation1);

            dfg.AddNode(operation1Node);
            
            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Droplet droplet1 = new Droplet(new BoardFluid("TestFluid1"));
            Droplet droplet2 = new Droplet(new BoardFluid("TestFluid2"));
            board.FastTemplatePlace(droplet1);
            board.FastTemplatePlace(droplet2);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputFluid1, droplet1);
            kage.Add(inputFluid2, droplet2);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:
            
            List<Board> boards = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();

            for (int i = 1; i < boards.Count; i++)
            {
                Assert.AreEqual(1, boards[i].placedModules.Count);
            }

            //The second last board should be where the module implementing the operation should be placed.
            Assert.AreEqual(module.GetType(), boards[boards.Count - 2].placedModules[0].GetType());
            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(operation1, schedule.ScheduledOperations[0]);
            Assert.IsTrue(module.OperationTime <= completionTime);
            //Two droplets are routed, and they should for such a simple board, take at most 20 movements.
            Assert.IsTrue(completionTime <= module.OperationTime + Schedule.DROP_MOVEMENT_TIME * 20 * 2);
        }

        [TestMethod]
        public void TestListSchedulingSemiParallelAssay()
        {
            //Seting up the test assay:
            String inputFluid1 = "Kage1";
            String inputFluid2 = "Kage2";
            String inputFluid3 = "Kage3";
            DFG<Block> dfg = new DFG<Block>();

            TestModule sequentialModule1 = new TestModule();
            TestModule sequentialModule2 = new TestModule(4, 4, 1500); //Different operation time, to check if it the schedule takes the max of the two input operation times.
            TestModule multiInputModule = new TestModule(1, 2);
            TestBlock operation11 = new TestBlock(new List<string>() { inputFluid1 }, null, null, sequentialModule2);
            TestBlock operation21 = new TestBlock(new List<string>() { inputFluid3 }, null, null, sequentialModule1);
            TestBlock operation31 = new TestBlock(new List<string>() { operation11.OutputVariable, operation21.OutputVariable }, null, null, multiInputModule);

            TestBlock operation12 = new TestBlock(new List<string>() { inputFluid2 }, null, null, sequentialModule1);
            TestBlock operation22 = new TestBlock(new List<string>() { operation12.OutputVariable }, null, null, sequentialModule1);
            TestBlock operation32 = new TestBlock(new List<string>() { operation22.OutputVariable }, null, null, sequentialModule2);

            TestBlock operationLast = new TestBlock(new List<string>() { operation31.OutputVariable, operation32.OutputVariable }, null, null, multiInputModule);


            Node<Block> operation11Node = new Node<Block>(operation11);
            Node<Block> operation21Node = new Node<Block>(operation21);
            Node<Block> operation31Node = new Node<Block>(operation31);

            Node<Block> operation12Node = new Node<Block>(operation12);
            Node<Block> operation22Node = new Node<Block>(operation22);
            Node<Block> operation32Node = new Node<Block>(operation32);

            Node<Block> operationLastNode = new Node<Block>(operationLast);

            dfg.AddNode(operation11Node);
            dfg.AddNode(operation21Node);
            dfg.AddNode(operation31Node);
            dfg.AddNode(operation12Node);
            dfg.AddNode(operation22Node);
            dfg.AddNode(operation32Node);

            dfg.AddNode(operationLastNode);

            dfg.AddEdge(operation11Node, operation31Node);
            dfg.AddEdge(operation21Node, operation31Node); 

            dfg.AddEdge(operation12Node, operation22Node);
            dfg.AddEdge(operation22Node, operation32Node);
            
            dfg.AddEdge(operation31Node, operationLastNode);
            dfg.AddEdge(operation32Node, operationLastNode);

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Droplet droplet1 = new Droplet(new BoardFluid("TestFluid1"));
            Droplet droplet2 = new Droplet(new BoardFluid("TestFluid2"));
            Droplet droplet3 = new Droplet(new BoardFluid("TestFluid3"));
            board.FastTemplatePlace(droplet1);
            board.FastTemplatePlace(droplet2);
            board.FastTemplatePlace(droplet3);
            Dictionary<string, Droplet> kage = new Dictionary<string, Droplet>();
            kage.Add(inputFluid1, droplet1);
            kage.Add(inputFluid2, droplet2);
            kage.Add(inputFluid3, droplet3);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing:

            //Does not include droplet routing:
            int estimatedCompletionTimeFirstRouteMixing = Math.Max(operation11.associatedModule.OperationTime, operation21.associatedModule.OperationTime) + operation31.associatedModule.OperationTime ;
            int estimatedCompletionTimeFinalMixing = Math.Max(estimatedCompletionTimeFirstRouteMixing, operation12.associatedModule.OperationTime +
                                                                                                       operation22.associatedModule.OperationTime +
                                                                                                       operation32.associatedModule.OperationTime) + operationLast.associatedModule.OperationTime;
            Assert.IsTrue(estimatedCompletionTimeFinalMixing <= completionTime   && completionTime      <= estimatedCompletionTimeFinalMixing + Schedule.DROP_MOVEMENT_TIME*20*6);
            Assert.IsTrue(sequentialModule2.OperationTime   <= operation11.endTime && operation11.endTime <= sequentialModule2.OperationTime + Schedule.DROP_MOVEMENT_TIME*20);
            Assert.IsTrue(sequentialModule1.OperationTime   <= operation21.endTime && operation11.endTime <= sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME*20);
            Assert.IsTrue(estimatedCompletionTimeFirstRouteMixing <= operation31.endTime && operation31.endTime <= estimatedCompletionTimeFirstRouteMixing + Schedule.DROP_MOVEMENT_TIME * 20 * 3);
            Assert.IsTrue(sequentialModule1.OperationTime   <= operation12.endTime && operation12.endTime <= 2*sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME * 20*3);
            Assert.IsTrue(2*sequentialModule1.OperationTime <= operation22.endTime && operation22.endTime <= 2*sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME * 20*4);
            Assert.IsTrue(2*sequentialModule1.OperationTime + sequentialModule2.OperationTime <= operation32.endTime && operation32.endTime <= 2 * sequentialModule1.OperationTime + sequentialModule2.OperationTime + Schedule.DROP_MOVEMENT_TIME * 20 * 5);

            Assert.AreEqual(dfg.Nodes.Count, schedule.ScheduledOperations.Count);
            Assert.AreEqual(operation11, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation21, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation12, schedule.ScheduledOperations[2]);
            Assert.AreEqual(operation31, schedule.ScheduledOperations[3]);
            Assert.AreEqual(operation22, schedule.ScheduledOperations[4]);
            Assert.AreEqual(operation32, schedule.ScheduledOperations[5]);
            Assert.AreEqual(operationLast, schedule.ScheduledOperations[6]);
        }

    }
}
