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
            Assay assay = new Assay(TestAssay.GetTotallyParallelDFG());
            

        }

    }
}
