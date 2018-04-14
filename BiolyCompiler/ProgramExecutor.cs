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

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;

        public ProgramExecutor(CommandExecutor<T> executor)
        {
            this.Executor = executor;
        }

        public void Run(int width, int height, string xmlText)
        {
            CDFG graph = XmlParser.Parse(xmlText);
            DFG<Block> runningGraph = graph.StartDFG;

            Board board = new Board(width, height);
            ModuleLibrary library = new ModuleLibrary();
            Dictionary<string, BoardFluid> dropPositions = new Dictionary<string, BoardFluid>();
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<List<string>> varScopeStack = new Stack<List<string>>();
            Stack<Conditional> controlStack = new Stack<Conditional>();
            Stack<int> repeatStack = new Stack<int>();
            bool firstRun = true;

            varScopeStack.Push(new List<string>());
            controlStack.Push(new Conditional(null, null, null));
            repeatStack.Push(0);

            foreach (Input input in runningGraph.Nodes.Select(x => x.value).OfType<Input>())
            {
                BoardFluid fluid = new BoardFluid(input.OutputVariable);
                fluid.droplets.Add((InputModule)input.getAssociatedModule());
                dropPositions.Add(input.OutputVariable, fluid);
            }      

            while (runningGraph != null)
            {
                List<Module> usedModules;
                List<Block> scheduledOperations = MakeSchedule(runningGraph, ref board, library, ref dropPositions, out usedModules);
                if (firstRun)
                {
                    List<Module> inputs = usedModules.Where(x => x is InputModule)
                                                     .ToList();
                    List<Module> outputs = usedModules.Where(x => x is OutputModule/* || x is Waste*/)
                                                      .ToList();
                    Executor.StartExecutor(inputs, outputs);
                    firstRun = false;
                }
                foreach (var operation in scheduledOperations)
                {
                    if (operation is FluidBlock fluidBLock)
                    {
                        ExecuteCommands(fluidBLock.boundModule.ToCommands());
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


                runningGraph.Nodes.ForEach(x => x.value.Reset());

                runningGraph = GetNextGraph(graph, runningGraph, variables, varScopeStack, controlStack, repeatStack);
            }
        }

        private List<Block> MakeSchedule(DFG<Block> runningGraph, ref Board board, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions, out List<Module> usedModules)
        {
            Assay assay = new Assay(runningGraph);
            Schedule scheduler = new Schedule();
            scheduler.TransferFluidVariableLocationInformation(dropPositions);
            scheduler.ListScheduling(assay, board, library);

            board = scheduler.boardAtDifferentTimes.MaxBy(x => x.Key).Value;
            dropPositions = scheduler.FluidVariableLocations;

            usedModules = scheduler.allUsedModules;
            return scheduler.ScheduledOperations;
        }

        private void ExecuteCommands(List<Command> commands)
        {
            int prevTime = commands.First().Time;
            for (int i = 0; i < commands.Count; i++)
            {
                List<Command> similarCommands = new List<Command>();
                Command prevCommand = null;
                Command command = commands[i];
                if (command.Time < prevTime)
                {
                    prevTime = command.Time;
                }
                do
                {
                    if (prevCommand != null)
                    {
                        i++;
                    }
                    similarCommands.Add(command);
                    if (i + 1 < commands.Count)
                    {
                        prevCommand = command;
                        command = commands[i + 1];
                    }
                    else
                    {
                        break;
                    }
                } while (prevCommand.GetType() == command.GetType() &&
                         prevCommand.Time == command.Time &&
                         prevCommand.Type == command.Type);
                Executor.SendCommands(similarCommands);

                if (prevTime < command.Time)
                {
                    Thread.Sleep(500);
                    prevTime = command.Time;
                }
            }
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
