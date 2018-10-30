using System;
using System.Linq;
using BiolyCompiler.Graphs;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts;
using Priority_Queue;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using MoreLinq;
using System.Collections;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler.Scheduling
{

    public class Assay : IEnumerable<Block>
    {
        public readonly DFG<Block> Dfg;
        private readonly Dictionary<Block, Node<Block>> OperationToNode = new Dictionary<Block, Node<Block>>();
        private readonly SimplePriorityQueue<Block, int> ReadyOperations = new SimplePriorityQueue<Block, int>();
        //For each static module, it contains a priority queue of the ready operations associated with the module,
        //and a bool of whether or not ReadyOperations contains one of those operations, or if the heater is in use: 
        //only one operation must be there at a time, and only when the heater is not in use.
        private Dictionary<string, (bool, SimplePriorityQueue<Block, int>)> StaticModuleOperations = new Dictionary<string, (bool, SimplePriorityQueue<Block, int>)>();

        public Assay(DFG<Block> dfg)
        {
            this.Dfg = dfg;
            CalculateCriticalPath(); //Giving the nodes the correct priority.
            //Set ready nodes
            foreach (Node<Block> node in dfg.Nodes)
            {
                if (node.value is VariableBlock varBlock)
                {
                    if (varBlock.CanBeScheduled)
                    {
                        OperationToNode.Add(node.value, node);
                    }
                }
                else
                {
                    OperationToNode.Add(node.value, node);
                }

                if (node.value is HeaterUsage heaterOperation)
                {
                    if (!StaticModuleOperations.ContainsKey(heaterOperation.ModuleName))
                    {
                        StaticModuleOperations.Add(heaterOperation.ModuleName, (false, new SimplePriorityQueue<Block, int>()));
                    }
                }
            }
            List<Block> initialReadyOperations = dfg.Input.Select(x => x.value).ToList();

            HashSet<string> usedHeaterModules = new HashSet<string>();
            foreach (Block operation in initialReadyOperations)
            {
                if (operation is HeaterUsage heaterOperation)
                {
                    usedHeaterModules.Add(heaterOperation.ModuleName);
                    StaticModuleOperations[heaterOperation.ModuleName].Item2.Enqueue(heaterOperation, heaterOperation.priority);
                }
                else ReadyOperations.Enqueue(operation, operation.priority);
            }

            //Only one operation for each static module must be in ReadyOperations at the same time:
            //This is to get log n time search time.
            foreach (var heaterModule in usedHeaterModules)
            {
                var pair = StaticModuleOperations[heaterModule];
                var priorityQueue = pair.Item2;
                var topPriorityOperation = priorityQueue.Dequeue();
                ReadyOperations.Enqueue(topPriorityOperation, topPriorityOperation.priority);
                StaticModuleOperations[heaterModule] = (true, priorityQueue);
            }
        }

        private void CalculateCriticalPath()
        {
            List<Node<Block>> rank = Dfg.Output;

            do
            {
                foreach (Node<Block> node in rank)
                {
                    foreach (Node<Block> backNode in node.GetIngoingEdges())
                    {
                        int newPriority = node.value.priority;
                        switch (backNode.value)
                        {
                            case HeaterUsage block:
                                newPriority -= block.Time;
                                break;
                            case VariableBlock block1:
                            case Union block2:
                            case StaticDeclarationBlock block3:
                            case Fluid block4:
                            case SetArrayFluid block5:
                                break;
                            case Mixer block:
                                newPriority -= Mixer.OPERATION_TIME;
                                break;
                            default:
                                throw new InternalRuntimeException($"Calculating critical path doesn't handle the block type {backNode.GetType().ToString()}.");
                        }

                        backNode.value.priority = Math.Min(backNode.value.priority, newPriority);
                    }
                }

                rank = rank.SelectMany(x => x.GetIngoingEdges())
                           .Distinct()
                           .ToList();
            } while (rank.Count > 0);

            foreach (Node<Block> node in Dfg.Nodes)
            {
                if (node.value is WasteUsage)
                {
                    node.value.priority = int.MinValue;
                }
                if (node.value is StaticDeclarationBlock)
                {
                    node.value.priority = int.MaxValue;
                }
            }
        }

        public void UpdateReadyOperations(Block operation)
        {
            //If it has already been registred as finished, then ignore the operation:
            if (operation.IsDone) return;
            operation.IsDone = true;
            if (ReadyOperations.Contains(operation))
                ReadyOperations.Remove(operation);

            OperationToNode.TryGetValue(operation, out Node<Block> operationNode);
            if (operationNode != null)
            {
                HashSet<string> usedHeaterModules = new HashSet<string>();
                foreach (var successorOperationNode in operationNode.GetOutgoingEdges().Distinct())
                {
                    if (successorOperationNode.GetIngoingEdges().All(node => node.value.IsDone || (node.value is VariableBlock && !((VariableBlock)node.value).CanBeScheduled)))
                    {
                        if (successorOperationNode.value is HeaterUsage heaterOperation)
                        {
                            usedHeaterModules.Add(heaterOperation.ModuleName);
                            StaticModuleOperations[heaterOperation.ModuleName].Item2.Enqueue(heaterOperation, heaterOperation.priority);
                        }
                        else
                        {
                            if (successorOperationNode.value is VariableBlock varBlock && !varBlock.CanBeScheduled)
                            {
                                continue;
                            }
                            ReadyOperations.Enqueue(successorOperationNode.value, successorOperationNode.value.priority);
                        }
                        //This will not happen multiple times, as once an operation list has been added to the readyOperaition list,
                        //all operations it depends on has already been scheduled, and as such they have been removed from readyOperaition.
                    }
                }

                if (operation is HeaterUsage heaterUsage)
                {
                    //The heater is not used anymore, so a new heater operation can be added to the ready opeartions:
                    var priorityQueue = StaticModuleOperations[heaterUsage.ModuleName].Item2;
                    StaticModuleOperations[heaterUsage.ModuleName] = (false, priorityQueue);
                    usedHeaterModules.Add(heaterUsage.ModuleName);
                }

                foreach (var heater in usedHeaterModules)
                {
                    var pair = StaticModuleOperations[heater];
                    //No heater operation associated with this heater is in ReadyOperation,
                    //nor is the heater currently running. Also at least one operation with the module exist:
                    if (!pair.Item1 && pair.Item2.Count > 0)
                    {
                        var priorityQueue = pair.Item2;
                        HeaterUsage topPriorityOperation = (HeaterUsage)priorityQueue.Dequeue();
                        ReadyOperations.Enqueue(topPriorityOperation, topPriorityOperation.priority);
                        StaticModuleOperations[heater] = (true, priorityQueue);
                    }
                }
            }
        }

        public bool IsEmpty()
        {
            return ReadyOperations.Count == 0;
        }

        public SimplePriorityQueue<Block, int> GetReadyOperations()
        {
            return ReadyOperations;
        }

        public IEnumerator<Block> GetEnumerator()
        {
            while (ReadyOperations.Count > 0)
            {
                yield return ReadyOperations.Dequeue();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}