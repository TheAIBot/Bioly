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

namespace BiolyCompiler.Scheduling
{

    public class Assay
    {
        public DFG<Block> Dfg;
        private Dictionary<Block, Node<Block>> OperationToNode = new Dictionary<Block, Node<Block>>();
        public SimplePriorityQueue<Block, int> ReadyOperations = new SimplePriorityQueue<Block, int>();
        public Dictionary<string, Module> StaticModules { get; private set; }
        //For each static module, it contains a priority queue of the ready operations associated with the module,
        //and a bool of whether or not ReadyOperations contains one of those operations, or if the heater is in use: 
        //only one operation must be there at a time, and only when the heater is not in use.
        public Dictionary<string, (bool, SimplePriorityQueue<Block, int>)> StaticModuleOperations = new Dictionary<string, (bool, SimplePriorityQueue<Block, int>)>();

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
                nodeToIndex.Add(topologicalSorting[i], i);


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
                else if (!Schedule.IsSpecialCaseOperation(currentNode.value) && !(currentNode.value is StaticUseageBlock))
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
            Dfg.Nodes.Select(node => node.value).Where(operation => operation is WasteUsage).ForEach(wasteUsage => wasteUsage.priority = -100000000);
            //The dfg is inverted back to normal:
            Dfg.InvertEdges();

        }

        private List<Node<Block>> GetTopologicalSortedDFG(DFG<Block> dfg)
        {
            //Can be done in O(n+m) time. A simpler algorithm will however be used here.
            //No real reason to use a stack except for the easy .pop method. Could be a list.
            Stack<Node<Block>> nodesWithDegree0 = new Stack<Node<Block>>();
            List<(Node<Block>, int)> nodesAndDegree = new List<(Node<Block>, int)>();
            List<Node<Block>> topologicalSorting = new List<Node<Block>>();
            //Works on basis of the pointers, which is what is desired:
            Dictionary<Node<Block>, int> nodeToIndex = new Dictionary<Node<Block>, int>();


            foreach (var node in dfg.Nodes)
            {
                nodeToIndex.Add(node, nodesAndDegree.Count());
                nodesAndDegree.Add((node, node.GetIngoingEdges().Count()));
                if (node.GetIngoingEdges().Count() == 0)
                {
                    nodesWithDegree0.Push(node);
                }
            }

            //One by one adding the nodes to the topological sorting,
            //by one by one removing the vertices with degree 0.
            while (nodesWithDegree0.Count() > 0)
            {
                var currentNode = nodesWithDegree0.Pop();
                topologicalSorting.Add(currentNode);
                foreach (var node in currentNode.getOutgoingEdges())
                {
                    //Decrease the ingoing edge count for the nodes that currenNode points to:
                    int indexOfNode = nodeToIndex[node];
                    (_, int ingoingEdgeCount) = nodesAndDegree[indexOfNode];
                    nodesAndDegree[indexOfNode] = (node, ingoingEdgeCount - 1);
                    if (ingoingEdgeCount - 1 == 0)
                    {
                        nodesWithDegree0.Push(node);
                    }
                }

            }

            //Checking for errors in the code:
            if (topologicalSorting.Count != Dfg.Nodes.Count) throw new Exception("Logic error: not all nodes are included in the topological sorting. Expected " + Dfg.Nodes.Count + " and has" + topologicalSorting.Count());
            if (!nodesAndDegree.All(pair => pair.Item2 == 0)) throw new Exception("Logic error: some nodes do not have degree 0 when adding them to the topological sorting");


            return topologicalSorting;
        }

        public SimplePriorityQueue<Block, int> GetReadyOperations()
        {
            return ReadyOperations;
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

        public bool hasUnfinishedOperations()
        {
            return !OperationToNode.All(node => node.Key.IsDone || (node.Key is VariableBlock && !((VariableBlock)node.Key).CanBeScheduled));
        }

        public void SetStaticModules(Dictionary<string, Module> staticModules)
        {
            this.StaticModules = staticModules;
        }
    }
}