using System;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Scheduling;
using BiolyCompiler.BlocklyParts;

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
            HashSet<Type> operationTypesUsed = new HashSet<Type>();
            foreach (var node in assay.dfg.Nodes)
            {
                Block operation = node.value;
                if (!operationTypesUsed.Contains(operation.GetType()))
                {
                    operationTypesUsed.Add(operation.GetType());
                    allocatedModules.Add(operation.getAssociatedModule());
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
            Module module = getOptimalModule(operation).GetCopyOf();
            if (module == null) return null;

            bool canBePlaced = board.FastTemplatePlace(module);
            if(!canBePlaced) throw new Exception("Module can't be placed");
            return module;
        }
    }
}