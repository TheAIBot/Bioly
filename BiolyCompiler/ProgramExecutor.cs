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

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;
        public int TIME_BETWEEN_COMMANDS = 50;
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
            Stack<List<string>> varScopeStack = new Stack<List<string>>();
            Stack<Conditional> controlStack = new Stack<Conditional>();
            Stack<(int, DFG<Block>)> repeatStack = new Stack<(int, DFG<Block>)>();
            Dictionary<int, Board> boards = new Dictionary<int, Board>();
            Board oldBoard = null;
            bool firstRun = true;

            varScopeStack.Push(new List<string>());
            controlStack.Push(new Conditional(null, null, null));
            repeatStack.Push((0,null));

            while (runningGraph != null)
            {
                //some blocks are able to  change their originaloutputvariable.
                //Those blocks will always appear at the top of a dfg  so the first
                //thing that should be done is to update these blocks originaloutputvariable.
                runningGraph.Nodes.ForEach(node => node.value.Update(variables, Executor, dropPositions));

                List<Module> usedModules;
                (List<Block> scheduledOperations, int time) = MakeSchedule(runningGraph, ref board, ref boards, library, ref dropPositions, ref staticModules, out usedModules);
                if (firstRun)
                {
                    StartExecutor(graph, staticModules.Select(pair => pair.Value).ToList());
                    firstRun = false;
                }

                List<Command>[] commandTimeline = CreateCommandTimeline(variables, varScopeStack, scheduledOperations, time, dropPositions);

                SendCommands(commandTimeline, ref oldBoard, boards);

                runningGraph.Nodes.ForEach(x => x.value.Reset());

                runningGraph = GetNextGraph(graph, runningGraph, variables, varScopeStack, controlStack, repeatStack, dropPositions);
            }
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

        private List<Command>[] CreateCommandTimeline(Dictionary<string, float> variables, Stack<List<string>> varScopeStack, List<Block> scheduledOperations, int time, Dictionary<string, BoardFluid> dropPositions)
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
                        varScopeStack.Peek().Add(variableName);
                    }
                    else
                    {
                        variables[variableName] = value;
                    }
                }
            }

            return commandTimeline;
        }

        private void SendCommands(List<Command>[] commandTimeline, ref Board oldBoard, Dictionary<int, Board> boards)
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
                    var closestsBoard = boards.MinBy(x => Math.Abs(x.Key - time));
                    if (closestsBoard.Value != oldBoard && closestsBoard.Value != null)
                    {
                        var emptyRectanglesToRemove = oldBoard?.EmptyRectangles.Except(closestsBoard.Value.EmptyRectangles);
                        emptyRectanglesToRemove?.ForEach(x => removeAreaCommands.Add(new AreaCommand(x, CommandType.REMOVE_AREA, 0)));

                        var emptyRectanglesToShow = closestsBoard.Value.EmptyRectangles.Except(oldBoard?.EmptyRectangles ?? new HashSet<Rectangle>());
                        emptyRectanglesToShow.ForEach(x => showAreaCommands.Add(new AreaCommand(x, CommandType.SHOW_AREA, 0)));
                    }
                    oldBoard = closestsBoard.Value ?? oldBoard;
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

                if (TIME_BETWEEN_COMMANDS > 0)
                {
                    Thread.Sleep(TIME_BETWEEN_COMMANDS);
                }
                time++;
            }
        }

        private (List<Block>, int) MakeSchedule(DFG<Block> runningGraph, ref Board board, ref Dictionary<int, Board> boards, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions, ref Dictionary<string, Module> staticModules, out List<Module> usedModules)
        {
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

        private DFG<Block> GetNextGraph(CDFG graph, DFG<Block> currentDFG, Dictionary<string, float> variables, Stack<List<string>> varScopeStack, Stack<Conditional> controlStack, Stack<(int, DFG<Block>)> repeatStack, Dictionary<string, BoardFluid> dropPositions)
        {
            IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
            if (control is If ifControl)
            {
                foreach (Conditional conditional in ifControl.IfStatements)
                {
                    //if result is 1 then take the if block
                    if (1f == conditional.DecidingBlock.Run(variables, Executor, dropPositions))
                    {
                        controlStack.Push(conditional);
                        varScopeStack.Push(new List<string>());
                        repeatStack.Push((0, conditional.NextDFG));
                        return conditional.GuardedDFG;
                    }
                }
                return ifControl.IfStatements.First().NextDFG;
            }
            else if (control is Repeat repeatControl)
            {
                int loopCount = (int)repeatControl.Cond.DecidingBlock.Run(variables, Executor, dropPositions);
                if (loopCount > 0)
                {
                    controlStack.Push(repeatControl.Cond);
                    varScopeStack.Push(new List<string>());
                    repeatStack.Push((--loopCount, repeatControl.Cond.GuardedDFG));
                    return repeatControl.Cond.GuardedDFG;
                }
                else
                {
                    return repeatControl.Cond.NextDFG;
                }
            }
            else if (control is Direct directControl)
            {
                return directControl.Cond.NextDFG;
            }

            while (repeatStack.Count > 0)
            {
                //if inside repeat block and still
                //need to repeat
                if (repeatStack.Peek().Item1 > 0)
                {
                    (var repeatCount, var dfg) = repeatStack.Pop();
                    repeatStack.Push((repeatCount - 1, dfg));
                    return dfg;
                } else if (controlStack.Peek().NextDFG != null)
                {
                    DFG<Block> nextDFG = controlStack.Peek().NextDFG;
                    controlStack.Pop();
                    varScopeStack.Pop();
                    repeatStack.Pop();
                    return nextDFG;
                } else
                {
                    controlStack.Pop();
                    varScopeStack.Pop();
                    repeatStack.Pop();
                }

            }

            return null;
        }
    }
}
