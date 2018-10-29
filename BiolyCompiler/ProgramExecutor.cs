using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;
using System.Linq;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.BlocklyParts.Misc;
using System.Threading.Tasks;
using System.Threading;
using BiolyCompiler.Exceptions.ParserExceptions;
using System.Diagnostics;
using BiolyCompiler.Exceptions;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Declarations;
using MoreLinq;

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;
        public int TimeBetweenCommands = 50;
        public bool ShowEmptyRectangles = false;
        public bool EnableOptimizations = true;
        public bool EnableGarbageCollection = true;
        public bool EnableSparseElectrodes = true;
        public readonly CancellationTokenSource KeepRunning = new CancellationTokenSource();
        public DFG<Block> OptimizedDFG = null;

        public ProgramExecutor(CommandExecutor<T> executor)
        {
            this.Executor = executor;
        }

        public void Run(int width, int height, CDFG graph, bool alreadyOptimized)
        {
            if (CanOptimizeCDFG(graph) && EnableOptimizations && !alreadyOptimized)
            {
                CDFG optimizedCDFG = new CDFG();
                optimizedCDFG.StartDFG = OptimizeCDFG<T>(width, height, graph, KeepRunning.Token, EnableGarbageCollection);
                optimizedCDFG.AddNode(null, optimizedCDFG.StartDFG);

                graph = optimizedCDFG;
            }

            DFG<Block> runningGraph = graph.StartDFG;
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Stack<List<string>> scopedVariables = new Stack<List<string>>();
            Rectangle[] oldRectangles = null;
            bool firstRun = true;

            Dictionary<string, List<IDropletSource>> sumOutputtedDropelts = new Dictionary<string, List<IDropletSource>>();

            controlStack.Push(null);
            scopedVariables.Push(new List<string>());

            Schedule scheduler = new Schedule(width, height);
            scheduler.SHOULD_DO_GARBAGE_COLLECTION = EnableGarbageCollection;
            List<StaticDeclarationBlock> staticModuleDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                              .Select(node => node.value as StaticDeclarationBlock)
                                                              .ToList();
            if (staticModuleDeclarations.Count > 0)
            {
                scheduler.PlaceStaticModules(staticModuleDeclarations);
                scopedVariables.Peek().AddRange(scheduler.NewVariablesCreatedInThisScope.Distinct());
            }

            while (runningGraph != null)
            {
                int time = scheduler.ListScheduling(runningGraph, Executor);
                scopedVariables.Peek().AddRange(scheduler.NewVariablesCreatedInThisScope.Distinct().Where(x => !x.Contains("#@#Index")));

                foreach (var item in scheduler.OutputtedDroplets)
                {
                    if (sumOutputtedDropelts.ContainsKey(item.Key))
                    {
                        sumOutputtedDropelts[item.Key].AddRange(item.Value);
                    }
                    else
                    {
                        sumOutputtedDropelts.Add(item.Key, item.Value);
                    }
                }

                List<Command>[] commandTimeline = CreateCommandTimeline(scheduler.ScheduledOperations, time);
                if (firstRun)
                {
                    bool[] usedElectrodes = GetusedElectrodes(width, height, commandTimeline, EnableSparseElectrodes);
                    StartExecutor(graph.StartDFG, scheduler.StaticModules.Select(pair => pair.Value).ToList(), usedElectrodes);
                    firstRun = false;
                }
                SendCommands(commandTimeline, ref oldRectangles, scheduler.rectanglesAtDifferentTimes);

                if (KeepRunning.IsCancellationRequested)
                {
                    return;
                }

                runningGraph.Nodes.ForEach(x => x.value.Reset());
                (runningGraph, _) = GetNextGraph(graph, runningGraph, Executor, scheduler.Variables, controlStack, scopedVariables, scheduler.FluidVariableLocations);
            }

            Executor.UpdateDropletData(sumOutputtedDropelts.Values.SelectMany(x => x.Select(y => y.GetFluidConcentrations())).ToList());
        }

        private static bool[] GetusedElectrodes(int width, int height, List<Command>[] commandTimeline, bool enableSparseElectrodes)
        {
            bool[] usedElectrodes = new bool[width * height];
            if (enableSparseElectrodes)
            {
                for (int i = 0; i < usedElectrodes.Length; i++)
                {
                    usedElectrodes[i] = false;
                }
                foreach (List<Command> commands in commandTimeline)
                {
                    if (commands == null)
                    {
                        continue;
                    }

                    foreach (Command command in commands)
                    {
                        if (command.Type == CommandType.ELECTRODE_ON ||
                            command.Type == CommandType.ELECTRODE_OFF)
                        {
                            usedElectrodes[command.Y * width + command.X] = true;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < usedElectrodes.Length; i++)
                {
                    usedElectrodes[i] = true;
                }
            }

            return usedElectrodes;
        }

        public static bool CanOptimizeCDFG(CDFG cdfg)
        {
            return cdfg.Nodes.All(x => x.dfg.Nodes.All(y => !(y is INonDeterministic)));
        }

        public static DFG<Block> OptimizeCDFG<T>(int width, int height, CDFG graph, CancellationToken keepRunning, bool useGC)
        {
            DFG<Block> runningGraph = graph.StartDFG;
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Stack<List<string>> scopedVariables = new Stack<List<string>>();

            controlStack.Push(null);
            scopedVariables.Push(new List<string>());

            DFG<Block> bigDFG = new DFG<Block>();
            Dictionary<string, string> renamer = new Dictionary<string, string>();
            Dictionary<string, string> variablePostfixes = new Dictionary<string, string>();

            Schedule scheduler = new Schedule(width, height);
            scheduler.SHOULD_DO_GARBAGE_COLLECTION = useGC;
            List<StaticDeclarationBlock> staticModuleDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                              .Select(node => node.value as StaticDeclarationBlock)
                                                              .ToList();
            if (staticModuleDeclarations.Count > 0)
            {
                scheduler.PlaceStaticModules(staticModuleDeclarations);
                scopedVariables.Peek().AddRange(scheduler.NewVariablesCreatedInThisScope.Distinct());
            }

            int nameID = 0;

            while (runningGraph != null)
            {
                int time = scheduler.ListScheduling<T>(runningGraph, null);
                scopedVariables.Peek().AddRange(scheduler.NewVariablesCreatedInThisScope.Distinct().Where(x => !x.Contains("#@#Index")));
                runningGraph.Nodes.ForEach(x => x.value.IsDone = false);

                Assay fisk = new Assay(runningGraph);
                foreach (Block toCopy in fisk)
                {
                    if (toCopy is FluidBlock fluidBlockToCopy)
                    {
                        if (!variablePostfixes.ContainsKey(toCopy.OutputVariable))
                        {
                            variablePostfixes.Add(toCopy.OutputVariable, $"##{nameID++}");
                        }

                        Block copy = fluidBlockToCopy.CopyBlock(bigDFG, renamer, variablePostfixes[toCopy.OutputVariable]);

                        bigDFG.AddNode(copy);
                    }

                    fisk.UpdateReadyOperations(toCopy);
                }

                runningGraph.Nodes.ForEach(x => x.value.Reset());

                var dropPositionsCopy = scheduler.FluidVariableLocations.ToDictionary();
                List<string> variablesOutOfScope;
                (runningGraph, variablesOutOfScope) = GetNextGraph(graph, runningGraph, null, scheduler.Variables, controlStack, scopedVariables, scheduler.FluidVariableLocations);

                if (useGC)
                {
                    AddWasteBlocks(variablesOutOfScope, bigDFG, renamer, dropPositionsCopy, staticModuleDeclarations);
                }

                foreach (var item in variablesOutOfScope)
                {
                    renamer.Remove(item);
                    variablePostfixes.Remove(item);
                }

                if (keepRunning.IsCancellationRequested)
                {
                    return null;
                }
            }

            if (useGC)
            {
                AddWasteBlocks(scopedVariables.Pop(), bigDFG, renamer, scheduler.FluidVariableLocations, staticModuleDeclarations);
            }

            bigDFG.FinishDFG();
            return bigDFG;
        }

        private static void AddWasteBlocks(List<string> fluidsOutOfScope, DFG<Block> bigDFG, Dictionary<string, string> renamer, Dictionary<string, BoardFluid> fluidLocations, List<StaticDeclarationBlock> staticModuleDeclarations)
        {
            foreach (string wasteFluidName in fluidsOutOfScope)
            {
                if (!fluidLocations.ContainsKey(wasteFluidName))
                {
                    continue;
                }

                if (staticModuleDeclarations.Any(x => x.OutputVariable == wasteFluidName))
                {
                    continue;
                }

                if (renamer.TryGetValue(wasteFluidName, out string correctedName))
                {
                    int dropletCount = fluidLocations[wasteFluidName].GetNumberOfDropletsAvailable();
                    if (dropletCount > 0)
                    {
                        List<FluidInput> fluidInputs = new List<FluidInput>();
                        fluidInputs.Add(new BasicInput("none", correctedName, dropletCount, false));

                        bigDFG.AddNode(new WasteUsage(Schedule.WASTE_MODULE_NAME, fluidInputs, null, ""));
                    }
                }
            }
        }

        private void StartExecutor(DFG<Block> graph, List<Module> staticModules, bool[] usedElectrodes)
        {
            List<string> inputNames = graph.Nodes.Where(x => x.value is InputDeclaration)
                                                 .Select(x => x.value.OutputVariable)
                                                 .ToList();
            List<Module> inputs = staticModules.Where(x => x is InputModule)
                                             .ToList();
            List<Module> outputs = staticModules.Where(x => x is OutputModule || x is WasteModule)
                                              .Distinct()
                                              .ToList();


            List<Module> staticModulesWithoutInputOutputs = staticModules.Except(inputs).Except(outputs).ToList();

            Executor.StartExecutor(inputNames, inputs, outputs, staticModulesWithoutInputOutputs, usedElectrodes);
        }

        private List<Command>[] CreateCommandTimeline(List<Block> scheduledOperations, int time)
        {
            List<Command>[] commandTimeline = new List<Command>[time + 1];
            foreach (Block operation in scheduledOperations)
            {
                if (operation is FluidBlock fluidBlock)
                {
                    List<Command> commands = fluidBlock.ToCommands();

                    foreach (Command command in commands)
                    {
                        int index = fluidBlock.StartTime + command.Time;

                        commandTimeline[index] = commandTimeline[index] ?? new List<Command>();
                        commandTimeline[index].Add(command);
                    }
                }
            }

            return commandTimeline;
        }

        private void SendCommands(List<Command>[] commandTimeline, ref Rectangle[] oldRectangles, Dictionary<int, Rectangle[]> boardLayouts)
        {
            int time = 0;
            foreach (List<Command> commands in commandTimeline)
            {
                List<Command> showAreaCommands = new List<Command>();
                List<Command> removeAreaCommands = new List<Command>();

                if (commands != null)
                {
                    List<Command> onCommands = commands.Where(x => x.Type == CommandType.ELECTRODE_ON).ToList();
                    List<Command> offCommands = commands.Where(x => x.Type == CommandType.ELECTRODE_OFF).ToList();
                    showAreaCommands.AddRange(commands.Where(x => x.Type == CommandType.SHOW_AREA));
                    removeAreaCommands.AddRange(commands.Where(x => x.Type == CommandType.REMOVE_AREA));

                    if (offCommands.Count > 0)
                    {
                        Executor.QueueCommands(offCommands);
                    }
                    if (onCommands.Count > 0)
                    {
                        Executor.QueueCommands(onCommands);
                    }
                }

                if (ShowEmptyRectangles)
                {
                    oldRectangles = AddRectangleShowCommands(oldRectangles, boardLayouts, time, showAreaCommands, removeAreaCommands);
                }

                removeAreaCommands.ForEach(x => Executor.QueueCommands(new List<Command>() { x }));
                showAreaCommands.ForEach(x => Executor.QueueCommands(new List<Command>() { x }));

                Executor.SendCommands();

                if (KeepRunning.IsCancellationRequested)
                {
                    return;
                }

                if (TimeBetweenCommands > 0)
                {
                    Thread.Sleep(TimeBetweenCommands);
                }
                time++;
            }
        }

        private static Rectangle[] AddRectangleShowCommands(Rectangle[] oldRectangles, Dictionary<int, Rectangle[]> boardLayouts, int time, List<Command> showAreaCommands, List<Command> removeAreaCommands)
        {
            Rectangle[] closestBoardLayout = boardLayouts.Where(x => x.Key <= time).Select(x => x.Value).LastOrDefault();
            closestBoardLayout = closestBoardLayout.Where(x => x.isEmpty).ToArray();

            if (closestBoardLayout != oldRectangles && closestBoardLayout != null)
            {
                var rectanglesToRemove = oldRectangles?.Except(closestBoardLayout);
                if (rectanglesToRemove != null)
                {
                    foreach (var x in rectanglesToRemove)
                    {
                        removeAreaCommands.Add(new AreaCommand(x.x, x.y, x.width, x.height, CommandType.REMOVE_AREA, 0));
                    }
                }

                var rectanglesToShow = closestBoardLayout.Except(oldRectangles ?? new Rectangle[0]);
                foreach (var x in rectanglesToShow)
                {
                    showAreaCommands.Add(new AreaCommand(x.x, x.y, x.width, x.height, CommandType.SHOW_AREA, 0));
                }
            }
            return closestBoardLayout ?? oldRectangles;
        }

        private static (DFG<Block>, List<string>) GetNextGraph(CDFG graph, DFG<Block> currentDFG, CommandExecutor<T> executor, Dictionary<string, float> variables, Stack<IControlBlock> controlStack, Stack<List<string>> scopeStack, Dictionary<string, BoardFluid> dropPositions)
        {
            List<string> variablesOutOfScope = new List<string>();
            {
                IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
                if (control != null)
                {
                    DFG<Block> guardedDFG = control.GuardedDFG(variables, executor, dropPositions);
                    if (guardedDFG != null)
                    {
                        controlStack.Push(control);
                        scopeStack.Push(new List<string>());
                        return (guardedDFG, variablesOutOfScope);
                    }

                    DFG<Block> nextDFG = control.NextDFG(variables, executor, dropPositions);
                    if (nextDFG != null)
                    {
                        return (nextDFG, variablesOutOfScope);
                    }
                }
            }


            while (controlStack.Count > 1)
            {
                IControlBlock control = controlStack.Pop();
                List<string> newVariablesOutOfScope = scopeStack.Pop();
                variablesOutOfScope.AddRange(newVariablesOutOfScope);
                foreach (string variable in newVariablesOutOfScope)
                {
                    if (variables.ContainsKey(variable))
                    {
                        variables.Remove(variable);
                    }
                    else if (dropPositions.ContainsKey(variable))
                    {
                        dropPositions.Remove(variable);
                    }
                }

                DFG<Block> loopDFG = control.TryLoop(variables, executor, dropPositions);
                if (loopDFG != null)
                {
                    controlStack.Push(control);
                    scopeStack.Push(new List<string>());
                    return (loopDFG, variablesOutOfScope);
                }

                DFG<Block> nextDFG = control.NextDFG(variables, executor, dropPositions);
                if (nextDFG != null)
                {
                    return (nextDFG, variablesOutOfScope);
                }
            }

            return (null, variablesOutOfScope);
        }
    }
}
