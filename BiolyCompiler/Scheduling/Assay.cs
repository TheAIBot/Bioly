using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts.Blocks;
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Assay
    {
        public DFG<Block> dfg;

        public Assay(DFG<Block> dfg){
            this.dfg = dfg;
            //Set ready nodes:
            dfg.nodes.Where(node => node.)
        }

        public void calculateCriticalPath(){
            
        }
        

        public List<Block> getReadyOperations(){
            List<Block> readyOperations = new List<Block>();
            dfg.nodes.Where(node => node.value.isReady())
                     .ForEach(operation => readyOperations.add(operation));
            return readyOperations;
        }

    }
}
