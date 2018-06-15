﻿using BiolyCompiler.Architechtures;
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

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;
        public int TimeBetweenCommands = 50;
        public bool ShowEmptyRectangles = true;

        public ProgramExecutor(CommandExecutor<T> executor)
        {
            this.Executor = executor;
        }

        public void Run(int width, int height, string xmlText)
        {
            (CDFG graph, List<ParseException> exceptions) = XmlParser.Parse(xmlText);
            if (exceptions.Count > 0)
            {
                return;
            }
            DFG<Block> runningGraph = graph.StartDFG;

            Board board = new Board(width, height);
            ModuleLibrary library = new ModuleLibrary();
            Dictionary<string, BoardFluid> dropPositions = new Dictionary<string, BoardFluid>();
            Dictionary<string, Module> staticModules = new Dictionary<string, Module>();
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<IControlBlock> controlStack = new Stack<IControlBlock>();
            Dictionary<int, Board> boards = new Dictionary<int, Board>();
            List<(int, int, int, int)> oldRectangles = null;
            bool firstRun = true;
            int runNumber = 0;

            while (runningGraph != null)
            {
                //some blocks are able to  change their originaloutputvariable.
                //Those blocks will always appear at the top of a dfg  so the first
                //thing that should be done is to update these blocks originaloutputvariable.
                runningGraph.Nodes.ForEach(node => node.value.Update(variables, Executor, dropPositions));

                List<Module> usedModules;
                (List<Block> scheduledOperations, int time) = MakeSchedule(runningGraph, ref board, ref boards, library, ref dropPositions, ref staticModules, out usedModules);
                runNumber++;
                if (firstRun)
                {
                    StartExecutor(graph, staticModules.Select(pair => pair.Value).ToList());
                    firstRun = false;
                }

                List<Command>[] commandTimeline = CreateCommandTimeline(variables, scheduledOperations, time, dropPositions);

                SendCommands(commandTimeline, ref oldRectangles, boards);

                runningGraph.Nodes.ForEach(x => x.value.Reset());

                runningGraph = GetNextGraph(graph, runningGraph, variables, controlStack, dropPositions);
            }
            Console.Write("");
        }

        private void StartExecutor(CDFG graph, List<Module> staticModules)
        {

            List<Module> inputs = staticModules.Where(x => x is InputModule)
                                             .ToList();
            List<Module> outputs = staticModules.Where(x => x is OutputModule/* || x is Waste*/)
                                              .Distinct()
                                              .ToList();
            List<Module> staticModulesWithoutInputOutputs = staticModules.Except(inputs).Except(outputs).ToList();

            Executor.StartExecutor(inputs, outputs, staticModulesWithoutInputOutputs);
        }

        private List<Command>[] CreateCommandTimeline(Dictionary<string, float> variables, List<Block> scheduledOperations, int time, Dictionary<string, BoardFluid> dropPositions)
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
                else if (operation is VariableBlock varBlock)
                {
                    (string variableName, float value) = varBlock.ExecuteBlock(variables, Executor, dropPositions);
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

            return commandTimeline;
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

                if (TimeBetweenCommands > 0)
                {
                    Thread.Sleep(TimeBetweenCommands);
                }
                time++;
            }
        }

       
       static int numberOfDFGsHandled = 0;
        private (List<Block>, int) MakeSchedule(DFG<Block> runningGraph, ref Board board, ref Dictionary<int, Board> boards, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions, ref Dictionary<string, Module> staticModules, out List<Module> usedModules)
        {
            numberOfDFGsHandled++;
            Schedule scheduler = new Schedule();
            scheduler.TransferFluidVariableLocationInformation(dropPositions);
            scheduler.TransferStaticModulesInformation(staticModules);
            List<StaticDeclarationBlock> staticModuleDeclarations = runningGraph.Nodes.Where(node => node.value is StaticDeclarationBlock)
                                                              .Select(node => node.value as StaticDeclarationBlock)
                                                              .ToList();
            scheduler.PlaceStaticModules(staticModuleDeclarations, board, library);
            Assay assay = new Assay(runningGraph);



            int time = scheduler.ListScheduling(assay, board, library);

            board = scheduler.boardAtDifferentTimes.MaxBy(x => x.Key).Value;
            boards = scheduler.boardAtDifferentTimes;
            dropPositions = scheduler.FluidVariableLocations;
            staticModules = scheduler.StaticModules;

            usedModules = scheduler.AllUsedModules;
            return (scheduler.ScheduledOperations, time);
        }

        private DFG<Block> GetNextGraph(CDFG graph, DFG<Block> currentDFG, Dictionary<string, float> variables, Stack<IControlBlock> controlStack, Dictionary<string, BoardFluid> dropPositions)
        {
            {
                IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
                if (control != null)
                {
                    DFG<Block> guardedDFG = control.GuardedDFG(variables, Executor, dropPositions);
                    if (guardedDFG != null)
                    {
                        controlStack.Push(control);
                        return guardedDFG;
                    }

                    DFG<Block> nextDFG = control.NextDFG(variables, Executor, dropPositions);
                    if (nextDFG != null)
                    {
                        return nextDFG;
                    }
                }
            }


            while (controlStack.Count > 0)
            {
                IControlBlock control = controlStack.Pop();

                DFG<Block> loopDFG = control.TryLoop(variables, Executor, dropPositions);
                if (loopDFG != null)
                {
                    controlStack.Push(control);
                    return loopDFG;
                }

                DFG<Block> nextDFG = control.NextDFG(variables, Executor, dropPositions);
                if (nextDFG != null)
                {
                    return nextDFG;
                }
            }

            return null;
        }
    }
}
