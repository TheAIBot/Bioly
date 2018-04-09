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

namespace BiolyCompiler
{
    public class ProgramExecutor<T>
    {
        private readonly CommandExecutor<T> Executor;

        public ProgramExecutor(CommandExecutor<T> executor)
        {
            this.Executor = executor;
        }

        public void Run(string xmlText)
        {
            CDFG graph = XmlParser.Parse(xmlText);

            DFG<Block> runningGraph = graph.StartDFG;
            Board board = new Board(10, 10);
            ModuleLibrary library = new ModuleLibrary();
            Dictionary<string, BoardFluid> dropPositions = new Dictionary<string, BoardFluid>();
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Stack<List<string>> varScopeStack = new Stack<List<string>>();
            Stack<Conditional> controlStack = new Stack<Conditional>();
            Stack<int> repeatStack = new Stack<int>();
            int loopCounter = 0;


            while (runningGraph == null)
            {
                List<Block> scheduledOperations = MakeSchedule(runningGraph, ref board, library, ref dropPositions);
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

                runningGraph = GetNextGraph(ref loopCounter, graph, runningGraph, variables, varScopeStack, controlStack);
            }
        }

        private List<Block> MakeSchedule(DFG<Block> runningGraph, ref Board board, ModuleLibrary library, ref Dictionary<string, BoardFluid> dropPositions)
        {
            Assay assay = new Assay(runningGraph);
            Schedule scheduler = new Schedule();
            scheduler.TransferFluidVariableLocationInformation(dropPositions);
            scheduler.ListScheduling(assay, board, library);

            board = scheduler.boardAtDifferentTimes.MaxBy(x => x.Key).Value;
            dropPositions = scheduler.FluidVariableLocations;

            return scheduler.ScheduledOperations;
        }

        private void ExecuteCommands(List<Command> commands)
        {
            commands.ForEach(x => Executor.SendCommand(x));
        }

        private DFG<Block> GetNextGraph(ref int loopCounter, CDFG graph, DFG<Block> currentDFG, Dictionary<string, float> variables, Stack<List<string>> varScopeStack, Stack<Conditional> controlStack, Stack<int> repeatStack)
        {
            IControlBlock control = graph.Nodes.Single(x => x.dfg == currentDFG).control;
            if (control is If ifControl)
            {
                foreach (Conditional conditional in ifControl.IfStatements)
                {
                    //if result is 1 then take the if block
                    if (1f == conditional.DecidingBlock.Run(variables, Executor))
                    {

                    }
                }
            }
            else if (control is Repeat repeatControl)
            {
                loopCounter = (int)repeatControl.Cond.DecidingBlock.Run(variables, Executor);
                controlStack.Push(repeatControl.Cond);
                varScopeStack.Push(new List<string>());
                repeatStack.
                return repeatControl.Cond.GuardedDFG;
            }

            //if inside repeat block and still
            //need to repeat
            if (loopCounter > 0)
            {
                loopCounter--;
                return currentDFG;
            }
        }
    }
}
