using System;
using System.Linq;
using BiolyCompiler.Graphs;
using System.Collections.Generic;
using BiolyCompiler.BlocklyParts.Blocks;
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Assay
    {
        public DFG<Block> dfg;
        Dictionary<Block, Node<Block>> operationToNode = new Dictionary<Block, Node<Block>>();
        public List<Block> readyOperations;

        public Assay(DFG<Block> dfg){
            this.dfg = dfg;
            //Set ready nodes
            dfg.nodes.ForEach(node => operationToNode.Add(node.value, node));
            readyOperations = dfg.nodes.Where(node => node.getIngoingEdges().Count == 0)
                                       .Select(node => node.value)
                                       .ToList();
        }

        public void calculateCriticalPath(){
            
        }
        

        public List<Block> getReadyOperations(){
            return readyOperations;
        }
        
        internal void updateReadyOperations(Block operation)
        {
            operation.hasBeenScheduled = true;
            readyOperations.Remove(operation); //TODO(*) make equals method for blocks.
                                               //An operation has been finished: there might be operation that can now be executed.

            Node<Block> operationNode;
            operationToNode.TryGetValue(operation, out operationNode);

            foreach (var successorOperationNode in operationNode.getOutgoingEdges())
            {

                if (!successorOperationNode.getIngoingEdges().Any(node => node.value.hasBeenScheduled == false))
                {
                    readyOperations.Add(successorOperationNode.value);
                    //This will not happen multiple times, as once an operation list has been added to the readyOperaition list,
                    //all operations it depends on has already been scheduled, and as such they have been removed from readyOperaition.
                }
            }
            operationNode.getOutgoingEdges();
        }
    }
}
