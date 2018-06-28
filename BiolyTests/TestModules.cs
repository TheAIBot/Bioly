using System;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Routing;
using BiolyCompiler.Scheduling;
using BiolyTests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests.ModulesTests
{
    [TestClass]
    public class TestModules
    {


        [TestMethod]
        public void TestMixer()
        {
            DFG<Block> dfg = new DFG<Block>();
            string op1Name = "op1";
            string op2Name = "op2";
            InputDeclaration inputOperation1 = new InputDeclaration("kage", op1Name, 1, "");
            InputDeclaration inputOperation2 = new InputDeclaration("fisk", op2Name, 1, "");
            Mixer mixingOperation = new Mixer(new List<FluidInput>() {new BasicInput("", inputOperation1.OriginalOutputVariable, inputOperation1.OriginalOutputVariable, 1, false), new BasicInput("", inputOperation2.OriginalOutputVariable, inputOperation2.OriginalOutputVariable, 1, false) }, "Lagkage", "");
            dfg.AddNode(inputOperation1);
            dfg.AddNode(inputOperation2);
            dfg.AddNode(mixingOperation);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation1, inputOperation2 }, board, library);
            schedule.ListScheduling(assay, board, library);

            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(mixingOperation, schedule.ScheduledOperations[0]);
            Assert.AreEqual(0, mixingOperation.StartTime);
            Assert.IsTrue(mixingOperation.StartTime + mixingOperation.BoundModule.OperationTime <= mixingOperation.EndTime);
            //Two droplets needs to be routed: the routes should be shorter than 10:
            Assert.IsTrue(mixingOperation.EndTime <= mixingOperation.StartTime + mixingOperation.BoundModule.OperationTime + 10*2);

            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();
            Assert.AreEqual(2, boardAtDifferentTimes[0].PlacedModules.Values.Count);
            Assert.AreEqual(3, boardAtDifferentTimes[1].PlacedModules.Values.Count);
            Assert.AreEqual(4, boardAtDifferentTimes[2].PlacedModules.Values.Count);
            Assert.AreEqual(mixingOperation.BoundModule, boardAtDifferentTimes[1].PlacedModules.Values.ToList()[2]);

            //The routed droplets should be placed at the input/output locations of the mixer
            Droplet droplet1 = (Droplet) boardAtDifferentTimes[2].PlacedModules.Values.ToList()[2];
            Droplet droplet2 = (Droplet) boardAtDifferentTimes[2].PlacedModules.Values.ToList()[3];
            Route route1 = mixingOperation.InputRoutes[op1Name][0];
            Route route2 = mixingOperation.InputRoutes[op2Name][0];
            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(route1, board, (InputModule)inputOperation1.BoundModule, droplet1));
            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(route2, board, (InputModule)inputOperation2.BoundModule, droplet2));

        }

        [TestMethod]
        public void TestFluidTransferFromDroplet()
        {
            DFG<Block> dfg = new DFG<Block>();
            int initialNumberOfDroplets = 10;
            int numberOfDropletsTransfered = 5;
            int numberOfDropletsRenamed = 3;
            string op1Name = "op1";
            string op2Name = "op2";
            InputDeclaration inputOperation = new InputDeclaration("kage", "Test", initialNumberOfDroplets, "");
            //First extracting the droplets:
            Fluid fluidTransfer1 = new Fluid(new List<FluidInput>() { new BasicInput("", inputOperation.OriginalOutputVariable, inputOperation.OriginalOutputVariable, numberOfDropletsTransfered, false) }, op1Name, "");
            //Testing (renaming) droplets:
            Fluid fluidTransfer2 = new Fluid(new List<FluidInput>() { new BasicInput("", fluidTransfer1.OriginalOutputVariable, fluidTransfer1.OriginalOutputVariable, numberOfDropletsRenamed, false) }, op2Name, "");
            dfg.AddNode(inputOperation);
            dfg.AddNode(fluidTransfer1);
            dfg.AddNode(fluidTransfer2);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation }, board, library);
            schedule.ListScheduling(assay, board, library);

            Assert.AreEqual(2, schedule.ScheduledOperations.Count);
            Assert.AreEqual(fluidTransfer1, schedule.ScheduledOperations[0]);
            Assert.AreEqual(fluidTransfer2, schedule.ScheduledOperations[1]);
            Assert.AreEqual(0, fluidTransfer1.StartTime);
            Assert.IsTrue(fluidTransfer1.EndTime <= numberOfDropletsTransfered * 20); //Tranfering one droplet should one average take less than 10 time units.
            Assert.AreEqual(initialNumberOfDroplets - numberOfDropletsTransfered, ((InputModule)inputOperation.BoundModule).DropletCount);
            //Renaming them should be really fast (though their is a delay associated with the operation):
            Assert.AreEqual(fluidTransfer1.EndTime + 1, fluidTransfer2.StartTime);
            Assert.IsTrue  (fluidTransfer2.EndTime <= fluidTransfer2.StartTime + 5);

            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();
            Assert.AreEqual(1, boardAtDifferentTimes[0].PlacedModules.Values.Count);
            Assert.AreEqual(1 + numberOfDropletsTransfered, boardAtDifferentTimes[1].PlacedModules.Values.Count);
            Assert.AreEqual(1 + numberOfDropletsTransfered, boardAtDifferentTimes[2].PlacedModules.Values.Count);
            //The first couple of droplets should have been renamed:
            for (int i = 0; i < numberOfDropletsTransfered; i++)
            {
                Droplet droplet = (Droplet)boardAtDifferentTimes[1].PlacedModules.Values.ToList()[i + 1];
                if (i < numberOfDropletsRenamed) {
                    Assert.AreEqual(op2Name, droplet.GetFluidType().FluidName);
                    Assert.IsTrue(schedule.FluidVariableLocations[op2Name].dropletSources.Contains(droplet));
                }
                else {
                    Assert.AreEqual(op1Name, droplet.GetFluidType().FluidName);
                    Assert.IsTrue(schedule.FluidVariableLocations[op1Name].dropletSources.Contains(droplet));
                }
                //Since the droplets have simply been renamed, they should still be placed the same place as before:
                Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fluidTransfer1.InputRoutes[inputOperation.OriginalOutputVariable][i], board, (InputModule)inputOperation.BoundModule, droplet));
            }
            Assert.AreEqual(3, schedule.FluidVariableLocations.Count);
            Assert.AreEqual(numberOfDropletsTransfered - numberOfDropletsRenamed, schedule.FluidVariableLocations[op1Name].GetNumberOfDropletsAvailable());
            Assert.AreEqual(numberOfDropletsRenamed, schedule.FluidVariableLocations[op2Name].GetNumberOfDropletsAvailable());
        }


        [TestMethod]
        public void TestFluidTransferFromInput()
        {
            DFG<Block> dfg = new DFG<Block>();
            int initialNumberOfDroplets = 10;
            int numberOfDropletsTransfered = 5;
            InputDeclaration inputOperation = new InputDeclaration("kage", "Test", initialNumberOfDroplets, "");
            //Testing extracting from an input:
            Fluid fluidTransfer1 = new Fluid(new List<FluidInput>() { new BasicInput("", inputOperation.OriginalOutputVariable, inputOperation.OriginalOutputVariable, numberOfDropletsTransfered, false) }, "op1", "");
            dfg.AddNode(inputOperation);
            dfg.AddNode(fluidTransfer1);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation}, board, library);
            schedule.ListScheduling(assay, board, library);

            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(fluidTransfer1, schedule.ScheduledOperations[0]);
            Assert.AreEqual(0, fluidTransfer1.StartTime);
            Assert.IsTrue(fluidTransfer1.EndTime <= numberOfDropletsTransfered * 20); //Tranfering one droplet should one average take less than 20 time units.
            Assert.AreEqual(initialNumberOfDroplets - numberOfDropletsTransfered, ((InputModule)inputOperation.BoundModule).DropletCount);

            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();
            Assert.AreEqual(1, boardAtDifferentTimes[0].PlacedModules.Values.Count);
            Assert.AreEqual(1 + numberOfDropletsTransfered, boardAtDifferentTimes[1].PlacedModules.Values.Count);
            for (int i = 0; i < numberOfDropletsTransfered; i++)
            {
                Droplet droplet = (Droplet)boardAtDifferentTimes[1].PlacedModules.Values.ToList()[i + 1];
                Assert.IsTrue(droplet != null);
                Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fluidTransfer1.InputRoutes[inputOperation.OriginalOutputVariable][i], board, (InputModule)inputOperation.BoundModule, droplet));
            }
        }


        [TestMethod]
        public void TestUnionFromInputs()
        {
            DFG<Block> dfg = new DFG<Block>();
            int initialNumberOfDroplets = 3;
            int numberOfDropletsTransfered1 = 2;
            int numberOfDropletsTransfered2 = 3;
            InputDeclaration inputOperation1 = new InputDeclaration("kage1", "Test1", initialNumberOfDroplets, "");
            InputDeclaration inputOperation2 = new InputDeclaration("kage2", "Test2", initialNumberOfDroplets, "");
            //Testing extracting from an input:
            Union union = new Union(new List<FluidInput>() { new BasicInput("", inputOperation1.OriginalOutputVariable, inputOperation1.OriginalOutputVariable, numberOfDropletsTransfered1, false), new BasicInput("", inputOperation2.OriginalOutputVariable, inputOperation2.OriginalOutputVariable, numberOfDropletsTransfered2, false) }, "op1", "");
            dfg.AddNode(inputOperation1);
            dfg.AddNode(inputOperation2);
            dfg.AddNode(union);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(15, 15);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation1, inputOperation2 }, board, library);
            schedule.ListScheduling(assay, board, library);

            Assert.AreEqual(1, schedule.ScheduledOperations.Count);
            Assert.AreEqual(union, schedule.ScheduledOperations[0]);
            Assert.AreEqual(0, union.StartTime);
            /*
            Assert.IsTrue(fluidTransfer1.EndTime <= numberOfDropletsTransfered * 10); //Tranfering one droplet should one average take less than 10 time units.
            Assert.AreEqual(initialNumberOfDroplets - numberOfDropletsTransfered, ((InputModule)inputOperation.BoundModule).DropletCount);

            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();
            Assert.AreEqual(1, boardAtDifferentTimes[0].PlacedModules.Values.Count);
            Assert.AreEqual(1 + numberOfDropletsTransfered, boardAtDifferentTimes[1].PlacedModules.Values.Count);
            for (int i = 0; i < numberOfDropletsTransfered; i++)
            {
                Droplet droplet = (Droplet)boardAtDifferentTimes[1].PlacedModules.Values.ToList()[i + 1];
                Assert.IsTrue(droplet != null);
                Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fluidTransfer1.InputRoutes[inputOperation.OriginalOutputVariable][i], board, (InputModule)inputOperation.BoundModule, droplet));
            }
            */
        }

        [TestMethod]
        public void TestHeater2Droplets()
        {
            DFG<Block> dfg = new DFG<Block>();
            int numberOfDroplets = 1;
            int time1 = 100;
            int time2 = 100;
            string heaterModuleName = "heaterModule";
            StaticDeclarationBlock inputOperation = new InputDeclaration("kage", "Test", 10, "");
            HeaterDeclaration heaterDeclaration = new HeaterDeclaration(heaterModuleName, "");
            HeaterUsage heaterOperation1 = new HeaterUsage(heaterModuleName, new List<FluidInput>() { new BasicInput("", inputOperation.OutputVariable, inputOperation.OriginalOutputVariable, numberOfDroplets, false)}, "Fisk", 500, time1, "");
            HeaterUsage heaterOperation2 = new HeaterUsage(heaterModuleName, new List<FluidInput>() { new BasicInput("", heaterOperation1.OutputVariable, heaterOperation1.OriginalOutputVariable, numberOfDroplets, false) }, "Derp", 500, time2, "");
            dfg.AddNode(inputOperation);
            dfg.AddNode(heaterDeclaration);
            dfg.AddNode(heaterOperation1);
            dfg.AddNode(heaterOperation2);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation, heaterDeclaration }, board, library);
            schedule.ListScheduling(assay, board, library);


            //Only two non-static-declaration operation.
            Assert.AreEqual(2, schedule.ScheduledOperations.Count);
            Assert.AreEqual(heaterOperation1, schedule.ScheduledOperations[0]);
            Assert.AreEqual(heaterOperation2, schedule.ScheduledOperations[1]);
            Assert.IsTrue(heaterOperation1.Time <= heaterOperation1.EndTime);
            Assert.IsTrue(heaterOperation1.EndTime <= heaterOperation1.Time + 20);
            Assert.IsTrue(heaterOperation1.EndTime + 1 == heaterOperation2.StartTime);
            Assert.IsTrue(heaterOperation2.StartTime + heaterOperation2.Time <= heaterOperation2.EndTime);
            Assert.IsTrue(heaterOperation2.EndTime <= heaterOperation2.StartTime + heaterOperation2.Time + 20);
            //It should first of all place both the heater and the input. 
            //Moreover, it should then route to the heater, and then out of the heater, two times:
            List<Board> boardAtDifferentTimes = schedule.boardAtDifferentTimes.Select(pair => pair.Value).ToList();
            Assert.AreEqual(2, boardAtDifferentTimes[0].PlacedModules.Values.Count);
            for (int i = 1; i < boardAtDifferentTimes.Count; i++)
            {
                int isDropletOnTheBoard = (i % 2 == 0)? 1: 0;
                Assert.AreEqual(2 + isDropletOnTheBoard, boardAtDifferentTimes[i].PlacedModules.Values.Count, "Failure at i=" + i);
            }
            Route fromInputToHeater     = heaterOperation1.InputRoutes[inputOperation.OriginalOutputVariable][0];
            Route fromHeaterToDroplet   = heaterOperation1.OutputRoutes[heaterOperation1.OriginalOutputVariable][0];
            Route fromDropletToHeater2  = heaterOperation2.InputRoutes[heaterOperation1.OriginalOutputVariable][0];
            Route fromHeater2ToDroplet  = heaterOperation2.OutputRoutes[heaterOperation2.OriginalOutputVariable][0];
            Droplet droplet = (Droplet) boardAtDifferentTimes.Last().PlacedModules.Values.Last();

            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fromInputToHeater   , board, (InputModule)inputOperation.BoundModule, heaterOperation1.BoundModule.GetInputLayout().Droplets[0]));
            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fromHeaterToDroplet , board, (Droplet) heaterOperation1.BoundModule.GetInputLayout().Droplets[0],droplet));
            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fromDropletToHeater2, board, droplet, heaterOperation2.BoundModule.GetInputLayout().Droplets[0]));
            Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(fromHeater2ToDroplet, board, heaterOperation2.BoundModule.GetInputLayout().Droplets[0], droplet));
            
        }
    }
}
