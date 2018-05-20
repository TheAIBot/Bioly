using System;
using BiolyCompiler.Graphs;
using MoreLinq;
using BiolyCompiler.Modules;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Routing;
using Priority_Queue;
using BiolyCompiler.BlocklyParts;
using System.Linq;
using System.Diagnostics;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.FFUs;

namespace BiolyCompiler.Scheduling
{
    public class Schedule
    {
        //Records how the board looks at all the times where the board have been changed. 
        //This is primarily for testing and visulization purposes.
        public Dictionary<int, Board> boardAtDifferentTimes = new Dictionary<int, Board>();
        // For debuging. Used when printing the board to the console, for visulization purposes.
        public List<Module> AllUsedModules = new List<Module>(); 
        public Dictionary<string, BoardFluid> FluidVariableLocations = new Dictionary<string, BoardFluid>();
        public Dictionary<string, Module> StaticModules = new Dictionary<string, Module>();
        public SimplePriorityQueue<FluidBlock> CurrentlyRunningOpertions = new SimplePriorityQueue<FluidBlock>();
        public List<Block> ScheduledOperations = new List<Block>();
        public const int DROP_MOVEMENT_TIME = 1; //How many time units it takes for a droplet to move from one electrode to the next.
        public const int IGNORED_TIME_DIFFERENCE = 100; 

        public Schedule(){

        }
        
        private static Board getCurrentBoard()
        {
            throw new NotImplementedException();
        }

        private void updateSchedule(Block operation, int currentTime, int startTime)
        {
            ScheduledOperations.Add(operation);
            operation.StartTime = startTime;
            if (operation is VariableBlock || operation is Fluid) return;
            else
            {
                FluidBlock fluidOperation = operation as FluidBlock;
                int moduleRunningTime = fluidOperation.GetRunningTime();
                fluidOperation.endTime = operation.StartTime + moduleRunningTime;// currentTime + fluidOperation.boundModule.OperationTime;
                CurrentlyRunningOpertions.Enqueue(fluidOperation, operation.endTime);
            }
        }
        
        public void TransferFluidVariableLocationInformation(Dictionary<string, BoardFluid> fluidLocationInformation)
        {
            fluidLocationInformation.ForEach(pair => FluidVariableLocations.Add(pair.Key, pair.Value));
        }

        public void TransferStaticModulesInformation(Dictionary<string, Module> staticModulesInformation)
        {
            staticModulesInformation.ForEach(pair => StaticModules.Add(pair.Key, pair.Value));
        }

        public void PlaceStaticModules(List<StaticDeclarationBlock> staticDeclarations, Board board, ModuleLibrary library)
        {
            foreach (var staticDeclaration in staticDeclarations)
            {
                if (staticDeclaration is InputDeclaration input)
                {
                    FluidVariableLocations.TryGetValue(input.OriginalOutputVariable, out BoardFluid fluidType);
                    if (fluidType == null)
                    {
                        fluidType = new BoardFluid(input.OriginalOutputVariable);
                        FluidVariableLocations.Add(input.OriginalOutputVariable, fluidType);
                    }
                    InputModule inputModule = new InputModule(fluidType, (int)input.Amount);
                    bool couldBePlaced = board.FastTemplatePlace(inputModule);
                    if (!couldBePlaced) throw new Exception("The input module couldn't be placed. The module is: " + inputModule.ToString());
                    input.BoundModule = inputModule;
                    inputModule.RepositionLayout();
                } else {
                    Module staticModule = library.getAndPlaceFirstPlaceableModule(staticDeclaration, board);
                    StaticModules.Add(staticDeclaration.ModuleName, staticModule);
                }                
            }
        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public int ListScheduling(Assay assay, Board board, ModuleLibrary library)
        {
            //Setup:
            int currentTime = 0;
            board = ListSchedulingSetup(assay, board, library, currentTime);
            List<Block> readyOperations = assay.getReadyOperations();

            //Continue until all operations have been scheduled:
            while (assay.hasUnfinishedOperations() && canExecuteMoreOperations(readyOperations))
            {
                Block nextOperation = removeOperation(readyOperations);
                if (isSpecialCaseOperation(nextOperation))
                {
                    (readyOperations, currentTime) = handleSpecialCases(assay, board, currentTime, nextOperation, readyOperations);
                }
                else if (nextOperation is FluidBlock topPriorityOperation)
                {
                    //If nextOperation is associated with a static module, 
                    //this needs to be chosen as the module to execute the operation.
                    //Else a module that can execute the operation needs to be found and placed on the board:
                    Module operationExecutingModule = (topPriorityOperation is StaticUseageBlock staticOperation) ?
                                                       StaticModules[staticOperation.ModuleName] :
                                                       library.getAndPlaceFirstPlaceableModule(topPriorityOperation, board);
                    topPriorityOperation.Bind(operationExecutingModule, FluidVariableLocations);

                    //For debuging:
                    AllUsedModules.Add(operationExecutingModule);
                    DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);

                    //If the module can't be placed, one must wait until there is enough space for it:
                    if (operationExecutingModule == null) throw new Exception("Not enough space for a module: this is not handeled yet");

                    //Now all the droplet that the module should operate on, needs to be delivered to it.
                    //By construction, there will be a route from the droplets to the module, 
                    //and so it will always be possible for this routing to be done:
                    currentTime = RouteDropletsToModuleAndUpdateSchedule(board, currentTime, topPriorityOperation, operationExecutingModule);
                    DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);

                    //When operations finishes, while the routing associated with nextOperation was performed, 
                    //this needs to be handled. Note that handleFinishingOperations will also wait for operations to finish, 
                    //in the case that there are no more operations that can be executed, before this happen:
                    (currentTime, board) = handleFinishingOperations(currentTime, assay, board);
                    readyOperations = assay.getReadyOperations();
                    DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                    nextOperation.hasBeenScheduled = true;
                }
                else
                {
                    throw new Exception("The given block/operation type is unhandeled by the scheduler. " +
                                         "The operation is: " + nextOperation.ToString());
                }
            }
            if (assay.hasUnfinishedOperations())
            {
                throw new Exception("There were operations that couldn't be scheduled.");
            }
            SortScheduledOperations();
            return getCompletionTime();
        }

        private void SortScheduledOperations()
        {
            ScheduledOperations.Sort((x, y) => (x.StartTime < y.StartTime || (x.StartTime == y.StartTime && x.endTime <= y.endTime)) ? 0 : 1);
        }

        private (List<Block>, int) handleFluidTransfers(Assay assay, Board board, int currentTime, Fluid nextOperation)
        {
            List<Block> readyOperations;
            int originalStartTime = currentTime;
            FluidInput input = nextOperation.InputVariables[0];
            int requiredDroplets = input.GetAmountInDroplets(FluidVariableLocations);
            //First it is checked if there is even any fluid that needs to be transfered:
            if(requiredDroplets == 0) {
                assay.updateReadyOperations(nextOperation);
                return (assay.getReadyOperations(), currentTime);
            }

            //Fluid needs to be allocated to a variable.
            //If the origin of the fluid is an input, a given amount of droplets needs to moved unto the board,
            //but if origin is simply droplets placed on the board, a simple renaiming can be done instead.
            //It will prioritize taking droplets already on the board, instead of taking droplets out of the inputs.
            
            //If there already exists droplets with the target fluid type (what the droplets should be renamed to),
            //then the droplets are renamed, are added to these droplets:
            BoardFluid targetFluidType;
            FluidVariableLocations.TryGetValue(nextOperation.OriginalOutputVariable, out targetFluidType);
            if (targetFluidType == null) {
                targetFluidType = new BoardFluid(nextOperation.OriginalOutputVariable);
                FluidVariableLocations.Add(nextOperation.OriginalOutputVariable, targetFluidType);
            }

            BoardFluid inputFluid;
            FluidVariableLocations.TryGetValue(input.OriginalFluidName, out inputFluid);
            if (inputFluid == null) throw new Exception("Fluid of type \"" + input.FluidName + "\" was to be transfered, but fluid of this type do not exist (or have ever been created).");
            
            //Trying to take as many droplets as possible from those already placed on the board,
            //to minimize the number of droplets that must be taken from the input modules:
            List<Droplet> availableDroplets = inputFluid.dropletSources.Where(dropletSource => dropletSource is Droplet)
                                                                       .Select(dropletSource => dropletSource as Droplet)
                                                                       .ToList();
            int numberOfDropletsToTransfer = Math.Min(availableDroplets.Count, requiredDroplets);
            for (int i = 0; i < numberOfDropletsToTransfer; i++) {
                availableDroplets[i].SetFluidType(targetFluidType);
            }
            int numberOfDropletsTransfered = numberOfDropletsToTransfer;
            if (numberOfDropletsTransfered != requiredDroplets)
            {
                //As there aren't enough droplets placed on the board, to satisfy the requirement, 
                //some must be taken from the input modules.
                List<InputModule> inputModules = inputFluid.dropletSources.Where(dropletSource => dropletSource is InputModule)
                                                                          .Select(dropletSource => dropletSource as InputModule)
                                                                          .ToList();
                List<Route> dropletRoutes = new List<Route>();
                nextOperation.InputRoutes.Add(input.FluidName, dropletRoutes);
                foreach (var inputModule in inputModules)
                {
                    while (inputModule.DropletCount > 0 && numberOfDropletsTransfered < requiredDroplets)
                    {
                        numberOfDropletsTransfered++;
                        inputModule.DecrementDropletCount();
                        Droplet droplet = new Droplet(targetFluidType);
                        AllUsedModules.Add(droplet);
                        board.FastTemplatePlace(droplet);
                        Route route = Router.RouteDropletToNewPosition(inputModule, droplet, board, currentTime);
                        currentTime = route.getEndTime() + 1;
                        dropletRoutes.Add(route);
                    }
                    if (numberOfDropletsTransfered == requiredDroplets) break;
                }
                if (numberOfDropletsTransfered != requiredDroplets) throw new Exception("Not enough droplets available.");
            }
            updateSchedule(nextOperation, currentTime, originalStartTime);
            currentTime += 2;
            DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
            assay.updateReadyOperations(nextOperation);
            readyOperations = assay.getReadyOperations();
            return (readyOperations, currentTime);
        }

        private static List<Block> handleStaticModuleDeclarations(Assay assay, Block nextOperation)
        {
            List<Block> readyOperations;
            assay.updateReadyOperations(nextOperation);
            readyOperations = assay.getReadyOperations();
            return readyOperations;
        }

        private List<Block> handleVariableOperation(Assay assay, int currentTime, Block nextOperation)
        {
            //This is a mathematical operation, and it should be scheduled to run as soon as possible
            if ((nextOperation as VariableBlock).CanBeScheduled)
            {
                //This is a mathematical operation, and it should be scheduled to run as soon as possible
                updateSchedule(nextOperation, currentTime, currentTime);
                assay.updateReadyOperations(nextOperation);
            }
            return assay.getReadyOperations();
        }

        private (List<Block>, int) handleSpecialCases(Assay assay, Board board, int currentTime, Block nextOperation, List<Block> readyOperations)
        {
            if (nextOperation is VariableBlock)
                readyOperations = handleVariableOperation(assay, currentTime, nextOperation);
            else if (nextOperation is StaticDeclarationBlock)
                readyOperations = handleStaticModuleDeclarations(assay, nextOperation);
            else if (nextOperation is Fluid)
                (readyOperations, currentTime) = handleFluidTransfers(assay, board, currentTime, nextOperation as Fluid);
            else throw new Exception("An operation has been categorized as a special operation, but it is not handeled. " +
                                     "The operation is: " + nextOperation.ToString());
            return (readyOperations, currentTime);
        }

        private bool isSpecialCaseOperation(Block nextOperation)
        {
            return (nextOperation is VariableBlock || nextOperation is StaticDeclarationBlock|| nextOperation is Fluid);
        }

        private int RouteDropletsToModuleAndUpdateSchedule(Board board, int startTime, FluidBlock topPriorityOperation, Module operationExecutingModule)
        {
            int finishedRoutingTime;
            if (topPriorityOperation is OutputUseage)
                finishedRoutingTime = Router.RouteDropletsToOutput(board, startTime, (OutputUseage) topPriorityOperation, FluidVariableLocations);
            else finishedRoutingTime = Router.RouteDropletsToModule(board, startTime, topPriorityOperation);
            updateSchedule(topPriorityOperation, finishedRoutingTime, startTime);
            return finishedRoutingTime;
        }

        private Board ListSchedulingSetup(Assay assay, Board board, ModuleLibrary library, int startTime) {
            assay.calculateCriticalPath();
            library.allocateModules(assay);
            library.sortLibrary();
            AllUsedModules.AddRange(board.PlacedModules);
            boardAtDifferentTimes.Add(startTime, board);
            Debug.WriteLine(board.print(AllUsedModules));
            board = board.Copy();
            return board;
        }

        public (int, Board) handleFinishingOperations(int startTime, Assay assay, Board board)
        {
            List<Block> readyOperations = assay.getReadyOperations();

            /*(*)TODO fix edge case, where the drops are routed/operations are scheduled, 
             * so that in the mean time, some operations finishes. This might lead to routing problems.
             */

            //If some operations finishes (or one needs to wait for this to happen, before any more scheduling can happen), 
            //the board needs to be saved:
            if (areOperationsFinishing(startTime, readyOperations))
            {
                boardAtDifferentTimes.Add(startTime, board);
                board = board.Copy();
            }

            //In the case that operations are finishing (or there are no operations that can be executed, before this is true),
            //the finishing operations droplets needs to be placed on the board,
            //and operations that now might be able to run, needs to be marked as such:
            while (areOperationsFinishing(startTime, readyOperations))
            {
                List<FluidBlock> nextBatchOfFinishedOperations = getNextBatchOfFinishedOperations();

                //In the case that the operations have finished while routing was performed, 
                //it is still impossible to go back in time. Therefore, the max of the two are chosen.
                startTime = Math.Max(nextBatchOfFinishedOperations.Last().endTime + 1, startTime + 1);
                foreach (var finishedOperation in nextBatchOfFinishedOperations)
                {
                    if (!(finishedOperation is StaticUseageBlock))
                    {
                        //If a module is not static, and it is not used anymore, it is "disolved",
                        //leaving the droplets that is inside the module behind:
                        BoardFluid dropletOutputFluid;
                        FluidVariableLocations.TryGetValue(finishedOperation.OriginalOutputVariable, out dropletOutputFluid);
                        //If it is the first time this type of fluid has been outputed, record it:
                        if (dropletOutputFluid == null) {
                            dropletOutputFluid = new BoardFluid(finishedOperation.OriginalOutputVariable);
                            FluidVariableLocations.Add(finishedOperation.OriginalOutputVariable, dropletOutputFluid);
                        }
                        DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                        List<Droplet> replacingDroplets = board.replaceWithDroplets(finishedOperation, dropletOutputFluid);
                        AllUsedModules.AddRange(replacingDroplets);
                    }
                    else {
                        if (finishedOperation is HeaterUseage)
                        {
                            //When a heater operation has finished, the droplets inside the heater needs to be moved out of the module,
                            //so that it can be used again, by other droplets:
                            ExtractInternalDropletsAndPlaceThemOnTheBoard(board, finishedOperation);
                        }
                    }
                    assay.updateReadyOperations(finishedOperation);
                }
                boardAtDifferentTimes.Add(startTime, board);
                readyOperations = assay.getReadyOperations();
                board = board.Copy();
            }

            return (startTime, board);
        }

        private static void ExtractInternalDropletsAndPlaceThemOnTheBoard(Board board, FluidBlock finishedOperation)
        {
            HeaterModule heater = (HeaterModule)finishedOperation.BoundModule;
            //The droplets needs to be extracted in such an order and way, that there is a route to "outside" the module, 
            //and they don't collide with the other internal droplets of the module.

            //The latter can be done by temporarily placing the internal structure of the module on the board 
            //(the routing algorithm handles the rest), while the first corresponds to extracting droplets, 
            //where they can reach an empty reactangle adjacent to the module bound to finishedOperation.
            
            //Breadth first to find the extraction order:




            //The module needs to be "placed" on the board again:
            board.UpdateGridAtGivenLocation(heater, heater.Shape);
        }

        private List<FluidBlock> getNextBatchOfFinishedOperations()
        {
            List<FluidBlock> batch = new List<FluidBlock>();
            FluidBlock nextFinishedOperation = CurrentlyRunningOpertions.Dequeue();
            batch.Add(nextFinishedOperation);
            //Need to dequeue all operations that has finishes at the same time as nextFinishedOperation.
            //Differences under "IGNORED_TIME_DIFFERENCE" are ignored.
            while (CurrentlyRunningOpertions.Count > 0 && nextFinishedOperation.endTime >= CurrentlyRunningOpertions.First.endTime - IGNORED_TIME_DIFFERENCE)
            {
                batch.Add(CurrentlyRunningOpertions.Dequeue());
            }

            return batch;
        }

        public static Block removeOperation(List<Block> readyOperations)
        {
            Block topPrioriyOperation = readyOperations.MaxBy(operation => operation.priority);
            readyOperations.Remove(topPrioriyOperation);
            return topPrioriyOperation;
        }

        private bool canExecuteMoreOperations(List<Block> readyOperations)
        {
            return readyOperations.Count > 0;
        }

        private bool areOperationsFinishing(int startTime, List<Block> readyOperations)
        {
            return CurrentlyRunningOpertions.Count > 0 && (readyOperations.Count == 0  || startTime >= CurrentlyRunningOpertions.First().endTime);
        }
        
        public int getCompletionTime(){
            return ScheduledOperations.Max(operation => operation.endTime);
        }

    }
}
