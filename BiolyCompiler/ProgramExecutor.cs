using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;
using MoreLinq;
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

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;
        public int TimeBetweenCommands = 50;
        public bool ShowEmptyRectangles = true;
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

            Board board = new Board(width, height);
            ModuleLibrary library = new ModuleLibrary();
            Dictionary<string, Module> staticModules = new Dictionary<string, Module>();

            Dictionary<string, BoardFluid> dropPositions = new Dictionary<string, BoardFluid>();
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Stack<List<string>> scopedVariables = new Stack<List<string>>();

            Dictionary<int, Board> boards = new Dictionary<int, Board>();
            List<(int, int, int, int)> oldRectangles = null;
            bool firstRun = true;

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

                Dictionary<string, List<Droplet>> outputtedDroplets;
                (List<Block> scheduledOperations, int time) = MakeSchedule(OptimizedDFG, ref board, ref boards, library, ref dropPositions, ref staticModules, out outputtedDroplets);

                List<Command>[] commandTimeline = CreateCommandTimeline(scheduledOperations, time);
                bool[] usedElectrodes = GetusedElectrodes(width, height, commandTimeline, EnableSparseElectrodes);

                StartExecutor(OptimizedDFG, staticModules.Select(pair => pair.Value).ToList(), usedElectrodes);
                Executor.UpdateDropletData(outputtedDroplets.Values.SelectMany(x => x.Select(y => y.FluidConcentrations)).ToList());
                SendCommands(commandTimeline, ref oldRectangles, boards);
            }
            else
            { 
                while (runningGraph != null)
                {
                    //some blocks are able to  change their originaloutputvariable.
                    //Those blocks will always appear at the top of a dfg  so the first
                    //thing that should be done is to update these blocks originaloutputvariable.
                    runningGraph.Nodes.ForEach(node => node.value.Update(variables, Executor, dropPositions));

                    HashSet<string> fluidVariablesBefore = dropPositions.Keys.ToHashSet();
                    (List<Block> scheduledOperations, int time) = MakeSchedule(runningGraph, ref board, ref boards, library, ref dropPositions, ref staticModules, out _);
                    HashSet<string> fluidVariablesAfter = dropPositions.Keys.ToHashSet();
                    scopedVariables.Peek().AddRange(fluidVariablesAfter.Except(fluidVariablesBefore).Where(x => !x.Contains("#@#Index")));

                    if (firstRun)
                    {
                        bool[] usedElectrodes = new bool[width * height].Select(x => true).ToArray();
                        StartExecutor(graph.StartDFG, staticModules.Select(pair => pair.Value).ToList(), usedElectrodes);
                        firstRun = false;
                    }

                    HashSet<string> numberVariablesBefore = variables.Keys.ToHashSet();
                    UpdateVariables(variables, Executor, scheduledOperations, dropPositions);
                    HashSet<string> numberVariablesAfter = variables.Keys.ToHashSet();
                    scopedVariables.Peek().AddRange(numberVariablesAfter.Except(numberVariablesBefore).Where(x => !x.Contains("#@#Index")));

                    List<Command>[] commandTimeline = CreateCommandTimeline(scheduledOperations, time);
                    SendCommands(commandTimeline, ref oldRectangles, boards);

                    if (KeepRunning.IsCancellationRequested)
                    {
                        return;
                    }

                    runningGraph.Nodes.ForEach(x => x.value.Reset());
                    runningGraph = GetNextGraph(graph, runningGraph, Executor, variables, controlStack, scopedVariables, dropPositions);
                }
            }
        }

        private static bool[] GetusedElectrodes(int width, int height, List<Command>[] commandTimeline, bool enableSparseElectrodes)
        {
            bool[] usedElectrodes = new bool[width * height];
            for (int i = 0; i < usedElectrodes.Length; i++)
            {
                usedElectrodes[i] = false;
            }
            if (enableSparseElectrodes)
            {
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

            return usedElectrodes;
        }

        public static bool CanOptimizeCDFG(CDFG cdfg)
        {
            return cdfg.Nodes.All(x => x.dfg.Nodes.All(y => !(y is INonDeterministic)));
        }

        public static DFG<Block> OptimizeCDFG(int width, int height, CDFG graph, CancellationToken keepRunning, bool useGC)
        {
            DFG<Block> runningGraph = graph.StartDFG;

            Board board = new Board(2*width, 2*height);
            ModuleLibrary library = new ModuleLibrary();
            Dictionary<string, Module> staticModules = new Dictionary<string, Module>();

            Dictionary<string, BoardFluid> dropPositions = new Dictionary<string, BoardFluid>();
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Stack<List<string>> scopedVariables = new Stack<List<string>>();

            Dictionary<int, Board> boards = new Dictionary<int, Board>();

            controlStack.Push(null);
            scopedVariables.Push(new List<string>());

            DFG<Block> bigDFG = new DFG<Block>();
            Dictionary<string, string> mostRecentRef = new Dictionary<string, string>();
            Dictionary<string, string> renamer = new Dictionary<string, string>();
            Dictionary<string, string> variablePostfixes = new Dictionary<string, string>();

            int nameID = 0;

            while (runningGraph != null)
            {
                //some blocks are able to  change their originaloutputvariable.
                //Those blocks will always appear at the top of a dfg  so the first
                //thing that should be done is to update these blocks originaloutputvariable.
                runningGraph.Nodes.ForEach(node => node.value.Update<T>(variables, null, dropPositions));

                HashSet<string> fluidVariablesBefore = dropPositions.Keys.ToHashSet();
                (List<Block> scheduledOperations, int time) = MakeSchedule(runningGraph, ref board, ref boards, library, ref dropPositions, ref staticModules, out _);
                HashSet<string> fluidVariablesAfter = dropPositions.Keys.ToHashSet();
                scopedVariables.Peek().AddRange(fluidVariablesAfter.Except(fluidVariablesBefore).Where(x => !x.Contains("#@#Index")));


                HashSet<string> numberVariablesBefore = variables.Keys.ToHashSet();
                UpdateVariables(variables, null, scheduledOperations, dropPositions);
                HashSet<string> numberVariablesAfter = variables.Keys.ToHashSet();
                scopedVariables.Peek().AddRange(numberVariablesAfter.Except(numberVariablesBefore).Where(x => !x.Contains("#@#Index")));

                runningGraph.Nodes.ForEach(x => x.value.IsDone = false);
                Assay fisk = new Assay(runningGraph);

                var cake = fisk.GetReadyOperations();
                while (cake.Count > 0)
                {
                    Block toCopy = cake.Dequeue();
                    if (toCopy is FluidBlock fluidBlockToCopy)
                    {
                        if (!variablePostfixes.ContainsKey(toCopy.OriginalOutputVariable))
                        {
                            variablePostfixes.Add(toCopy.OriginalOutputVariable, $"##{nameID++}");
                        }

                        Block copy = fluidBlockToCopy.CopyBlock(bigDFG, mostRecentRef, renamer, variablePostfixes[toCopy.OriginalOutputVariable]);

                        bigDFG.AddNode(copy);

                        if (mostRecentRef.ContainsKey(copy.OriginalOutputVariable))
                        {
                            mostRecentRef[copy.OriginalOutputVariable] = copy.OutputVariable;
                        }
                        else
                        {
                            mostRecentRef.Add(copy.OriginalOutputVariable, copy.OutputVariable);
                        }
                    }

                    fisk.UpdateReadyOperations(toCopy);
                }

                runningGraph.Nodes.ForEach(x => x.value.Reset());

                var dropPositionsCopy = dropPositions.ToDictionary();
                fluidVariablesBefore = dropPositions.Keys.ToHashSet();
                numberVariablesBefore = variables.Keys.ToHashSet();
                runningGraph = GetNextGraph(graph, runningGraph, null, variables, controlStack, scopedVariables, dropPositions);
                fluidVariablesAfter = dropPositions.Keys.ToHashSet();
                numberVariablesAfter = variables.Keys.ToHashSet();
                List<string> fluidsOutOfScope = fluidVariablesBefore.Except(fluidVariablesAfter).ToList();
                List<string> numbersOutOfScope = numberVariablesBefore.Except(numberVariablesAfter).ToList();

                if (useGC)
                {
                    foreach (string wasteFluidName in fluidsOutOfScope)
                    {
                        if (renamer.TryGetValue(wasteFluidName, out string correctedName))
                        {
                            string instanceName = mostRecentRef[renamer[wasteFluidName]];
                            int dropletCount = dropPositionsCopy[wasteFluidName].GetNumberOfDropletsAvailable();
                            if (dropletCount > 0)
                            {
                                List<FluidInput> fluidInputs = new List<FluidInput>();
                                fluidInputs.Add(new BasicInput("none", instanceName, correctedName, dropletCount, false));

                                bigDFG.AddNode(new WasteUsage(Schedule.WASTE_MODULE_NAME, fluidInputs, null, ""));
                            }
                        }
                    }
                }



                fluidsOutOfScope.ForEach(x => mostRecentRef.Remove(x));
                numbersOutOfScope.ForEach(x => mostRecentRef.Remove(x));
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
                    if (staticBlocks.Any(x => x.value.OriginalOutputVariable == wasteFluidName))
                    {
                        continue;
                    }

                    if (renamer.TryGetValue(wasteFluidName, out string correctedName)) 
                    {
                        string instanceName = mostRecentRef[renamer[wasteFluidName]];
                        int dropletCount = dropPositions[wasteFluidName].GetNumberOfDropletsAvailable();
                        if (dropletCount > 0)
                        {
                            List<FluidInput> fluidInputs = new List<FluidInput>();
                            fluidInputs.Add(new BasicInput("none", instanceName, correctedName, dropletCount, false));

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
                                                 .Select(x => x.value.OriginalOutputVariable)
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

        private void SendCommands(List<Command>[] commandTimeline, ref List<(int, int, int, int)> oldRectangles, Dictionary<int, Board> boards)
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
                    List<(int, int, int, int)> closestBoardData = boards.MinBy(x => x.Key - time < 0 ? int.MaxValue : x.Key - time)
                                                                        .Value.EmptyRectangles
                                                                        .Values
                                                                        .Select(rectangle => (rectangle.x, rectangle.y, rectangle.width, rectangle.height))
                                                                        .ToList();
                    if (closestBoardData != oldRectangles && closestBoardData != null)
                    {
                        var rectanglesToRemove = oldRectangles?.Except(closestBoardData);
                        rectanglesToRemove?.ForEach(x => removeAreaCommands.Add(new AreaCommand(x.Item1, x.Item2, x.Item3, x.Item4, CommandType.REMOVE_AREA, 0)));

                        var rectanglesToShow = closestBoardData.Except(oldRectangles ?? new List<(int, int, int, int)>());
                        rectanglesToShow.ForEach(x => showAreaCommands.Add(new AreaCommand(x.Item1, x.Item2, x.Item3, x.Item4, CommandType.SHOW_AREA, 0)));
                    }
                    oldRectangles = closestBoardData ?? oldRectangles;
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

       
       static int numberOfDFGsHandled = 0;
        private static (List<Block>, int) MakeSchedule(DFG<Block> runningGraph, ref Board board, ref Dictionary<int, Board> boards, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions, ref Dictionary<string, Module> staticModules, out Dictionary<string, List<Droplet>> outputtedDroplets)
        {
            numberOfDFGsHandled++;
            Schedule scheduler = new Schedule();
            scheduler.TransferFluidVariableLocationInformation(dropPositions);
            scheduler.TransferStaticModulesInformation(staticModules);
            List<StaticDeclarationBlock> staticModuleDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                              .Select(node => node.value as StaticDeclarationBlock)
                                                              .ToList();
            if (staticModuleDeclarations.Count > 0)
            {
                scheduler.PlaceStaticModules(staticModuleDeclarations, board, library);
            }
            Assay assay = new Assay(runningGraph);



            int time = scheduler.ListScheduling(assay, board, library);

            board = scheduler.boardAtDifferentTimes.MaxBy(x => x.Key).Value;
            boards = scheduler.boardAtDifferentTimes;
            dropPositions = scheduler.FluidVariableLocations;
            staticModules = scheduler.StaticModules;
            outputtedDroplets = scheduler.OutputtedDroplets;


            return (scheduler.ScheduledOperations, time);
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
