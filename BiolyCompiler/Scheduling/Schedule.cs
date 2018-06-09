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
using BiolyCompiler.BlocklyParts.Declarations;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.BlocklyParts.FluidicInputs;

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
        private const string RENAME_FLUIDNAME_STRING = "renaiming - fluidtype #";
        public Schedule(){

        }
        
        private static Board getCurrentBoard()
        {
            throw new NotImplementedException();
        }

        private void UpdateSchedule(Block operation, int currentTime, int startTime)
        {
            ScheduledOperations.Add(operation);
            operation.StartTime = startTime;
            if (operation is VariableBlock || 
                operation is Fluid || 
                operation is SetArrayFluid ||
                operation is Union) {
                operation.EndTime = currentTime;
                return;
            }            
            else
            {
                FluidBlock fluidOperation = operation as FluidBlock;
                int operationExecutionTime = fluidOperation.GetRunningTime();
                fluidOperation.EndTime = operation.StartTime + operationExecutionTime;// currentTime + fluidOperation.boundModule.OperationTime;
                CurrentlyRunningOpertions.Enqueue(fluidOperation, operation.EndTime);
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
                if (staticDeclaration is DropletDeclaration dropletDeclaration)
                {
                    BoardFluid fluidType = RecordNewFluidType(dropletDeclaration);
                    Droplet droplet = (Droplet) dropletDeclaration.getAssociatedModule();
                    bool couldBePlaced = board.FastTemplatePlace(droplet);
                    if (!couldBePlaced) throw new Exception("The input module couldn't be placed. The module is: " + droplet.ToString());
                    //It is not really static, and thus must does not need to be registered as such.
                }
                else if (staticDeclaration is InputDeclaration input)
                {
                    BoardFluid fluidType = RecordNewFluidType(input);
                    InputModule inputModule = new InputModule(fluidType, (int)input.Amount);
                    bool couldBePlaced = board.FastTemplatePlace(inputModule);
                    if (!couldBePlaced) throw new Exception("The input module couldn't be placed. The module is: " + inputModule.ToString());
                    input.BoundModule = inputModule;
                    inputModule.RepositionLayout();
                    StaticModules.Add(staticDeclaration.ModuleName, inputModule);
                }
                else {
                    Module staticModule = library.getAndPlaceFirstPlaceableModule(staticDeclaration, board);
                    StaticModules.Add(staticDeclaration.ModuleName, staticModule);
                }

                DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
            }
        }

        private BoardFluid RecordNewFluidType(FluidBlock operation)
        {
            return RecordNewFluidType(operation.OriginalOutputVariable);
        }

        private BoardFluid RecordNewFluidType(string fluidName)
        {
            if (FluidVariableLocations.ContainsKey(fluidName))
            {
                //If there already are droplets associated with the fluid name,
                //they must be overwritten: This is done by changing their internal names,
                //so they can never be used in any operation later on.

                BoardFluid oldFluidType = FluidVariableLocations[fluidName];
                BoardFluid overwrittingFluidType = new BoardFluid("overwritten - fluidname");
                List<IDropletSource> dropletSources = new List<IDropletSource>(oldFluidType.dropletSources);
                foreach (var dropletSource in dropletSources)
                {
                    dropletSource.SetFluidType(overwrittingFluidType);
                }
            }
            BoardFluid fluidType = new BoardFluid(fluidName);
            FluidVariableLocations[fluidName] = fluidType;
            return fluidType;
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
            Console.Write("");
            //currentTime = 0;

            //Continue until all operations have been scheduled:
            while (assay.hasUnfinishedOperations() && CanExecuteMoreOperations(readyOperations))
            {
                Block nextOperation = RemoveOperation(readyOperations);
                if (IsSpecialCaseOperation(nextOperation))
                {
                    (readyOperations, currentTime, board) = HandleSpecialCases(assay, board, currentTime, nextOperation, readyOperations);
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
                    if (!(topPriorityOperation is StaticUseageBlock)) AllUsedModules.Add(operationExecutingModule);
                    DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);

                    //If the module can't be placed, one must wait until there is enough space for it:
                    if (operationExecutingModule == null) throw new Exception("Not enough space for a module: this is not handeled yet");

                    //Now all the droplet that the module should operate on, needs to be delivered to it.
                    //By construction, there will be a route from the droplets to the module, 
                    //and so it will always be possible for this routing to be done:
                    currentTime = RouteDropletsToModuleAndUpdateSchedule(board, currentTime, topPriorityOperation, operationExecutingModule);
                    DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                } else  throw new Exception("The given block/operation type is unhandeled by the scheduler. " +
                                            "The operation is: " + nextOperation.ToString());
                
                //When operations finishes, while the routing associated with nextOperation was performed, 
                //this needs to be handled. Note that handleFinishingOperations will also wait for operations to finish, 
                //in the case that there are no more operations that can be executed, before this happen:
                (currentTime, board) = HandleFinishingOperations(currentTime, assay, board);
                readyOperations = assay.getReadyOperations();
                DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
            }
            if (assay.hasUnfinishedOperations())
            {
                throw new Exception("There were operations that couldn't be scheduled.");
            }
            SortScheduledOperations();
            return GetCompletionTime();
        }

        private void SortScheduledOperations()
        {
            ScheduledOperations = ScheduledOperations.OrderBy(x => x.StartTime).ThenBy(x => x.EndTime).ToList();
        }

        private (List<Block>, int, Board) ExtractAndReassignDroplets(Assay assay, Board board, int currentTime, FluidBlock nextOperation, int requiredDroplets, BoardFluid targetFluidType, BoardFluid inputFluid)
        {
            //First it is checked if there is there even any fluid that needs to be transfered:
            if (requiredDroplets == 0)
            {
                assay.updateReadyOperations(nextOperation);
                return (assay.getReadyOperations(), currentTime, board);
            }
            int originalStartTime = currentTime;


            //Fluid needs to be allocated to a variable.
            //If the origin of the fluid is an input, a given amount of droplets needs to moved unto the board,
            //but if origin is simply droplets placed on the board, a simple renaiming can be done instead.
            //It will prioritize taking droplets already on the board, instead of taking droplets out of the inputs.

            //Trying to take as many droplets as possible from those already placed on the board,
            //to minimize the number of droplets that must be taken from the input modules:
            List<Droplet> availableDroplets = inputFluid.dropletSources.Where(dropletSource => dropletSource is Droplet)
                                                                       .Select(dropletSource => dropletSource as Droplet)
                                                                       .ToList();
            int numberOfDropletsToTransfer = Math.Min(availableDroplets.Count, requiredDroplets);
            for (int i = 0; i < numberOfDropletsToTransfer; i++)
            {
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
                nextOperation.InputRoutes.Add(inputFluid.FluidName, dropletRoutes);
                foreach (var inputModule in inputModules)
                {
                    while (inputModule.DropletCount > 0 && numberOfDropletsTransfered < requiredDroplets)
                    {
                        numberOfDropletsTransfered++;
                        inputModule.DecrementDropletCount();
                        Droplet droplet = new Droplet(targetFluidType);
                        AllUsedModules.Add(droplet);
                        bool couldPlace = board.FastTemplatePlace(droplet);
                        if (!couldPlace) throw new Exception("Not enough space for the fluid transfer.");
                        Route route = Router.RouteDropletToNewPosition(inputModule, droplet, board, currentTime);
                        currentTime = route.getEndTime() + 1;
                        dropletRoutes.Add(route);
                    }
                    if (numberOfDropletsTransfered == requiredDroplets) break;
                }
                if (numberOfDropletsTransfered != requiredDroplets) throw new Exception("Not enough droplets available.");
            }
            else
            {
                currentTime += 1; //Necessary for the recording of the board below.
            }
            boardAtDifferentTimes.Add(currentTime, board);
            board = board.Copy();
            currentTime += 2;
            DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
            assay.updateReadyOperations(nextOperation);
            List<Block> readyOperations = assay.getReadyOperations();
            return (readyOperations, currentTime, board);
        }

        private (List<Block>, int, Board) HandleFluidTransfers(Assay assay, Board board, int currentTime, FluidBlock nextOperation)
        {
            FluidInput input = nextOperation.InputVariables[0];
            int requiredDroplets = input.GetAmountInDroplets(FluidVariableLocations);
            
            //If there already exists droplets with the target fluid type (what the droplets should be renamed to),
            //then they are overwritten:
            BoardFluid targetFluidType = RecordNewFluidType(nextOperation);

            BoardFluid inputFluid;
            FluidVariableLocations.TryGetValue(input.OriginalFluidName, out inputFluid);
            if (inputFluid == null) throw new Exception("Fluid of type \"" + input.FluidName + "\" was to be transfered, but fluid of this type do not exist (or have ever been created).");

            int originalStartTime = currentTime;
            List<Block> readyOperations;
            (readyOperations, currentTime, board) = ExtractAndReassignDroplets(assay, board, currentTime, nextOperation, requiredDroplets, targetFluidType, inputFluid);
            UpdateSchedule(nextOperation, currentTime, originalStartTime);
            currentTime++;
            //DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
            return (readyOperations, currentTime, board);
        }

        private static List<Block> HandleStaticModuleDeclarations(Assay assay, Block nextOperation)
        {
            List<Block> readyOperations;
            assay.updateReadyOperations(nextOperation);
            readyOperations = assay.getReadyOperations();
            return readyOperations;
        }

        private List<Block> HandleVariableOperation(Assay assay, int currentTime, Block nextOperation)
        {
            //This is a mathematical operation, and it should be scheduled to run as soon as possible
            if ((nextOperation as VariableBlock).CanBeScheduled)
            {
                //This is a mathematical operation, and it should be scheduled to run as soon as possible
                UpdateSchedule(nextOperation, currentTime, currentTime);
                assay.updateReadyOperations(nextOperation);
            }
            return assay.getReadyOperations();
        }

        private (List<Block>, int, Board) HandleUnionOperation(Assay assay, Board board, int currentTime, Union nextOperation){
            FluidInput input1 = nextOperation.InputVariables[0];
            FluidInput input2 = nextOperation.InputVariables[1];
            int requiredDroplets1 = input1.GetAmountInDroplets(FluidVariableLocations);
            int requiredDroplets2 = input2.GetAmountInDroplets(FluidVariableLocations);
            int originalStartTime = currentTime;

            //First all the droplets are assigned to an intermediate fluidtype,
            //and then to the actual target fluidtype. This is necessary for the case of an union,
            //where the target is also the input.
            BoardFluid intermediateFluidtype = RecordNewFluidType(RENAME_FLUIDNAME_STRING);

            BoardFluid inputFluid1;
            FluidVariableLocations.TryGetValue(input1.OriginalFluidName, out inputFluid1);
            if (inputFluid1 == null) inputFluid1 = RecordNewFluidType(input1.OriginalFluidName);

            BoardFluid inputFluid2;
            FluidVariableLocations.TryGetValue(input2.OriginalFluidName, out inputFluid2);
            if (inputFluid2 == null) inputFluid2 = RecordNewFluidType(input2.OriginalFluidName);

            List<Block> readyOperations;
            (readyOperations, currentTime, board) = ExtractAndReassignDroplets(assay, board, currentTime, nextOperation, requiredDroplets1, intermediateFluidtype, inputFluid1);
            (readyOperations, currentTime, board) = ExtractAndReassignDroplets(assay, board, currentTime, nextOperation, requiredDroplets2, intermediateFluidtype, inputFluid2);
            BoardFluid targetFluidtype = RecordNewFluidType(nextOperation);
            int targetRequiredDroplets = requiredDroplets1 + requiredDroplets2;
            (readyOperations, currentTime, board) = ExtractAndReassignDroplets(assay, board, currentTime, nextOperation, targetRequiredDroplets, targetFluidtype, intermediateFluidtype);

            UpdateSchedule(nextOperation, currentTime, originalStartTime);
            return (readyOperations, currentTime, board);
        }

        private (List<Block>, int, Board) HandleSpecialCases(Assay assay, Board board, int currentTime, Block nextOperation, List<Block> readyOperations)
        {
            if (nextOperation is VariableBlock)
                readyOperations = HandleVariableOperation(assay, currentTime, nextOperation);
            else if (nextOperation is Union)
                (readyOperations, currentTime, board) = HandleUnionOperation(assay, board, currentTime, (Union)nextOperation);
            else if (nextOperation is StaticDeclarationBlock)
                readyOperations = HandleStaticModuleDeclarations(assay, nextOperation);
            else if (nextOperation is Fluid || nextOperation is SetArrayFluid)
                (readyOperations, currentTime, board) = HandleFluidTransfers(assay, board, currentTime, (FluidBlock)nextOperation);
            else throw new Exception("An operation has been categorized as a special operation, but it is not handeled. " +
                                     "The operation is: " + nextOperation.ToString());
            return (readyOperations, currentTime, board);
        }

        public static bool IsSpecialCaseOperation(Block nextOperation)
        {
            return (nextOperation is VariableBlock || 
                    nextOperation is Union || 
                    nextOperation is StaticDeclarationBlock || 
                    nextOperation is Fluid ||
                    nextOperation is SetArrayFluid);
        }

        private int RouteDropletsToModuleAndUpdateSchedule(Board board, int startTime, FluidBlock topPriorityOperation, Module operationExecutingModule)
        {
            int finishedRoutingTime;
            if (topPriorityOperation is OutputUseage)
                finishedRoutingTime = Router.RouteDropletsToOutput(board, startTime, (OutputUseage) topPriorityOperation, FluidVariableLocations);
            else finishedRoutingTime = Router.RouteDropletsToModule(board, startTime, topPriorityOperation);
            UpdateSchedule(topPriorityOperation, finishedRoutingTime, startTime);
            return finishedRoutingTime;
        }

        private Board ListSchedulingSetup(Assay assay, Board board, ModuleLibrary library, int startTime) {
            assay.calculateCriticalPath();
            library.allocateModules(assay);
            library.sortLibrary();
            AllUsedModules.AddRange(board.PlacedModules);
            boardAtDifferentTimes.Add(startTime, board);
            if (!assay.dfg.Nodes.Select(node => node.value).All(operation => operation is VariableBlock))
            {
                DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                board = board.Copy();
            }
            return board;
        }
        
        public (int, Board) HandleFinishingOperations(int startTime, Assay assay, Board board)
        {
            List<Block> readyOperations = assay.getReadyOperations();

            /*(*)TODO fix edge case, where the drops are routed/operations are scheduled, 
             * so that in the mean time, some operations finishes. This might lead to routing problems.
             */

            //If some operations finishes (or one needs to wait for this to happen, before any more scheduling can happen), 
            //the board needs to be saved:
            if (AreOperationsFinishing(startTime, readyOperations))
            {
                boardAtDifferentTimes.Add(startTime, board);
                board = board.Copy();
            }

            //In the case that operations are finishing (or there are no operations that can be executed, before this is true),
            //the finishing operations droplets needs to be placed on the board,
            //and operations that now might be able to run, needs to be marked as such:
            while (AreOperationsFinishing(startTime, readyOperations))
            {
                List<FluidBlock> nextBatchOfFinishedOperations = GetNextBatchOfFinishedOperations();

                //In the case that the operations have finished while routing was performed, 
                //it is still impossible to go back in time. Therefore, the max of the two are chosen.
                startTime = Math.Max(nextBatchOfFinishedOperations.Last().EndTime + 1, startTime + 1);
                foreach (var finishedOperation in nextBatchOfFinishedOperations)
                {
                    if (!(finishedOperation is StaticUseageBlock))
                    {
                        //If a module is not static, and it is not used anymore, it is "disolved",
                        //leaving the droplets that is inside the module behind:
                        BoardFluid dropletOutputFluid = RecordNewFluidType(finishedOperation);
                        List<Droplet> replacingDroplets = board.replaceWithDroplets(finishedOperation, dropletOutputFluid);
                        DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                        AllUsedModules.AddRange(replacingDroplets);
                    }
                    else {
                        if (finishedOperation is HeaterUseage heaterOperation)
                        {
                            //When a heater operation has finished, the droplets inside the heater needs to be moved out of the module,
                            //so that it can be used again, by other droplets:

                            //General method:
                            //ExtractInternalDropletsAndPlaceThemOnTheBoard(board, finishedOperation);

                            //For the special case that the heater has size 3x3, with only one droplet inside it:
                            DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
                            BoardFluid dropletOutputFluid = RecordNewFluidType(finishedOperation);
                            Droplet droplet = new Droplet(dropletOutputFluid);
                            AllUsedModules.Add(droplet);
                            bool couldBePlaced = board.FastTemplatePlace(droplet);

                            //Temporaily placing a droplet on the initial position of the heater, for routing purposes:
                            Droplet routingDroplet = new Droplet(new BoardFluid("Routing - droplet"));
                            routingDroplet.Shape.PlaceAt(heaterOperation.BoundModule.Shape.x, heaterOperation.BoundModule.Shape.y);
                            board.UpdateGridAtGivenLocation(routingDroplet, heaterOperation.BoundModule.Shape);
                            if (!couldBePlaced) throw new Exception("Not enough space available to place a Droplet.");
                            Route dropletRoute = Router.RouteDropletToNewPosition(routingDroplet, droplet, board, startTime);
                            startTime = dropletRoute.getEndTime() + 1;
                            heaterOperation.OutputRoutes.Add(heaterOperation.OriginalOutputVariable, new List<Route>() { dropletRoute});
                            heaterOperation.EndTime = startTime;
                            startTime++;
                            
                            board.UpdateGridAtGivenLocation(heaterOperation.BoundModule, heaterOperation.BoundModule.Shape);
                            DebugTools.makeDebugCorrectnessChecks(board, CurrentlyRunningOpertions, AllUsedModules);
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

        private List<FluidBlock> GetNextBatchOfFinishedOperations()
        {
            List<FluidBlock> batch = new List<FluidBlock>();
            FluidBlock nextFinishedOperation = CurrentlyRunningOpertions.Dequeue();
            batch.Add(nextFinishedOperation);
            //Need to dequeue all operations that has finishes at the same time as nextFinishedOperation.
            //Differences under "IGNORED_TIME_DIFFERENCE" are ignored.
            while (CurrentlyRunningOpertions.Count > 0 && nextFinishedOperation.EndTime >= CurrentlyRunningOpertions.First.EndTime - IGNORED_TIME_DIFFERENCE)
            {
                batch.Add(CurrentlyRunningOpertions.Dequeue());
            }

            return batch;
        }

        public static Block RemoveOperation(List<Block> readyOperations)
        {
            Block topPrioriyOperation = readyOperations.MaxBy(operation => operation.priority);
            readyOperations.Remove(topPrioriyOperation);
            return topPrioriyOperation;
        }

        private bool CanExecuteMoreOperations(List<Block> readyOperations)
        {
            return readyOperations.Count > 0;
        }

        private bool AreOperationsFinishing(int startTime, List<Block> readyOperations)
        {
            return CurrentlyRunningOpertions.Count > 0 && (readyOperations.Count == 0  || startTime >= CurrentlyRunningOpertions.First().EndTime);
        }
        
        public int GetCompletionTime()
        {
            if (ScheduledOperations.Count == 0)
            {
                return -1;
            }
            return ScheduledOperations.Max(operation => operation.EndTime);
        }

    }
}