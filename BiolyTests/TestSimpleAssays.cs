using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
using BiolyCompiler.Routing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyTests.TestObjects;
using BiolyCompiler.BlocklyParts.Misc;
using System.IO;
using BiolyCompiler.Parser;
using BiolyCompiler.Exceptions.ParserExceptions;
//using MoreLinq;

namespace BiolyTests.SimpleAssayTests
{
    [TestClass]
    public class TestSimpleAssays
    {
        [TestMethod]
        public void testSimpleInputOutput()
        {
            DFG<Block> dfg = new DFG<Block>();
            int numberOfInputs = 5;
            StaticDeclarationBlock inputOperation = new InputDeclaration("kage", "Test", 10, "");
            StaticDeclarationBlock outputDeclaration = new OutputDeclaration("også_kage", "meh", null, "");
            FluidBlock outputOperation = new OutputUseage("også_kage", new List<FluidInput> { new FluidInput(inputOperation.OutputVariable, numberOfInputs, false) }, "Kage", null, "");
            dfg.AddNode(inputOperation);
            dfg.AddNode(outputDeclaration);
            dfg.AddNode(outputOperation);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation, outputDeclaration }, board,library);
            schedule.ListScheduling(assay, board, library);
            Assert.AreEqual(0, inputOperation.InputRoutes.Count);
            Assert.AreEqual(5, outputOperation.InputRoutes[inputOperation.OutputVariable].Count);
            int startTime = 0;
            for (int i = 0; i < numberOfInputs; i++)
            {
                Route route = outputOperation.InputRoutes[inputOperation.OutputVariable][i];
                Assert.AreEqual(startTime, route.startTime);
                Assert.AreEqual(startTime + 3, route.getEndTime());
                Assert.IsTrue(RoutingTests.TestRouting.isAnActualRoute(route, board));
                Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(route, board, inputOperation.BoundModule.GetInputLayout().Droplets[0], outputOperation.BoundModule.GetInputLayout().Droplets[0]));
                startTime += 4;
            }
        }

        [TestMethod]
        public void testSimpleFluidTransfer()
        {
            Schedule schedule = runSelectedProgram("SimpleFluidTransfer");
            Board initialBoard = schedule.boardAtDifferentTimes[0];
        }

        [TestMethod]
        public void testSequentialMixer()
        {
            Schedule schedule = runSelectedProgram("SequentialMixing");
            Board initialBoard = schedule.boardAtDifferentTimes[0];
            Assert.AreEqual(3, initialBoard.placedModules.Count);
            Assert.AreEqual(2, initialBoard.placedModules.Where(module => module is InputModule).ToList().Count);
            Assert.AreEqual(1, initialBoard.placedModules.Where(module => module is OutputModule).ToList().Count);
            Mixer mixOperation1 = (Mixer) schedule.ScheduledOperations[0];
            Mixer mixOperation2 = (Mixer) schedule.ScheduledOperations[1];
            OutputUseage outputOpereration = (OutputUseage) schedule.ScheduledOperations[2];

            Assert.IsTrue(mixOperation1.StartTime == 0);
            Assert.IsTrue(mixOperation1.BoundModule.OperationTime + mixOperation1.StartTime + 10*Schedule.DROP_MOVEMENT_TIME <= mixOperation1.endTime);
            Assert.IsTrue(mixOperation1.endTime   <= mixOperation1.StartTime + mixOperation1.BoundModule.OperationTime + 30 * Schedule.DROP_MOVEMENT_TIME);

            Assert.IsTrue(mixOperation1.endTime + 1 == mixOperation2.StartTime);
            Assert.IsTrue(mixOperation2.BoundModule.OperationTime + mixOperation2.StartTime + 10 * Schedule.DROP_MOVEMENT_TIME <= mixOperation2.endTime);
            Assert.IsTrue(mixOperation2.endTime <= mixOperation2.StartTime + mixOperation2.BoundModule.OperationTime + 30 * Schedule.DROP_MOVEMENT_TIME);

            Assert.IsTrue(mixOperation2.endTime + 1 == outputOpereration.StartTime);
            Assert.IsTrue(outputOpereration.BoundModule.OperationTime + outputOpereration.StartTime + 10 * Schedule.DROP_MOVEMENT_TIME <= outputOpereration.endTime);
            Assert.IsTrue(outputOpereration.endTime <= outputOpereration.StartTime + outputOpereration.BoundModule.OperationTime + 30 * Schedule.DROP_MOVEMENT_TIME);
        }


        public Schedule runSelectedProgram(String programName)
        {
            //C:\Users\Lombre\Bioly\BiolyTests\BiolyPrograms
            String xmlAssayCode = File.ReadAllText("../../../BiolyPrograms/" + programName + ".bc.txt");
            (CDFG graph, List<ParseException> exceptions) = XmlParser.Parse(xmlAssayCode);
            DFG<Block> runningGraph = graph.StartDFG;
            Assay assay = new Assay(runningGraph);
            Board board = new Board(15, 15);
            ModuleLibrary library = new ModuleLibrary();
            Schedule schedule = new Schedule();
            List<StaticDeclarationBlock> staticDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                                          .Select(node => node.value as StaticDeclarationBlock)
                                                                          .ToList();
            schedule.PlaceStaticModules(staticDeclarations, board, library);
            schedule.ListScheduling(assay, board, library);
            return schedule;
        }
    }
}
