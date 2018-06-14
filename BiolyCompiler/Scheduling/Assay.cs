using System;
using System.Linq;
using BiolyCompiler.Graphs;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts;
using Priority_Queue;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.FFUs;

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

        public Assay(DFG<Block> dfg){
            this.Dfg = dfg;
            calculateCriticalPath(); //Giving the nodes the correct priority.
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
                } else ReadyOperations.Enqueue(operation, operation.priority);
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

        public void calculateCriticalPath(){
            
        }
        
        public SimplePriorityQueue<Block, int> GetReadyOperations(){
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

            HashSet<string> usedHeaterModules = new HashSet<string>();
            foreach (var successorOperationNode in operationNode.getOutgoingEdges())
            {
                if (successorOperationNode.getIngoingEdges().All(node => node.value.IsDone || (node.value is VariableBlock && !((VariableBlock)node.value).CanBeScheduled)))
                {
                    if (successorOperationNode.value is HeaterUsage heaterOperation) {
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
