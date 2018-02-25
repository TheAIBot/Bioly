using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts.Blocks;
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Schedule
    {
        Dictionary<int, Module[][]> boardAtDifferentTimes = new Dictionary<int, Module[][]>();

        public Schedule(){

        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public static Tuple<int, Schedule> ListScheduling(Assay assay, Architechture architecture, ModuleLibrary library){
            
            //Place the modules that are fixed on the board, 
            //so that the dynamic algorithm doesn't have to handle this.
            //PlaceFixedModules(); //TODO implement.

            Board board = new Board(architecture.getBoardHeigth, architecture.getBoardWidth); //The board is initially empty

            //Many optimization can be made to this method (later)
            Schedule schedule = new Schedule(); //Initially empty
            assay.calculateCriticalPath();
            library.allocateModules(assay); 
            library.sortLibrary();
            List<Block> readyOperations = assay.getReadyOperations();
            while(readyOperations.Count > 0){
                Block operation = removeOperation(readyOperations);
                Module module   = library.getAndPlaceFirstPlaceableModule(operation, board); //Also called place
                //If the module can't be placed, one must wait until there is enough place for it.
                int timeStart;
                if (module == null) {}
                    timeStart = waitForAFinishedOperation();
                } else{
                    //TODO What if there is no module that can be placed?(*)
                    operation.Bind(module); //TODO make modules unique
                    //Route route   = determineRouteToModule(operation, module, architecture); //Will be included as part of a later step.
                    timeStart = updateSchedule(operation, schedule, route, currentPlacedModules); 
                }

                board = getCurrentBoard();
                updateReadyOperations(assay, timeStart, readyOperations);
                readyOperations = assay.getReadyOperations();
            }
            return Tuple.Create(schedule.getCompletionTime(), schedule);
        }


        public int getCompletionTime(){
            return 0;
        }

        public static void updateReadyOperations(Assay assay, int newTime, List<Block> readyOperations){
            
        }

        public static Route determineRouteToModule(Block operation, Module module, Architechture architecture){
            return null;
        }

        public static int updateSchedule(Block operation, Schedule schedule, Route route){
            return 0;
        }

        public static Block removeOperation(List<Block> readyOperations){
            Block operation = readyOperations[0];
            readyOperations.RemoveAt(0);
            return operation;
        }



    }


    public class Route{

    }
}
