using System;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyTests.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests.ModulesTests
{
    [TestClass]
    public class TestModules
    {
        [TestMethod]
        public void testFluidTransfer()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void TestHeater1Droplet()
        {
            DFG<Block> dfg = new DFG<Block>();
            int numberOfDroplets = 1;
            string heaterModuleName = "heaterModule";
            StaticDeclarationBlock inputOperation = new InputDeclaration("kage", "Test", 10, "");
            HeaterDeclaration heaterDeclaration = new HeaterDeclaration(heaterModuleName, null, "");
            HeaterUseage heaterOperation = new HeaterUseage(heaterModuleName, new List<FluidInput>() { new FluidInput(inputOperation.OriginalOutputVariable, numberOfDroplets, false)}, "Fisk", 500, 60, "");
            dfg.AddNode(inputOperation);
            dfg.AddNode(heaterDeclaration);
            dfg.AddNode(heaterOperation);
            dfg.FinishDFG();
            Assay assay = new Assay(dfg);
            Schedule schedule = new Schedule();
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            schedule.PlaceStaticModules(new List<StaticDeclarationBlock>() { inputOperation, heaterDeclaration }, board, library);
            schedule.ListScheduling(assay, board, library);
            Assert.Fail("Not implemented yet");
        }
    }
}
