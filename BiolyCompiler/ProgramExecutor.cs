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
            DFG<Block> runningGraph = graph.StartDFG;
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Stack<List<string>> scopedVariables = new Stack<List<string>>();
            Rectangle[] oldRectangles = null;
            bool firstRun = true;

            Dictionary<string, List<IDropletSource>> sumOutputtedDropelts = new Dictionary<string, List<IDropletSource>>();

            controlStack.Push(null);
            scopedVariables.Push(new List<string>());

            if (CanOptimizeCDFG(graph) && EnableOptimizations)
            {
                if (alreadyOptimized)
                {
                    OptimizedDFG = runningGraph;
                }
                else
                {
                    OptimizedDFG = OptimizeCDFG(width, height, graph, KeepRunning.Token, EnableGarbageCollection);
                }

                if (KeepRunning.IsCancellationRequested)
                {
                    return;
                }

                Schedule scheduler = new Schedule(width, height);
                scheduler.SHOULD_DO_GARBAGE_COLLECTION = EnableGarbageCollection;
                List<StaticDeclarationBlock> staticModuleDeclarations = OptimizedDFG.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                                  .Select(node => node.value as StaticDeclarationBlock)
                                                                  .ToList();
                if (staticModuleDeclarations.Count > 0)
                {
                    scheduler.PlaceStaticModules(staticModuleDeclarations);
                }

                int time = scheduler.ListScheduling(OptimizedDFG);
                List<Block> scheduledOperations = scheduler.ScheduledOperations;

                List<Command>[] commandTimeline = CreateCommandTimeline(scheduledOperations, time);
                bool[] usedElectrodes = GetusedElectrodes(width, height, commandTimeline, EnableSparseElectrodes);

                StartExecutor(OptimizedDFG, scheduler.StaticModules.Select(pair => pair.Value).ToList(), usedElectrodes);
                Executor.UpdateDropletData(scheduler.OutputtedDroplets.Values.SelectMany(x => x.Select(y => y.GetFluidConcentrations())).ToList());
                SendCommands(commandTimeline, ref oldRectangles, scheduler.rectanglesAtDifferentTimes);
            }
            else
            {
                Schedule scheduler = new Schedule(width, height);
                scheduler.SHOULD_DO_GARBAGE_COLLECTION = EnableGarbageCollection;
                List<StaticDeclarationBlock> staticModuleDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                                  .Select(node => node.value as StaticDeclarationBlock)
                                                                  .ToList();
                if (staticModuleDeclarations.Count > 0)
                {
                    scheduler.PlaceStaticModules(staticModuleDeclarations);
                }

                while (runningGraph != null)
                {
                    //some blocks are able to  change their originaloutputvariable.
                    //Those blocks will always appear at the top of a dfg  so the first
                    //thing that should be done is to update these blocks originaloutputvariable.
                    runningGraph.Nodes.ForEach(node => node.value.Update(variables, Executor, scheduler.FluidVariableLocations));

                    HashSet<string> fluidVariablesBefore = scheduler.FluidVariableLocations.Keys.ToHashSet();
                    int time = scheduler.ListScheduling(runningGraph);
                    List<Block> scheduledOperations = scheduler.ScheduledOperations;
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
                    HashSet<string> fluidVariablesAfter = scheduler.FluidVariableLocations.Keys.ToHashSet();
                    scopedVariables.Peek().AddRange(fluidVariablesAfter.Except(fluidVariablesBefore).Where(x => !x.Contains("#@#Index")));

                    if (firstRun)
                    {
                        bool[] usedElectrodes = new bool[width * height].Select(x => true).ToArray();
                        StartExecutor(graph.StartDFG, scheduler.StaticModules.Select(pair => pair.Value).ToList(), usedElectrodes);
                        firstRun = false;
                    }

                    HashSet<string> numberVariablesBefore = variables.Keys.ToHashSet();
                    UpdateVariables(variables, Executor, scheduledOperations, scheduler.FluidVariableLocations);
                    HashSet<string> numberVariablesAfter = variables.Keys.ToHashSet();
                    scopedVariables.Peek().AddRange(numberVariablesAfter.Except(numberVariablesBefore).Where(x => !x.Contains("#@#Index")));

                    List<Command>[] commandTimeline = CreateCommandTimeline(scheduledOperations, time);
                    SendCommands(commandTimeline, ref oldRectangles, scheduler.rectanglesAtDifferentTimes);

                    if (KeepRunning.IsCancellationRequested)
                    {
                        return;
                    }

                    runningGraph.Nodes.ForEach(x => x.value.Reset());
                    runningGraph = GetNextGraph(graph, runningGraph, Executor, variables, controlStack, scopedVariables, scheduler.FluidVariableLocations);
                }

                Executor.UpdateDropletData(sumOutputtedDropelts.Values.SelectMany(x => x.Select(y => y.GetFluidConcentrations())).ToList());
            }
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

        public static DFG<Block> OptimizeCDFG(int width, int height, CDFG graph, CancellationToken keepRunning, bool useGC)
        {
            DFG<Block> runningGraph = graph.StartDFG;
            Dictionary<string, float> variables = new Dictionary<string, float>();
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
            }

            int nameID = 0;

            while (runningGraph != null)
            {
                //some blocks are able to  change their originaloutputvariable.
                //Those blocks will always appear at the top of a dfg  so the first
                //thing that should be done is to update these blocks originaloutputvariable.
                runningGraph.Nodes.ForEach(node => node.value.Update<T>(variables, null, scheduler.FluidVariableLocations));

                HashSet<string> fluidVariablesBefore = scheduler.FluidVariableLocations.Keys.ToHashSet();
                int time = scheduler.ListScheduling(runningGraph);
                List<Block> scheduledOperations = scheduler.ScheduledOperations;
                HashSet<string> fluidVariablesAfter = scheduler.FluidVariableLocations.Keys.ToHashSet();
                scopedVariables.Peek().AddRange(fluidVariablesAfter.Except(fluidVariablesBefore).Where(x => !x.Contains("#@#Index")));


                HashSet<string> numberVariablesBefore = variables.Keys.ToHashSet();
                UpdateVariables(variables, null, scheduledOperations, scheduler.FluidVariableLocations);
                HashSet<string> numberVariablesAfter = variables.Keys.ToHashSet();
                scopedVariables.Peek().AddRange(numberVariablesAfter.Except(numberVariablesBefore).Where(x => !x.Contains("#@#Index")));

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
                fluidVariablesBefore = scheduler.FluidVariableLocations.Keys.ToHashSet();
                numberVariablesBefore = variables.Keys.ToHashSet();
                runningGraph = GetNextGraph(graph, runningGraph, null, variables, controlStack, scopedVariables, scheduler.FluidVariableLocations);
                fluidVariablesAfter = scheduler.FluidVariableLocations.Keys.ToHashSet();
                numberVariablesAfter = variables.Keys.ToHashSet();
                List<string> fluidsOutOfScope = fluidVariablesBefore.Except(fluidVariablesAfter).ToList();
                List<string> numbersOutOfScope = numberVariablesBefore.Except(numberVariablesAfter).ToList();

                if (useGC)
                {
                    foreach (string wasteFluidName in fluidsOutOfScope)
                    {
                        if (renamer.TryGetValue(wasteFluidName, out string correctedName))
                        {
                            if (dropPositionsCopy[wasteFluidName].RefCount > 1)
                            {
                                dropPositionsCopy[wasteFluidName].RefCount--;
                                continue;
                            }
                            int dropletCount = dropPositionsCopy[wasteFluidName].GetNumberOfDropletsAvailable();
                            if (dropletCount > 0)
                            {
                                List<FluidInput> fluidInputs = new List<FluidInput>();
                                fluidInputs.Add(new BasicInput("none", correctedName, dropletCount, false));

                                bigDFG.AddNode(new WasteUsage(Schedule.WASTE_MODULE_NAME, fluidInputs, null, ""));
                            }
                        }
                    }
                }

                fluidsOutOfScope.ForEach(x => renamer.Remove(x));
                numbersOutOfScope.ForEach(x => renamer.Remove(x));
                fluidsOutOfScope.ForEach(x => variablePostfixes.Remove(x));
                numbersOutOfScope.ForEach(x => variablePostfixes.Remove(x));

                if (keepRunning.IsCancellationRequested)
                {
                    return null;
                }
            }

            if (useGC)
            {
                var staticBlocks = graph.StartDFG.Nodes.Where(x => x.value is StaticDeclarationBlock);
                foreach (string wasteFluidName in scopedVariables.Pop())
                {
                    if (staticBlocks.Any(x => x.value.OutputVariable == wasteFluidName))
                    {
                        continue;
                    }

                    if (renamer.TryGetValue(wasteFluidName, out string correctedName)) 
                    {
                        if (scheduler.FluidVariableLocations[wasteFluidName].RefCount > 1)
                        {
                            scheduler.FluidVariableLocations[wasteFluidName].RefCount--;
                            continue;
                        }
                        int dropletCount = scheduler.FluidVariableLocations[wasteFluidName].GetNumberOfDropletsAvailable();
                        if (dropletCount > 0)
                        {
                            List<FluidInput> fluidInputs = new List<FluidInput>();
                            fluidInputs.Add(new BasicInput("none", correctedName, dropletCount, false));

                            bigDFG.AddNode(new WasteUsage(Schedule.WASTE_MODULE_NAME, fluidInputs, null, ""));
                        }
                    }
                }
            }

            bigDFG.FinishDFG();
            return bigDFG;
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

        private static void UpdateVariables(Dictionary<string, float> variables, CommandExecutor<T> executor, List<Block> scheduledOperations, Dictionary<string, BoardFluid> dropPositions)
        {
            foreach (Block operation in scheduledOperations)
            {
                if (operation is VariableBlock varBlock)
                {
                    (string variableName, float value) = varBlock.ExecuteBlock(variables, executor, dropPositions);
                    if (float.IsInfinity(value) || float.IsNaN(value))
                    {
                        throw new InvalidNumberException(varBlock.BlockID, value);
                    }
                    if (!variables.ContainsKey(variableName))
                    {
                        variables.Add(variableName, value);
                    }
                    else
                    {
                        variables[variableName] = value;
                    }
                }
            }
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
                    oldRectangles = closestBoardLayout ?? oldRectangles;
                }

                if (removeAreaCommands.Count > 0)
                {
                    removeAreaCommands.ForEach(x => Executor.QueueCommands(new List<Command>() { x }));
                }
                if (showAreaCommands.Count > 0)
                {
                    showAreaCommands.ForEach(x => Executor.QueueCommands(new List<Command>() { x }));
                }

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

        private static DFG<Block> GetNextGraph(CDFG graph, DFG<Block> currentDFG, CommandExecutor<T> executor, Dictionary<string, float> variables, Stack<IControlBlock> controlStack, Stack<List<string>> scopeStack, Dictionary<string, BoardFluid> dropPositions)
        {
            {
                IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
                if (control != null)
                {
                    DFG<Block> guardedDFG = control.GuardedDFG(variables, executor, dropPositions);
                    if (guardedDFG != null)
                    {
                        controlStack.Push(control);
                        scopeStack.Push(new List<string>());
                        return guardedDFG;
                    }

                    DFG<Block> nextDFG = control.NextDFG(variables, executor, dropPositions);
                    if (nextDFG != null)
                    {
                        return nextDFG;
                    }
                }
            }


            while (controlStack.Count > 1)
            {
                IControlBlock control = controlStack.Pop();
                foreach (string variable in scopeStack.Pop())
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
                    return loopDFG;
                }

                DFG<Block> nextDFG = control.NextDFG(variables, executor, dropPositions);
                if (nextDFG != null)
                {
                    return nextDFG;
                }
            }

            return null;
        }
    }
}
