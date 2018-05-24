using System;
using System.Linq;
using BiolyCompiler.Graphs;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts;

namespace BiolyCompiler.Scheduling
{

    public class Assay
    {
        public DFG<Block> dfg;
        private Dictionary<Block, Node<Block>> operationToNode = new Dictionary<Block, Node<Block>>();
        public List<Block> readyOperations;

        public Assay(DFG<Block> dfg){
            this.dfg = dfg;
            //Set ready nodes
            foreach (Node<Block> node in dfg.Nodes)
            {
                if (node.value is VariableBlock varBlock)
                {
                    if (varBlock.CanBeScheduled)
                    {
                        operationToNode.Add(node.value, node);
                    }
                }
                else
                {
                    operationToNode.Add(node.value, node);
                }
            }
            readyOperations = dfg.Input.Select(x => x.value).ToList();
        }

        public void calculateCriticalPath(){
            
        }
        
        public List<Block> getReadyOperations(){
            return readyOperations;
        }
        
        public void updateReadyOperations(Block operation)
        {
            operation.isFinished = true;
            readyOperations.Remove(operation);

            operationToNode.TryGetValue(operation, out Node<Block> operationNode);

            foreach (var successorOperationNode in operationNode.getOutgoingEdges())
            {
                if (successorOperationNode.getIngoingEdges().All(node => node.value.isFinished || (node.value is VariableBlock && !((VariableBlock)node.value).CanBeScheduled)))
                {
                    readyOperations.Add(successorOperationNode.value);
                    //This will not happen multiple times, as once an operation list has been added to the readyOperaition list,
                    //all operations it depends on has already been scheduled, and as such they have been removed from readyOperaition.
                }
            }
        }

        public bool hasUnfinishedOperations()
        {
            return !operationToNode.All(node => node.Key.isFinished || (node.Key is VariableBlock && !((VariableBlock)node.Key).CanBeScheduled));
        }
    }
}
