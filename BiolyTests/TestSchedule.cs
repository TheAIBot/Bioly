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
        [TestInitialize]
        public void ClearWorkspace() => TestTools.ClearWorkspace();

        [TestMethod]
        public void TestScheduleOneSeqMixer()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List <Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(1, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Mixer);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 2);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqMixers()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("d", "c", 1, false, "b", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Mixer);
            Assert.IsTrue(scheduledBlocks[1] is Mixer);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 2);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqMixersOverrideName()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("c", "c", 1, false, "b", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Mixer);
            Assert.IsTrue(scheduledBlocks[1] is Mixer);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 2);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoParMixers()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("d", "a", 1, false, "b", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Mixer);
            Assert.IsTrue(scheduledBlocks[1] is Mixer);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 2);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 2);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleOneSeqHeater()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(1, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is HeaterUsage);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqHeaters()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("d", "b", 10, 27, "c", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is HeaterUsage);
            Assert.IsTrue(scheduledBlocks[1] is HeaterUsage);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 0);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqHeatersOverrideName()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("c", "b", 10, 27, "c", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is HeaterUsage);
            Assert.IsTrue(scheduledBlocks[1] is HeaterUsage);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoParHeaters()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("d", "b", 10, 27, "a", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is HeaterUsage);
            Assert.IsTrue(scheduledBlocks[1] is HeaterUsage);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleOneSeqRenamer()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(1, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Fluid);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 6);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 4);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleOneSeqRenamersOverrideName()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "b", 1, true);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(1, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Fluid);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 10);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 0);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqRenamers()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.AddFluidSegment("c", "b", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Fluid);
            Assert.IsTrue(scheduledBlocks[1] is Fluid);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 6);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 3);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoParRenamers()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.AddFluidSegment("c", "a", 1, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Fluid);
            Assert.IsTrue(scheduledBlocks[1] is Fluid);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 5);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 4);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 1);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleOneSeqUnion()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(1, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Union);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 9);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 7);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 4);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqUnions()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("d", "a", 1, false, "c", 2, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Union);
            Assert.IsTrue(scheduledBlocks[1] is Union);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 7);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 2);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 3);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoSeqUnionsOverrideName()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("c", "a", 1, false, "c", 2, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Union);
            Assert.IsTrue(scheduledBlocks[1] is Union);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 7);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 3);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        [TestMethod]
        public void TestScheduleTwoParUnions()
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("d", "a", 1, false, "b", 2, false);
            program.Finish();

            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            Assert.AreEqual(2, scheduledBlocks.Count);
            Assert.IsTrue(scheduledBlocks[0] is Union);
            Assert.IsTrue(scheduledBlocks[1] is Union);

            CheckFluidInfo(scheduler.FluidVariableLocations, "a", 8);
            CheckFluidInfo(scheduler.FluidVariableLocations, "b", 5);
            CheckFluidInfo(scheduler.FluidVariableLocations, "c", 4);
            CheckFluidInfo(scheduler.FluidVariableLocations, "d", 3);

            VerifyDFGTimings(cdfg.StartDFG, data.time);
        }

        private static (Schedule scheduler, int time) ScheduleDFG(DFG<Block> dfg)
        {
            Assay assay = new Assay(dfg);
            Board board = new Board(20, 20);
            ModuleLibrary library = new ModuleLibrary();

            Schedule schedule = new Schedule();
            schedule.PlaceStaticModules(dfg.Nodes.Select(x => x.value).OfType<StaticDeclarationBlock>().ToList(), board, library);
            int time = schedule.ListScheduling(assay, board, library);
            return (schedule, time);
        }

        private static void CheckFluidInfo(Dictionary<string, BoardFluid> fluids, string fluidName, int expectedDroplets)
        {
            Assert.IsTrue(fluids.ContainsKey(fluidName));
            Assert.AreEqual(fluids[fluidName].GetNumberOfDropletsAvailable(), expectedDroplets);
        }

        private static void VerifyDFGTimings(DFG<Block> dfg, int completionTime)
        {
            List<Node<Block>> rank = new List<Node<Block>>();
            rank.AddRange(dfg.Input.Where(x => x.value is FluidBlock));

            do
            {
                foreach (Node<Block> node in rank)
                {
                    if (node.value is StaticDeclarationBlock)
                    {
                        continue;
                    }

                    Assert.IsTrue(node.value.StartTime <= node.value.EndTime);
                    Assert.IsTrue(node.value.EndTime <= completionTime);

                    foreach (Node<Block> dependableNode in node.getOutgoingEdges())
                    {
                        if (!(dependableNode.value is FluidBlock))
                        {
                            continue;
                        }

                        Assert.IsTrue(node.value.EndTime <= dependableNode.value.StartTime);
                    }
                }

                List<Node<Block>> newRank = rank.SelectMany(x => x.getOutgoingEdges())
                                                .Where(x => x.value is FluidBlock)
                                                .ToList();
                rank = newRank;
            } while (rank.Count > 0);
        }
    }
}
