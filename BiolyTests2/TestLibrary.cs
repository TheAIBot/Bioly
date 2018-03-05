using System;
using System.Collections.Generic;
using MoreLinq;
using BiolyTests.AssayTests;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.BlocklyParts.Blocks.Sensors;
using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyTests.ModuleLibraryTests
{
    [TestClass]
    public class TestLibrary
    {
        [TestMethod]
        public void TestAllocateModules()
        {
            ModuleLibrary library = new ModuleLibrary();
            Assay assay = new Assay(TestAssay.GetSemiParallelDFG());
            library.allocateModules(assay);
            List<OperationType> usedOperationTypes = assay.dfg.nodes.DistinctBy(node => node.value.getOperationType())
                                                                    .Select(node => node.value.getOperationType())
                                                                    .ToList();
            
            List<OperationType> allocatedOperationTypes = library.allocatedModules.DistinctBy(module => module.getOperationType())
                                                                                  .Select(module => module.getOperationType())
                                                                                  .ToList();
            Assert.AreEqual(usedOperationTypes.Count, allocatedOperationTypes.Count);
            foreach(var operationType in usedOperationTypes)
            {
                Assert.IsTrue(allocatedOperationTypes.Contains(operationType));
            }
        }
        
        [TestMethod]
        public void TestGetFirstPlaceableModule()
        {
            Assert.Fail("Has not been implemented yet.");
        }

        [TestMethod]
        public void TestGetOptimalModule()
        {
            Assert.Fail("Has not been implemented yet.");
        }

        [TestMethod]
        public void TestGetAndPlaceFirstPlaceableModule()
        {
            Assert.Fail("Has not been implemented yet.");
        }
        


    }
}
