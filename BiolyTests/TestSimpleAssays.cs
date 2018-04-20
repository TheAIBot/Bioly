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
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyTests.TestObjects;
using BiolyCompiler.BlocklyParts.Misc;
using System.IO;
using BiolyCompiler.Parser;
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
            FluidBlock inputOperation = new InputDeclaration("kage", "Test", 10);
            FluidBlock outputOperation = new OutputUseage("også_kage",new List<FluidInput> { new FluidInput(inputOperation.OutputVariable, numberOfInputs, false) }, "Kage", null);
            dfg.AddNode(inputOperation);
            dfg.AddNode(outputOperation);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            schedule.ListScheduling(assay, board, new ModuleLibrary());
            Assert.AreEqual(1, outputOperation.boundModule.InputRoutes.Count);
            Assert.AreEqual(5, outputOperation.boundModule.InputRoutes[inputOperation.OutputVariable].Count);
            int startTime = 0;
            for (int i = 0; i < numberOfInputs; i++)
            {
                Route route = outputOperation.boundModule.InputRoutes[inputOperation.OutputVariable][i];
                Assert.AreEqual(startTime, route.startTime);
                Assert.AreEqual(startTime + 3, route.getEndTime());
                Assert.IsTrue(RoutingTests.TestRouting.isAnActualRoute(route, board));
                Assert.IsTrue(RoutingTests.TestRouting.hasCorrectStartAndEnding(route, board, inputOperation.boundModule.GetInputLayout().Droplets[0], outputOperation.boundModule.GetInputLayout().Droplets[0]));
                startTime += 4;
            }
        }

        [TestMethod]
        public void testSequentialMixer()
        {
            //C:\Users\Lombre\Bioly\BiolyTests\BiolyPrograms
            String xmlAssayCode = File.ReadAllText("../../../BiolyPrograms/SequentialMixing.bc" + ".txt");
            CDFG graph = XmlParser.Parse(xmlAssayCode);
            DFG<Block> runningGraph = graph.StartDFG;
            Assay assay = new Assay(runningGraph);
            Board board = new Board(15, 15);
            ModuleLibrary library = new ModuleLibrary();
            Schedule schedule = new Schedule();
            schedule.ListScheduling(assay, board, library);
            Assert.Fail("Hasn't been implemented yet.");
        }
    }
}
