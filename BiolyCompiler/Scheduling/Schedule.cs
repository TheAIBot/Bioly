using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Architechture;
using BiolyCompiler.BlocklyParts.Blocks;
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Schedule
    {

        public static void main(string[] args){
            ListScheduling(null,null,null);
        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public (int, Schedule) ListScheduling(DFG<Block> assay, Architechture architecture, ModuleLibrary library){
            
            //Place the modules that are fixed on the board, 
            //so that the dynamic algorithm doesn't have to handle this.
            //PlaceFixedModules(); //TODO implement.

            
            Schedule schedule; //Initially empty
            assay.calculateCriticalPath();
            library.allocateModules(assay); 
            library.SortLibrary();
            List list = assay.getReadyOperations();
            while(list.size() > 0){
                Operation operation = removeOperation(list);
                Module module       = library.getAndPlaceFirstPlaceableModule(operation, architecture); //Also called place
                //TODO What if there is no module that can be placed?(*)
                Bind(module, operation);
                route   = determineRouteToModule(operation, module, architecture);
                timeStart = updateSchedule(operation, schedule, route); 
                updateReadyOperations(assay, newTime, list);
                list = getReadyOperations(assay);
            }
            return (completionTime, schedule);
        }

    }
}
