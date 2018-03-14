using System;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BiolyTests {

    [TestClass]
    public class TestModuleLibrary {
        
        Block testSensorBlock1 = new Sensor (null, null, null);
        Block testSensorBlock2 = new Sensor (null, null, null);
        Block testMixerBlock1 = new Sensor (4, 4, 2000);
        Block testMixerBlock2 = new Sensor (4, 4, 2000);

        [TestMethod]
        public void TestAllocateModulesSimpleAssayOneBlock () {
            Assay assay = new Assay ();
            assay.AddNode (testSensorBlock1);
            assay.
            ModuleLibrary library = new ModuleLibrary ();
            assay.allocateModules (assay);
            Assert.AreEqual(library.allocateModules.Count, 1);
        }

        [TestMethod]
        public void TestAllocateModulesSimpleAssayMultiBlockUnconencted () {
            Assay assay = new Assay ();
            assay.AddNode (testSensorBlock1);
            assay.AddNode (testMixerBlock2);
            ModuleLibrary library = new ModuleLibrary ();
            assay.allocateModules (assay);
            Assert.AreEqual(library.allocateModules.Count, 2);
        }

        [TestMethod]
        public void TestAllocateModulesSimpleAssayMultiBlockOverlap () {
            Assay assay = new Assay ();
            assay.AddNode (testSensorBlock1);
            assay.AddNode (testSensorBlock2);
            assay.AddNode (testMixerBlock1);
            assay.AddNode (testMixerBlock2);
            ModuleLibrary library = new ModuleLibrary ();
            assay.allocateModules (assay);
            Assert.AreEqual(library.allocateModules.Count, 2);
        }

    }
}