using System;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Modules;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Scheduling;


namespace BiolyCompiler.Modules
{
    public class ModuleLibrary
    {
        public List<Module> allocatedModules = new List<Module>(); 
        //For now it simply contains the modules that have been allocated. This will be changed later.

        public ModuleLibrary(){

        }

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
            operationsUsed.UnionWith(assay.dfg.nodes.Select(node => node.value.getOperationType()));
            
            foreach (var operation in operationsUsed)
            {
                //Can be implemented as part of the different classes later.
                switch(operation){
                    case OperationType.Mixer:
                        allocatedModules.Add(new MixerModule(4,4,2000));
                        break;
                    case OperationType.Sensor:
                        allocatedModules.Add(new SensorModule());
                        break;
                    default:
                        throw new Exception("Operations of type " + operation.ToString() + " not handled in the allocation phase");
                        break;
                }
            }

        }

        public Module getOptimalModule(Block operation)
        {
            Module module = null;
            for (int i = 0; i < allocatedModules.Count; i++)
            {
                //The modules are sorted after speed,
                //so it will chose the fastest module that can execute the operation.
                if (allocatedModules[i].getOperationType() == operation.getOperationType())
                {
                    module = allocatedModules[i];
                    break;
                }
            }

            if (module == null)
            {
                throw new Exception("No allocated modules implements operations of type \" " + operation.getOperationType().ToString() + "\"");
            }

            return module;
        }

        public Module getAndPlaceFirstPlaceableModule(Block operation, Board board){
            Module module = getOptimalModule(operation);
            if (module == null) return null;

            bool canBePlaced = board.place(module);
            if(!canBePlaced) throw new Exception("Module can't be placed");
            return module;
        }
    }
}
