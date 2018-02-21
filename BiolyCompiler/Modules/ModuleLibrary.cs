using System;
using BiolyCompiler.Modules;
using System.Collections.Generic;


namespace BiolyCompiler.Modules
{
    public class ModuleLibrary
    {
        List<Module> allocatedModules; 
        //For now it simply contains the modules that have been allocated. This will be changed later.

        public void sortLibrary(){
            //Orders the modules after their operation times.
            allocatedModules.orderBy(x => x.operationTime);
        }

        public Module GetFirstPlaceableModule(Operation operation, Archetichture archetichture){
            for (int i = 0; i < allocatedModules.length; i++)
            {
                Module module = allocatedModules[i];
                if(allocatedModules[i].canExecute(operation) && 
                   archetichture.canBePlaced(module))
                {
                    return module;
                }
            }
            throw new Error("No module can execute the operation and be placed on the board");
        }
    }
}
