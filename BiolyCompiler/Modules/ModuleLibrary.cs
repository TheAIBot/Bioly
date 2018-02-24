using System;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Modules;
using BiolyCompiler.Graphs;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Scheduling;


namespace BiolyCompiler.Modules
{
    public class ModuleLibrary
    {
        List<Module> allocatedModules; 
        //For now it simply contains the modules that have been allocated. This will be changed later.

        //Orders the modules after their operation times.
        public void sortLibrary(){
            allocatedModules.Sort((x,y) => (x.operationTime < y.operationTime)? 0: 1);
        }

        public Module GetFirstPlaceableModule(Block operation, Architechture archetichture){
            for (int i = 0; i < allocatedModules.Count; i++)
            {
                Module module = allocatedModules[i];
                if(allocatedModules[i].getOperationType() == operation.getOperationType() && 
                   archetichture.canBePlaced(module))
                {
                    return module;
                }
            }
            throw new Exception("No module can execute the operation and also be placed on the board");
        }

        public void allocateModules(Assay assay){
            //It needs to find which modules are included in the assay.
            HashSet<OperationType> operationsUsed = new HashSet<OperationType>();
            assay.nodes.foreach(x => operationsUsed.add(x.getOperationType()));
            
            foreach (var operation in operationsUsed)
            {
                switch(operation){
                    case OperationTypes.Mixer:
                        allocatedModules.add(new Mixer(4,4,2000));
                        break;
                    case OperationTypes.Sensor:
                        allocatedModules.add(new Sensor());
                        break;
                    default:
                        throw new Exception("Operations of type " + operation.toString() + " not handled in the allocation phase");
                        break;
                }
            }

        }

        public Module getAndPlaceFirstPlaceableModule(Block operation, Architechture architechture){
            return null;
        }
    }
}
