using System;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Scheduling;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.Exceptions;

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
            allocatedModules.Sort((x,y) => (x.OperationTime < y.OperationTime)? 0: 1);
        }

        public void allocateModules(Assay assay){
            //It needs to find which modules are included in the assay.
            HashSet<Module> associatedModules = new HashSet<Module>();
            foreach (var node in assay.Dfg.Nodes)
            {
                if (  node.value is FluidBlock operation && 
                    !(node.value is StaticUseageBlock) &&
                    !(node.value is Fluid) &&
                    !(node.value is SetArrayFluid) &&
                    !(node.value is Union) &&
                    !associatedModules.Contains(operation.getAssociatedModule()))
                {
                    associatedModules.Add(operation.getAssociatedModule());
                    allocatedModules.Add(operation.getAssociatedModule());
                }
            }
        }

        public Module getOptimalModule(FluidBlock operation)
        {
            return operation.getAssociatedModule();
            //Currently unused. Will become important for larger programs, 
            //where there are multiple different modules that can execute an operation:
            //Also if there is only a certain amount of different modules available.

            /*
            Module module = null;
            for (int i = 0; i < allocatedModules.Count; i++)
            {
                //The modules are sorted after speed,
                //so it will chose the fastest module that can execute the operation.
                if (allocatedModules[i].Equals(operation.getAssociatedModule()))
                {
                    module = allocatedModules[i];
                    break;
                }
            }

            if (module == null)
            {
                throw new InternalRuntimeException("No allocated modules implements operations of type \" " + operation.ToString() + "\"");
            }

            return module;
            */
        }

        public Module getAndPlaceFirstPlaceableModule(FluidBlock operation, Board board){
            Module optimalModuleTemplate = getOptimalModule(operation);
            if (optimalModuleTemplate.GetInputLayout() == null) throw new InternalRuntimeException("The layout of the module have never been set. The module is: " + optimalModuleTemplate.ToString());
            Module module = optimalModuleTemplate.GetCopyOf();
            if (module == null) return null;
            
            bool canBePlaced = board.FastTemplatePlace(module);
            if(!canBePlaced) throw new RuntimeException("Module \"" + module.ToString() +  "\" can't be placed");
            //Now that the module has been placed, the internal rectangles in the module layout can be modified, such that they are placed correctly.
            module.RepositionLayout();
            return module;
        }
    }
}
