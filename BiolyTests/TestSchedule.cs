using System;
using System.Collections.Generic;
using MoreLinq;
using BiolyTests.AssayTests;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.Architechtures;
using BiolyTests.TestObjects;
using System.Diagnostics;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.Declarations;
using Priority_Queue;

namespace BiolyTests.ScheduleTests
{
    [TestClass]
    public class TestSchedule
    {
        [TestMethod]
        public void TestRemoveOperation()
        {
            SimplePriorityQueue<Block, int> Operations = new SimplePriorityQueue<Block, int>();
            int OperationsToAdd = 5;            
            Block OperationThatShouldBeRemoved = null;

            for (int i = 0; i < OperationsToAdd; i++)
            {
                Block Operation1 = new Sensor(null, null, null, String.Empty);
                Block Operation2 = new Mixer(null, null, String.Empty);
                Operation1.priority = i;
                Operation2.priority = i;
                if (i == 2)
                {
                    Operation1.priority = -10;
                    OperationThatShouldBeRemoved = Operation1;
                }
                Operations.Enqueue(Operation1, Operation1.priority);
                Operations.Enqueue(Operation2, Operation2.priority);
            }
            
            Block RemovedOperation = Schedule.RemoveOperation(Operations);
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
            InputDeclaration input1 = new InputDeclaration("input1", inputFluid1, 1, "");
            InputDeclaration input2 = new InputDeclaration("input2", inputFluid2, 1, "");
            TestBlock operation1 = new TestBlock(new List<FluidBlock>() { input1 }, "op1", module);
            TestBlock operation2 = new TestBlock(new List<FluidBlock>() { input2 }, "op2", module);

            dfg.AddNode(input1);
            dfg.AddNode(input2);
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);

            Board board = new Board(20, 20);
            Schedule schedule = new Schedule();
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input1, input2}, board, library);
            int completionTime = schedule.ListScheduling(assay, board, library);

            //It should be able to schedule everything to run in parallel, so the time taken by the schedule,
            //should only be a bit bigger than the operation times. At the same time it must be larger:

            Assert.IsTrue(module.OperationTime < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime + Schedule.DROP_MOVEMENT_TIME * 30*2);

            Assert.AreEqual(2, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.EndTime), completionTime);
            //The operations should be able to run in parallel, and as such they should be completed/start at almost the same time:
            Assert.IsTrue(Math.Abs(schedule.ScheduledOperations[0].StartTime - schedule.ScheduledOperations[1].StartTime) <= Schedule.DROP_MOVEMENT_TIME * 30);
            Assert.IsTrue(Math.Abs(schedule.ScheduledOperations[0].EndTime - schedule.ScheduledOperations[1].EndTime) <= Schedule.DROP_MOVEMENT_TIME * 30);
            Assert.IsTrue(schedule.ScheduledOperations.Contains(operation1));
            Assert.IsTrue(schedule.ScheduledOperations.Contains(operation2));
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));
        }


        [TestMethod]
        public void TestListSchedulingSequentialAssay()
        {
            //Construction of test assay:
            string inputFluid = "Kage";

            TestModule module = new TestModule();
            InputDeclaration input = new InputDeclaration("testModule", inputFluid, 1, "");
            TestBlock operation1 = new TestBlock(new List<FluidBlock>() { input }, "op1", module);
            TestBlock operation2 = new TestBlock(new List<FluidBlock>() { operation1 }, "op2", module);
            TestBlock operation3 = new TestBlock(new List<FluidBlock>() { operation2 }, "op3", module);

            DFG<Block> dfg = new DFG<Block>();
            dfg.AddNode(input);
            dfg.AddNode(operation1);
            dfg.AddNode(operation2);
            dfg.AddNode(operation3);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            //BoardFluid fluidType = new BoardFluid(inputFluid);
            //Droplet droplet1 = new Droplet(fluidType);
            //board.FastTemplatePlace(droplet1);

            Dictionary<string, BoardFluid> kage = new Dictionary<string, BoardFluid>();
            //kage.Add(inputFluid, fluidType);
            Schedule schedule = new Schedule();
            schedule.TransferFluidVariableLocationInformation(kage);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input}, board, library);
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            //Everything should be run sequentially, with a little overhead in the form of droplet movement
            Assert.IsTrue(module.OperationTime * (dfg.Nodes.Count-1) < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime * (dfg.Nodes.Count - 1) + Schedule.DROP_MOVEMENT_TIME * (dfg.Nodes.Count - 1) * 20);

            Assert.AreEqual(dfg.Nodes.Count - 1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.EndTime), completionTime);

            for (int i = 0; i < schedule.ScheduledOperations.Count - 1; i++)
            {
                Block operation = schedule.ScheduledOperations[i];
                Block nextOperation = schedule.ScheduledOperations[i + 1];
                Assert.IsTrue(Math.Abs(nextOperation.StartTime - operation.EndTime) <= Schedule.DROP_MOVEMENT_TIME * 20);
                Assert.IsTrue(operation.EndTime < nextOperation.StartTime);
            }

            Assert.AreEqual(operation1, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation2, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation3, schedule.ScheduledOperations[2]);

            List<KeyValuePair<int, Board>> boardsAtDifferentTimes = schedule.boardAtDifferentTimes.ToList();
            boardsAtDifferentTimes.Sort((x, y) => x.Key <= y.Key ? 0 : 1);
            for (int i = 0; i < boardsAtDifferentTimes.Count; i++)
            {
                Debug.WriteLine("Time: " + boardsAtDifferentTimes[i].Key);
                Debug.WriteLine(boardsAtDifferentTimes[i].Value.print(schedule.AllUsedModules));
            }
            Assert.IsTrue(schedule.boardAtDifferentTimes.All(pair => pair.Value.PlacedModules.Count == 3 || pair.Key == 0));
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));

        }

        [TestMethod]
        public void TestMultipleJobsFinishesAtTheSameTime()
        {
            String inputFluid1 = "Kage1";
            String inputFluid2 = "Kage2";
            DFG<Block> dfg = new DFG<Block>();

            TestModule module = new TestModule();
            InputDeclaration input1 = new InputDeclaration("testModule1", inputFluid1, 1, "");
            InputDeclaration input2 = new InputDeclaration("testModule2", inputFluid2, 1, "");
            TestBlock operation11 = new TestBlock(new List<FluidBlock>() { input1 }, "op11", module);
            TestBlock operation21 = new TestBlock(new List<FluidBlock>() { operation11 }, "op21", module);
            TestBlock operation31 = new TestBlock(new List<FluidBlock>() { operation21 }, "op31", module);
            TestBlock operation12 = new TestBlock(new List<FluidBlock>() { input2 }, "op12", module);
            TestBlock operation22 = new TestBlock(new List<FluidBlock>() { operation12 }, "op22", module);
            TestBlock operation32 = new TestBlock(new List<FluidBlock>() { operation22 }, "op32", module);

            dfg.AddNode(input1);
            dfg.AddNode(input2);
            dfg.AddNode(operation11);
            dfg.AddNode(operation21);
            dfg.AddNode(operation31);
            dfg.AddNode(operation12);
            dfg.AddNode(operation22);
            dfg.AddNode(operation32);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Schedule schedule = new Schedule();
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input1, input2 }, board, library);
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:
            int numberOfInputs = 2;
            int sequentialOperationLineLenght = (dfg.Nodes.Count - numberOfInputs)/2;

            Assert.IsTrue(module.OperationTime * sequentialOperationLineLenght < completionTime);
            Assert.IsTrue(completionTime <= module.OperationTime * sequentialOperationLineLenght + Schedule.DROP_MOVEMENT_TIME * sequentialOperationLineLenght * 60);

            Assert.AreEqual(dfg.Nodes.Count - numberOfInputs, schedule.ScheduledOperations.Count);
            Assert.AreEqual(schedule.ScheduledOperations.Max(operation => operation.EndTime), completionTime);

            for (int i = 0; i < schedule.ScheduledOperations.Count - 2; i++)
            {
                Block operation = schedule.ScheduledOperations[i];
                //+2 instead of +1, as there are two parallel rows of sequentiel operations running
                Block nextOperation = schedule.ScheduledOperations[i + 2];
                Assert.IsTrue(Math.Abs(nextOperation.StartTime - operation.EndTime) <= Schedule.DROP_MOVEMENT_TIME * 50);
                Assert.IsTrue(operation.EndTime < nextOperation.StartTime);
            }


            Assert.AreEqual(operation11, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation12, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation21, schedule.ScheduledOperations[2]);
            Assert.AreEqual(operation22, schedule.ScheduledOperations[3]);
            Assert.AreEqual(operation31, schedule.ScheduledOperations[4]);
            Assert.AreEqual(operation32, schedule.ScheduledOperations[5]);

            //It is sorted:
            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();

            Assert.AreEqual(3, boardAtDifferentTimes[0].PlacedModules.Count);
            for (int i = 1; i < boardAtDifferentTimes.Count; i++)
            {
                Assert.AreEqual(5, boardAtDifferentTimes[i].PlacedModules.Count);
            }
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));

        }

        [TestMethod]
        public void TestListSchedulingSingleModuleSingleInputSingleOutputAssay()
        {
            //Construction of test assay:
            String inputFluid1 = "Fisk";
            DFG<Block> dfg = new DFG<Block>();
            TestModule module = new TestModule(1,1);
            InputDeclaration input1 = new InputDeclaration("testModule", inputFluid1, 1, "");
            TestBlock operation1 = new TestBlock(new List<FluidBlock>() {input1}, null, module);

            dfg.AddNode(input1);
            dfg.AddNode(operation1);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            BoardFluid fluidType1 = new BoardFluid(inputFluid1);
            Schedule schedule = new Schedule();
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input1 }, board, library);
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(board));

            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            List<Board> boards = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();

            Assert.AreEqual(2, boards[0].PlacedModules.Count);
            for (int i = 1; i < boards.Count; i++)
            {
                Assert.AreEqual(3, boards[i].PlacedModules.Count);
            }
            
            //The second last board should be where the module implementing the operation should be placed.
            Assert.AreEqual(module.GetType(), boards[boards.Count - 2].PlacedModules.Values.Select(placedModule => placedModule.GetType()).Last());
            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(operation1, schedule.ScheduledOperations[0]);
            Assert.IsTrue(module.OperationTime <= completionTime);
            //One droplets are routed, and it should for such a simple board, take at most 25 movements.
            Assert.IsTrue(completionTime <= module.OperationTime + Schedule.DROP_MOVEMENT_TIME * 25);
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));
        }

        //[TestMethod]
        public void TestListSchedulingSingleModuleMultiInputMultiOutputAssay()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestListSchedulingSingleModuleMultiInputOneOutputAssay()
        {
            //Construction of test assay:
            String inputFluid1 = "Fisk";
            String inputFluid2 = "Kage";
            DFG<Block> dfg = new DFG<Block>();
            TestModule module = new TestModule(2, 1);
            InputDeclaration input1 = new InputDeclaration("testModule1", inputFluid1, 1, "");
            InputDeclaration input2 = new InputDeclaration("testModule2", inputFluid2, 1, "");
            TestBlock operation1 = new TestBlock(new List<FluidBlock>() { input1, input2 }, "op1", module);

            dfg.AddNode(input1);
            dfg.AddNode(input2);
            dfg.AddNode(operation1);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Schedule schedule = new Schedule();
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input1, input2}, board, library);
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(board));

            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing the results:

            List<Board> boards = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();

            for (int i = 1; i < boards.Count; i++)
            {
                Assert.AreEqual(4, boards[i].PlacedModules.Count);
            }



            //The second last board should be where the module implementing the operation should be placed.
            Assert.AreEqual(module.GetType(), boards[boards.Count - 2].PlacedModules.Values.Select(placedModule => placedModule.GetType()).Last());
            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(operation1, schedule.ScheduledOperations[0]);
            Assert.IsTrue(module.OperationTime <= completionTime);
            //Two droplets are routed, and they should for such a simple board, take at most 20 movements.
            Assert.IsTrue(completionTime <= module.OperationTime + Schedule.DROP_MOVEMENT_TIME * 30 * 2);
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));
        }

        [TestMethod]
        public void TestListSchedulingSemiParallelAssay()
        {
            //Seting up the test assay:
            String inputFluid1 = "Kage1";
            String inputFluid2 = "Kage2";
            String inputFluid3 = "Kage3";
            DFG<Block> dfg = new DFG<Block>();

            int numberOfInputes = 3;
            TestModule sequentialModule1 = new TestModule();
            TestModule sequentialModule2 = new TestModule(4, 4, 1500); //Different operation time, to check if it the schedule takes the max of the two input operation times.
            TestModule multiInputModule = new TestModule(2, 1);
            InputDeclaration input1 = new InputDeclaration("testModule1", inputFluid1, 1, "");
            InputDeclaration input2 = new InputDeclaration("testModule2", inputFluid2, 1, "");
            InputDeclaration input3 = new InputDeclaration("testModule3", inputFluid3, 1, "");
            TestBlock operation11 = new TestBlock(new List<FluidBlock>() { input1 }, "op11", sequentialModule2);
            TestBlock operation21 = new TestBlock(new List<FluidBlock>() { input2 }, "op21", sequentialModule1);
            TestBlock operation31 = new TestBlock(new List<FluidBlock>() { operation11, operation21 }, "op31", multiInputModule);

            TestBlock operation12 = new TestBlock(new List<FluidBlock>() { input3 }, "op12", sequentialModule1);
            TestBlock operation22 = new TestBlock(new List<FluidBlock>() { operation12 }, "op22", sequentialModule1);
            TestBlock operation32 = new TestBlock(new List<FluidBlock>() { operation22 }, "op32", sequentialModule2);

            TestBlock operationLast = new TestBlock(new List<FluidBlock>() { operation31, operation32 }, "opL", multiInputModule);

            dfg.AddNode(input1);
            dfg.AddNode(input2);
            dfg.AddNode(input3);
            dfg.AddNode(operation11);
            dfg.AddNode(operation21);
            dfg.AddNode(operation31);
            dfg.AddNode(operation12);
            dfg.AddNode(operation22);
            dfg.AddNode(operation32);

            dfg.AddNode(operationLast);
            dfg.FinishDFG();

            Assay assay = new Assay(dfg);
            //Scheduling the assay:
            Board board = new Board(20, 20);
            Schedule schedule = new Schedule();
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { input1, input2, input3 }, board, library);
            int completionTime = schedule.ListScheduling(assay, board, library);

            //Testing:

            //Does not include droplet routing:
            int estimatedCompletionTimeFirstRouteMixing = Math.Max(operation11.associatedModule.OperationTime, operation21.associatedModule.OperationTime) + operation31.associatedModule.OperationTime;
            int estimatedCompletionTimeFinalMixing = Math.Max(estimatedCompletionTimeFirstRouteMixing, operation12.associatedModule.OperationTime +
                                                                                                       operation22.associatedModule.OperationTime +
                                                                                                       operation32.associatedModule.OperationTime) + operationLast.associatedModule.OperationTime;
            Assert.IsTrue(estimatedCompletionTimeFinalMixing <= completionTime && completionTime <= estimatedCompletionTimeFinalMixing + Schedule.DROP_MOVEMENT_TIME * 50 * 6);
            Assert.IsTrue(sequentialModule2.OperationTime <= operation11.EndTime && operation11.EndTime <= sequentialModule2.OperationTime + Schedule.DROP_MOVEMENT_TIME * 50);
            Assert.IsTrue(sequentialModule1.OperationTime <= operation21.EndTime && operation11.EndTime <= sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME * 50);
            Assert.IsTrue(estimatedCompletionTimeFirstRouteMixing <= operation31.EndTime && operation31.EndTime <= estimatedCompletionTimeFirstRouteMixing + Schedule.DROP_MOVEMENT_TIME * 50 * 3);
            Assert.IsTrue(sequentialModule1.OperationTime <= operation12.EndTime && operation12.EndTime <= 2 * sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME * 50 * 3);
            Assert.IsTrue(2 * sequentialModule1.OperationTime <= operation22.EndTime && operation22.EndTime <= 2 * sequentialModule1.OperationTime + Schedule.DROP_MOVEMENT_TIME * 50 * 4);
            Assert.IsTrue(2 * sequentialModule1.OperationTime + sequentialModule2.OperationTime <= operation32.EndTime && operation32.EndTime <= 2 * sequentialModule1.OperationTime + sequentialModule2.OperationTime + Schedule.DROP_MOVEMENT_TIME * 50 * 5);

            Assert.AreEqual(dfg.Nodes.Count, schedule.ScheduledOperations.Count + numberOfInputes);
            Assert.AreEqual(operation11, schedule.ScheduledOperations[0]);
            Assert.AreEqual(operation21, schedule.ScheduledOperations[1]);
            Assert.AreEqual(operation12, schedule.ScheduledOperations[2]);
            Assert.AreEqual(operation31, schedule.ScheduledOperations[3]);
            Assert.AreEqual(operation22, schedule.ScheduledOperations[4]);
            Assert.AreEqual(operation32, schedule.ScheduledOperations[5]);
            Assert.AreEqual(operationLast, schedule.ScheduledOperations[6]);
            Board lastBoard = schedule.boardAtDifferentTimes.ToList().OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().Last();
            Assert.IsTrue(BiolyTests.PlacementTests.TestBoard.doAdjacencyGraphContainTheCorrectNodes(lastBoard));
        }
    }
}
