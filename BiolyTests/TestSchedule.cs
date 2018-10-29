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
using BiolyCompiler;

namespace BiolyTests.ScheduleTests
{
    [TestClass]
    public class TestSchedule
    {
        private const int BOARD_WIDTH = 20;
        private const int BOARD_HEIGHT = 20;

        [TestInitialize]
        public void ResetWorkspace() => TestTools.ResetBrowser();

        [TestMethod]
        public void TestScheduleOneSeqMixer()
        {
            ScheduleOneSeqMixer(false, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqMixerWithGC()
        {
            ScheduleOneSeqMixer(true, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqMixerWithOptimizations()
        {
            ScheduleOneSeqMixer(false, true);
        }
        [TestMethod]
        public void TestScheduleOneSeqMixerWithGCAndOptimizations()
        {
            ScheduleOneSeqMixer(true, true);
        }
        public void ScheduleOneSeqMixer(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("b", 9),
                ("c", 2),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Mixer),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqMixers()
        {
            ScheduleTwoSeqMixers(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersWithGC()
        {
            ScheduleTwoSeqMixers(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersWithOptimizations()
        {
            ScheduleTwoSeqMixers(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersWithGCAndOptimizations()
        {
            ScheduleTwoSeqMixers(true, true);
        }
        public void ScheduleTwoSeqMixers(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("d", "c", 1, false, "b", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("b", 8),
                ("c", 1),
                ("d", 2),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Mixer),
                typeof(Mixer),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqMixersOverrideName()
        {
            ScheduleTwoSeqMixersOverrideName(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersOverrideNameWithGC()
        {
            ScheduleTwoSeqMixersOverrideName(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersOverrideNameWithOptimizations()
        {
            ScheduleTwoSeqMixersOverrideName(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqMixersOverrideNameWithGCAndOptimizations()
        {
            ScheduleTwoSeqMixersOverrideName(true, true);
        }
        public void ScheduleTwoSeqMixersOverrideName(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("c", "c", 1, false, "b", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("b", 8),
                ("c", 2),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Mixer),
                typeof(Mixer),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoParMixers()
        {
            ScheduleTwoParMixers(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoParMixersWithGC()
        {
            ScheduleTwoParMixers(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoParMixersWithOptimizations()
        {
            ScheduleTwoParMixers(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoParMixersWithGCAndOptimizations()
        {
            ScheduleTwoParMixers(true, true);
        }
        public void ScheduleTwoParMixers(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddMixerSegment("c", "a", 1, false, "b", 1, false);
            program.AddMixerSegment("d", "a", 1, false, "b", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 8),
                ("b", 8),
                ("c", 2),
                ("d", 2),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Mixer),
                typeof(Mixer),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleOneSeqHeater()
        {
            ScheduleOneSeqHeater(false, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqHeaterWithGC()
        {
            ScheduleOneSeqHeater(true, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqHeaterWithOptimizations()
        {
            ScheduleOneSeqHeater(false, true);
        }
        [TestMethod]
        public void TestScheduleOneSeqHeaterWithGCAndOptimizations()
        {
            ScheduleOneSeqHeater(true, true);
        }
        public void ScheduleOneSeqHeater(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("c", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(HeaterUsage),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqHeaters()
        {
            ScheduleTwoSeqHeaters(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersWithGC()
        {
            ScheduleTwoSeqHeaters(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersWithOptimizations()
        {
            ScheduleTwoSeqHeaters(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersWithGCAndOptimizations()
        {
            ScheduleTwoSeqHeaters(true, true);
        }
        public void ScheduleTwoSeqHeaters(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("d", "b", 10, 27, "c", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("c", 0),
                ("d", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(HeaterUsage),
                typeof(HeaterUsage),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqHeatersOverrideName()
        {
            ScheduleTwoSeqHeatersOverrideName(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersOverrideNameWithGC()
        {
            ScheduleTwoSeqHeatersOverrideName(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersOverrideNameWithOptimizations()
        {
            ScheduleTwoSeqHeatersOverrideName(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqHeatersOverrideNameWithGCAndOptimizations()
        {
            ScheduleTwoSeqHeatersOverrideName(true, true);
        }
        public void ScheduleTwoSeqHeatersOverrideName(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("c", "b", 10, 27, "c", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("c", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(HeaterUsage),
                typeof(HeaterUsage),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoParHeaters()
        {
            ScheduleTwoParHeaters(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoParHeatersWithGC()
        {
            ScheduleTwoParHeaters(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoParHeatersWithOptimizations()
        {
            ScheduleTwoParHeaters(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoParHeatersWithGCAndOptimizations()
        {
            ScheduleTwoParHeaters(true, true);
        }
        public void ScheduleTwoParHeaters(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddHeaterDeclarationBlock("b");
            program.AddHeaterSegment("c", "b", 10, 27, "a", 1, false);
            program.AddHeaterSegment("d", "b", 10, 27, "a", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 8),
                ("c", 1),
                ("d", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(HeaterUsage),
                typeof(HeaterUsage),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleOneSeqRenamer()
        {
            ScheduleOneSeqRenamer(false, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamerWithGC()
        {
            ScheduleOneSeqRenamer(true, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamerWithOptimizations()
        {
            ScheduleOneSeqRenamer(false, true);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamerWithGCAndOptimizations()
        {
            ScheduleOneSeqRenamer(true, true);
        }
        public void ScheduleOneSeqRenamer(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 6),
                ("b", 4),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Fluid),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleOneSeqRenamersOverrideName()
        {
            ScheduleOneSeqRenamersOverrideName(false, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamersOverrideNameWithGC()
        {
            ScheduleOneSeqRenamersOverrideName(true, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamersOverrideNameWithOptimizations()
        {
            ScheduleOneSeqRenamersOverrideName(false, true);
        }
        [TestMethod]
        public void TestScheduleOneSeqRenamersOverrideNameWithGCAndOptimizations()
        {
            ScheduleOneSeqRenamersOverrideName(true, true);
        }
        public void ScheduleOneSeqRenamersOverrideName(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "b", 1, true);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 10),
                ("b", 0),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Fluid),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqRenamers()
        {
            ScheduleTwoSeqRenamers(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqRenamersWithGC()
        {
            ScheduleTwoSeqRenamers(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqRenamersWithOptimizations()
        {
            ScheduleTwoSeqRenamers(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqRenamersWithGCAndOptimizations()
        {
            ScheduleTwoSeqRenamers(true, true);
        }
        public void ScheduleTwoSeqRenamers(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.AddFluidSegment("c", "b", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 6),
                ("b", 3),
                ("c", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Fluid),
                typeof(Fluid),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoParRenamers()
        {
            ScheduleTwoParRenamers(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoParRenamersWithGC()
        {
            ScheduleTwoParRenamers(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoParRenamersWithOptimizations()
        {
            ScheduleTwoParRenamers(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoParRenamersWithGCAndOptimizations()
        {
            ScheduleTwoParRenamers(true, true);
        }
        public void ScheduleTwoParRenamers(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddFluidSegment("b", "a", 4, false);
            program.AddFluidSegment("c", "a", 1, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 5),
                ("b", 4),
                ("c", 1),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Fluid),
                typeof(Fluid),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleOneSeqUnion()
        {
            ScheduleOneSeqUnion(false, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqUnionWithGC()
        {
            ScheduleOneSeqUnion(true, false);
        }
        [TestMethod]
        public void TestScheduleOneSeqUnionWithOptimizations()
        {
            ScheduleOneSeqUnion(false, true);
        }
        [TestMethod]
        public void TestScheduleOneSeqUnionWithGCAndOptimizations()
        {
            ScheduleOneSeqUnion(true, true);
        }
        public void ScheduleOneSeqUnion(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 9),
                ("b", 7),
                ("c", 4),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Union),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqUnions()
        {
            ScheduleTwoSeqUnions(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsWithGC()
        {
            ScheduleTwoSeqUnions(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsWithOptimizations()
        {
            ScheduleTwoSeqUnions(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsWithGCAndOptimizations()
        {
            ScheduleTwoSeqUnions(true, true);
        }
        public void ScheduleTwoSeqUnions(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("d", "a", 1, false, "c", 2, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 8),
                ("b", 7),
                ("c", 2),
                ("d", 3),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Union),
                typeof(Union),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoSeqUnionsOverrideName()
        {
            ScheduleTwoSeqUnionsOverrideName(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsOverrideNameWithGC()
        {
            ScheduleTwoSeqUnionsOverrideName(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsOverrideNameWithOptimizations()
        {
            ScheduleTwoSeqUnionsOverrideName(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoSeqUnionsOverrideNameWithGCAndOptimizations()
        {
            ScheduleTwoSeqUnionsOverrideName(true, true);
        }
        public void ScheduleTwoSeqUnionsOverrideName(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("c", "a", 1, false, "c", 2, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 8),
                ("b", 7),
                ("c", 3),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Union),
                typeof(Union),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        [TestMethod]
        public void TestScheduleTwoParUnions()
        {
            ScheduleTwoParUnions(false, false);
        }
        [TestMethod]
        public void TestScheduleTwoParUnionsWithGC()
        {
            ScheduleTwoParUnions(true, false);
        }
        [TestMethod]
        public void TestScheduleTwoParUnionsWithOptimizations()
        {
            ScheduleTwoParUnions(false, true);
        }
        [TestMethod]
        public void TestScheduleTwoParUnionsWithGCAndOptimizations()
        {
            ScheduleTwoParUnions(true, true);
        }
        public void ScheduleTwoParUnions(bool enableGC, bool enableOptimizations)
        {
            JSProgram program = new JSProgram();
            program.AddInputBlock("a", 10);
            program.AddInputBlock("b", 10);
            program.AddUnionSegment("c", "a", 1, false, "b", 3, false);
            program.AddUnionSegment("d", "a", 1, false, "b", 2, false);
            program.Finish();

            (string, int)[] leftoverFluids = new (string, int)[]
            {
                ("a", 8),
                ("b", 5),
                ("c", 4),
                ("d", 3),
            };

            Type[] blockTypes = new Type[]
            {
                typeof(Union),
                typeof(Union),
            };

            CheckScheduler(program, leftoverFluids, blockTypes, enableGC, enableOptimizations);
        }

        private static void CheckScheduler(JSProgram program, (string, int)[] leftoverFluid, Type[] blockTypes, bool enableGC, bool enableOptimizations)
        {
            (CDFG cdfg, var exceptions) = TestTools.ParseProgram(program);
            if (enableOptimizations)
            {
                CDFG newCDFG = new CDFG();
                newCDFG.StartDFG = ProgramExecutor<string>.OptimizeCDFG<string>(100, 100, cdfg, new System.Threading.CancellationToken(), enableGC);
                newCDFG.AddNode(null, newCDFG.StartDFG);
                cdfg = newCDFG;
            }
            Assert.AreEqual(0, exceptions.Count);

            var data = ScheduleDFG(cdfg.StartDFG, enableGC);
            Schedule scheduler = data.scheduler;

            List<Block> scheduledBlocks = scheduler.ScheduledOperations;
            if (enableGC)
            {
                scheduledBlocks.RemoveAll(x => x is WasteUsage);
            }

            Assert.AreEqual(blockTypes.Length, scheduledBlocks.Count);
            for (int i = 0; i < blockTypes.Length; i++)
            {
                Assert.AreEqual(scheduledBlocks[i].GetType(), blockTypes[i]);
            }

            Assert.AreEqual(leftoverFluid.Length, scheduler.FluidVariableLocations.Count);
            if (!enableGC && !enableOptimizations)
            {
                for (int i = 0; i < leftoverFluid.Length; i++)
                {
                    CheckFluidInfo(scheduler.FluidVariableLocations, leftoverFluid[i].Item1, leftoverFluid[i].Item2);
                }
            }

            VerifyDFGTimings(cdfg.StartDFG, data.time);

            RectangleTestTools.VerifyBoards(scheduler.rectanglesAtDifferentTimes.Select(x => x.Value).ToList(), BOARD_WIDTH, BOARD_HEIGHT);
        }

        private static (Schedule scheduler, int time) ScheduleDFG(DFG<Block> dfg, bool enableGC)
        {
            Schedule schedule = new Schedule(BOARD_WIDTH, BOARD_HEIGHT);
            schedule.SHOULD_DO_GARBAGE_COLLECTION = enableGC;
            schedule.PlaceStaticModules(dfg.Nodes.Select(x => x.value).OfType<StaticDeclarationBlock>().ToList());
            int time = schedule.ListScheduling<string>(dfg, null);
            return (schedule, time);
        }

        private static void CheckFluidInfo(Dictionary<string, BoardFluid> fluids, string fluidName, int expectedDroplets)
        {
            Assert.IsTrue(fluids.ContainsKey(fluidName));
            Assert.AreEqual(expectedDroplets, fluids[fluidName].GetNumberOfDropletsAvailable());
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

                    foreach (Node<Block> dependableNode in node.GetOutgoingEdges())
                    {
                        if (!(dependableNode.value is FluidBlock))
                        {
                            continue;
                        }

                        Assert.IsTrue(node.value.EndTime <= dependableNode.value.StartTime);
                    }
                }

                List<Node<Block>> newRank = rank.SelectMany(x => x.GetOutgoingEdges())
                                                .Where(x => x.value is FluidBlock)
                                                .ToList();
                rank = newRank;
            } while (rank.Count > 0);
        }
    }
}
