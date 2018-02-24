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
        DFG<Block> dfg;

        public Assay(DFG<Block> dfg){
            this.dfg = dfg;
        }


        public void calculateCriticalPath(){
            
        }


        public List<Block> getReadyOperations(){
            return null;
        }

    }
}
