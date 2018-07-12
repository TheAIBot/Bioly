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

namespace BiolyCompiler.Scheduling
{

    public class Assay : IEnumerable<Block>
    {
        public DFG<Block> Dfg;
        private Dictionary<Block, Node<Block>> OperationToNode = new Dictionary<Block, Node<Block>>();
        private SimplePriorityQueue<Block, int> ReadyOperations = new SimplePriorityQueue<Block, int>();
        private Dictionary<string, Module> StaticModules;
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
            //Inverting is necessary for the correct calculations:
            Dfg.InvertEdges();
            //Its a DAG, so a topological sorting exists:
            List<Node<Block>> topologicalSorting = GetTopologicalSortedDFG(Dfg);
            int[] lenghtOfLongestPathToNode = new int[topologicalSorting.Count];
            //The dictionary works on basis of the pointers, which is what is desired:
            Dictionary<Node<Block>, int> nodeToIndex = new Dictionary<Node<Block>, int>();

            //Below the lenght of the longest paths are calculated, in the inverted dfg:

            for (int i = 0; i < topologicalSorting.Count; i++)
            {
                nodeToIndex.Add(topologicalSorting[i], i);
            }


            for (int i = 0; i < topologicalSorting.Count; i++)
            {
                var currentNode = topologicalSorting[i];
                int currentNodeExecutionTime;

                if (currentNode.value is HeaterUsage heaterUsage)
                {
                    currentNodeExecutionTime = heaterUsage.Time; //Heaters have an extra time variable.
                }
                else if (currentNode.value is WasteUsage)
                {
                    currentNodeExecutionTime = 2; //100.000.000
                }
                else if (!(currentNode.value is VariableBlock ||
                           currentNode.value is Union ||
                           currentNode.value is StaticDeclarationBlock ||
                           currentNode.value is Fluid ||
                           currentNode.value is SetArrayFluid) && !(currentNode.value is StaticUseageBlock))
                {
                    currentNodeExecutionTime = ((FluidBlock)currentNode.value).getAssociatedModule().OperationTime; //Operation involving a module with an execution time.
                }
                else
                {
                    currentNodeExecutionTime = 0; //Special case operations does not have any inherent execution time
                }

                //A min priority queue is used, so the priority is inverted.
                currentNode.value.priority = -(lenghtOfLongestPathToNode[i] + currentNodeExecutionTime);
                foreach (var node in currentNode.getOutgoingEdges())
                {
                    //Update the lenght of the paths:
                    int indexOfNode = nodeToIndex[node];
                    int currentLength = lenghtOfLongestPathToNode[indexOfNode];
                    if (currentLength < lenghtOfLongestPathToNode[i] + currentNodeExecutionTime)
                    {
                        lenghtOfLongestPathToNode[indexOfNode] = lenghtOfLongestPathToNode[i] + currentNodeExecutionTime;
                    }
                }
            }
            Dfg.Nodes.Select(node => node.value)
                     .Where(operation => operation is WasteUsage)
                     .ForEach(wasteUsage => wasteUsage.priority = -100000000);
            //The dfg is inverted back to normal:
            Dfg.InvertEdges();

        }

        private List<Node<Block>> GetTopologicalSortedDFG(DFG<Block> dfg)
        {
            List<Node<Block>> topologicalSorted = new List<Node<Block>>();
            HashSet<Node<Block>> alreadyAdded = new HashSet<Node<Block>>();
            List<Node<Block>> rank = dfg.Input;

            do
            {
                topologicalSorted.AddRange(rank);
                rank.ForEach(x => alreadyAdded.Add(x));

                rank = rank.SelectMany(x => x.getOutgoingEdges())
                           .Distinct()
                           .Except(alreadyAdded)
                           .ToList();
            } while (rank.Count > 0);

            return topologicalSorted;
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
                foreach (var successorOperationNode in operationNode.getOutgoingEdges())
                {
                    if (successorOperationNode.GetIngoingEdges().All(node => node.value.IsDone || (node.value is VariableBlock && !((VariableBlock)node.value).CanBeScheduled)))
                    {
                        if (successorOperationNode.value is HeaterUsage heaterOperation)
                        {
                            usedHeaterModules.Add(heaterOperation.ModuleName);
                            StaticModuleOperations[heaterOperation.ModuleName].Item2.Enqueue(heaterOperation, heaterOperation.priority);
                        }
                        else ReadyOperations.Enqueue(successorOperationNode.value, successorOperationNode.value.priority);
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

        public void SetStaticModules(Dictionary<string, Module> staticModules)
        {
            this.StaticModules = staticModules;
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