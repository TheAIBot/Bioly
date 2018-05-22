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
        public const int TIME_BETWEEN_COMMANDS = 50;

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
            Stack<int> repeatStack = new Stack<int>();
            bool firstRun = true;

            varScopeStack.Push(new List<string>());
            controlStack.Push(new Conditional(null, null, null));
            repeatStack.Push(0);

            while (runningGraph != null)
            {
                List<Module> usedModules;
                (List<Block> scheduledOperations, int time) = MakeSchedule(runningGraph, ref board, library, ref dropPositions, ref staticModules, out usedModules);
                if (firstRun)
                {
                    StartExecutor(graph, staticModules.Select(pair => pair.Value).ToList());
                    firstRun = false;
                }

                List<Command>[] commandTimeline = CreateCommandTimeline(variables, varScopeStack, scheduledOperations, time);

                SendCommands(commandTimeline);

                runningGraph.Nodes.ForEach(x => x.value.Reset());

                runningGraph = GetNextGraph(graph, runningGraph, variables, varScopeStack, controlStack, repeatStack);
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

        private List<Command>[] CreateCommandTimeline(Dictionary<string, float> variables, Stack<List<string>> varScopeStack, List<Block> scheduledOperations, int time)
        {
            List<Command>[] commandTimeline = new List<Command>[time + 1];
            foreach (Block operation in scheduledOperations)
            {
                if (operation is FluidBlock fluidBlock)
                {
                    List<Command> commands;
                    if (operation is Fluid fluidOperation)
                    {
                        commands = fluidOperation.GetFluidTransferOperations();
                    }
                    else
                    {
                        commands = fluidBlock.ToCommands();
                    }

                    foreach (Command command in commands)
                    {
                        int index = fluidBlock.StartTime + command.Time;

                        commandTimeline[index] = commandTimeline[index] ?? new List<Command>();
                        commandTimeline[index].Add(command);
                    }
                }
                else if (operation is VariableBlock varBlock)
                {
                    (string variableName, float value) = varBlock.ExecuteBlock(variables, Executor);
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

        private void SendCommands(List<Command>[] commandTimeline)
        {
            int time = 0;
            foreach (List<Command> commands in commandTimeline)
            {
                if (commands != null)
                {
                    List<Command> onCommands = commands.Where(x => x.Type == CommandType.ELECTRODE_ON).ToList();
                    List<Command> offCommands = commands.Where(x => x.Type == CommandType.ELECTRODE_OFF).ToList();
                    List<Command> showAreaCommands = commands.Where(x => x.Type == CommandType.SHOW_AREA).ToList();
                    List<Command> removeAreaCommands = commands.Where(x => x.Type == CommandType.REMOVE_AREA).ToList();

                    if (offCommands.Count > 0)
                    {
                        Executor.QueueCommands(offCommands);
                    }
                    if (onCommands.Count > 0)
                    {
                        Executor.QueueCommands(onCommands);
                    }
                    if (showAreaCommands.Count > 0)
                    {
                        Executor.QueueCommands(showAreaCommands);
                    }
                    if (removeAreaCommands.Count > 0)
                    {
                        Executor.QueueCommands(removeAreaCommands);
                    }

                    Executor.SendCommands();
                }

                Thread.Sleep(TIME_BETWEEN_COMMANDS);
                time++;
            }
        }

        private (List<Block>, int) MakeSchedule(DFG<Block> runningGraph, ref Board board, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions, ref Dictionary<string, Module> staticModules, out List<Module> usedModules)
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
            dropPositions = scheduler.FluidVariableLocations;
            staticModules = scheduler.StaticModules;

            usedModules = scheduler.AllUsedModules;
            return (scheduler.ScheduledOperations, time);
        }

        private DFG<Block> GetNextGraph(CDFG graph, DFG<Block> currentDFG, Dictionary<string, float> variables, Stack<List<string>> varScopeStack, Stack<Conditional> controlStack, Stack<int> repeatStack)
        {
            IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
            if (control is If ifControl)
            {
                foreach (Conditional conditional in ifControl.IfStatements)
                {
                    //if result is 1 then take the if block
                    if (1f == conditional.DecidingBlock.Run(variables, Executor))
                    {
                        controlStack.Push(conditional);
                        varScopeStack.Push(new List<string>());
                        repeatStack.Push(0);
                        return conditional.GuardedDFG;
                    }
                }
                return ifControl.IfStatements.First().NextDFG;
            }
            else if (control is Repeat repeatControl)
            {
                int loopCount = (int)repeatControl.Cond.DecidingBlock.Run(variables, Executor);
                controlStack.Push(repeatControl.Cond);
                varScopeStack.Push(new List<string>());
                repeatStack.Push(--loopCount);
                return repeatControl.Cond.GuardedDFG;
            }

            while (repeatStack.Count > 0)
            {
                //if inside repeat block and still
                //need to repeat
                if (repeatStack.Peek() > 0)
                {
                    repeatStack.Push(repeatStack.Pop() - 1);
                    return currentDFG;
                }

                if (controlStack.Peek().NextDFG != null)
                {
                    DFG<Block> nextDFG = controlStack.Peek().NextDFG;
                    controlStack.Pop();
                    varScopeStack.Pop();
                    repeatStack.Pop();

                    return nextDFG;
                }

                controlStack.Pop();
                varScopeStack.Pop();
                repeatStack.Pop();
            }

            return null;
        }
    }
}
