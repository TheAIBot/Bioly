using System.Collections.Generic;
using MoreLinq;
using BiolyTests.AssayTests;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using BiolyCompiler.BlocklyParts;

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

            Assert.Fail();
            //List<OperationType> usedOperationTypes = assay.dfg.Nodes.DistinctBy(node => (node.value as FluidBlock).getOperationType())
            //                                                        .Select(node => (node.value as FluidBlock).getOperationType())
            //                                                        .ToList();
            
            //List<OperationType> allocatedOperationTypes = library.allocatedModules.DistinctBy(module => module.getOperationType())
            //                                                                      .Select(module => module.getOperationType())
            //                                                                      .ToList();
            //Assert.AreEqual(usedOperationTypes.Count, allocatedOperationTypes.Count);
            //foreach(var operationType in usedOperationTypes)
            //{
            //    Assert.IsTrue(allocatedOperationTypes.Contains(operationType));
            //}
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
