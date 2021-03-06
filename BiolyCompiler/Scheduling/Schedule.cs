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
using BiolyCompiler.Exceptions;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Scheduling
{
    public class Schedule
    {
        //Records how the board looks at all the times where the board have been changed. 
        //This is primarily for testing and visulization purposes.
        public Dictionary<int, Rectangle[]> rectanglesAtDifferentTimes = new Dictionary<int, Rectangle[]>();
        // For debuging. Used when printing the board to the console, for visulization purposes.
        public Dictionary<string, BoardFluid> FluidVariableLocations = new Dictionary<string, BoardFluid>();
        public Dictionary<string, float> Variables = new Dictionary<string, float>();
        public Dictionary<string, Module> StaticModules = new Dictionary<string, Module>();
        public SimplePriorityQueue<FluidBlock> CurrentlyRunningOpertions = new SimplePriorityQueue<FluidBlock>();
        public List<Block> ScheduledOperations = new List<Block>();
        public bool SHOULD_DO_GARBAGE_COLLECTION = true;
        public HashSet<String> NameOfInputFluids = new HashSet<string>();
        public Dictionary<string, List<IDropletSource>> OutputtedDroplets = new Dictionary<string, List<IDropletSource>>();
        public List<string> NewVariablesCreatedInThisScope = new List<string>();
        private readonly Board board;

        public const int DROP_MOVEMENT_TIME = 1; //How many time units it takes for a droplet to move from one electrode to the next.
        public const int IGNORED_TIME_DIFFERENCE = 30;
        private const string RENAME_FLUIDNAME_STRING = "renaiming - fluidtype #";
        private const string WASTE_FLUIDNAME_STRING = "waste - fluidtype #";
        public const string WASTE_MODULE_NAME = "waste @ module";

        public Schedule(int width, int height)
        {
            this.board = new Board(width, height);
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

        public void PlaceStaticModules(List<StaticDeclarationBlock> staticDeclarations)
        {
            foreach (var staticDeclaration in staticDeclarations)
            {
                if (staticDeclaration is DropletDeclaration dropletDeclaration)
                {
                    throw new NotImplementedException();
                    BoardFluid fluidType = RecordCompletlyNewFluidType(dropletDeclaration);
                    Droplet droplet = (Droplet) dropletDeclaration.getAssociatedModule();
                    bool couldBePlaced = board.FastTemplatePlace(droplet);
                    if (!couldBePlaced) throw new RuntimeException("The input module couldn't be placed. The module is: " + droplet.ToString());
                    //It is not really static, and thus must does not need to be registered as such.
                }
                else if (staticDeclaration is InputDeclaration input)
                {
                    BoardFluid fluidType = RecordCompletlyNewFluidType(input);
                    InputModule inputModule = new InputModule(fluidType, (int)input.Amount);
                    bool couldBePlaced = board.FastTemplatePlace(inputModule);
                    if (!couldBePlaced) throw new RuntimeException("The input module couldn't be placed. The module is: " + inputModule.ToString());
                    input.BoundModule = inputModule;
                    inputModule.RepositionLayout();
                    StaticModules.Add(staticDeclaration.ModuleName, inputModule);
                    NameOfInputFluids.Add(fluidType.FluidName);
                }
                else {
                    Module staticModule = getAndPlaceFirstPlaceableModule(staticDeclaration, board);
                    StaticModules.Add(staticDeclaration.ModuleName, staticModule);
                }
                
            }
            if (SHOULD_DO_GARBAGE_COLLECTION)
            {
                WasteModule waste = new WasteModule();
                bool couldBePlaced = board.FastTemplatePlace(waste);
                if (!couldBePlaced) throw new RuntimeException("The waste module couldn't be placed. The module is: " + waste.ToString());
                waste.GetInputLayout().Reposition(waste.Shape.x, waste.Shape.y);
                StaticModules.Add(WASTE_MODULE_NAME, waste);
            }
        }

        private Module getAndPlaceFirstPlaceableModule(FluidBlock operation, Board board)
        {
            Module optimalModuleTemplate = operation.getAssociatedModule();

            if (!board.FastTemplatePlace(optimalModuleTemplate))
            {
                board.FastTemplatePlace(optimalModuleTemplate);
                //Console.WriteLine(board.print(usedModules));
                throw new RuntimeException("Module \"" + optimalModuleTemplate.ToString() + "\" can't be placed");
            }
            //Now that the module has been placed, the internal rectangles in the module layout can be modified, such that they are placed correctly.
            optimalModuleTemplate.RepositionLayout();
            return optimalModuleTemplate;
        }

        private BoardFluid RecordCompletlyNewFluidType(FluidBlock operation) => RecordCompletlyNewFluidType(operation.OutputVariable);

        private BoardFluid RecordCompletlyNewFluidType(String fluidName)
        {
            if (FluidVariableLocations.ContainsKey(fluidName))
            {
                throw new InternalRuntimeException("Logic error: RecordCompletlyNewFluidType is only for fluid names that have never been used before.");
            }
            BoardFluid fluidType = new BoardFluid(fluidName);
            if (!FluidVariableLocations.ContainsKey(fluidName))
            {
                NewVariablesCreatedInThisScope.Add(fluidName);
            }
            FluidVariableLocations[fluidName] = fluidType;
            return fluidType;
        }

        private (BoardFluid, int) RecordNewFluidType(string fluidName, int currentTime, FluidBlock operation)
        {
            //If there already are droplets associated with the fluid name
            //they must be overwritten or moved to a waste module
            currentTime = RemoveFluidVariable(fluidName, currentTime, operation);

            BoardFluid fluidType = new BoardFluid(fluidName);
            if (!FluidVariableLocations.ContainsKey(fluidName))
            {
                NewVariablesCreatedInThisScope.Add(fluidName);
            }
            FluidVariableLocations[fluidName] = fluidType;
            return (fluidType, currentTime);
        }

        private int RemoveFluidVariable(string fluidName, int currentTime, FluidBlock operation)
        {
            if (FluidVariableLocations.TryGetValue(fluidName, out BoardFluid oldFluidType))
            {
                if (SHOULD_DO_GARBAGE_COLLECTION)
                {
                    currentTime = DoGarbageCollection(currentTime, operation, oldFluidType);
                }
                else
                {
                    DiscardDroplets(oldFluidType);
                }
            }

            return currentTime;
        }

        private static void DiscardDroplets(BoardFluid oldFluidType)
        {
            //This is done by changing their internal names,
            //so they can never be used in any operation later on.
            BoardFluid overwrittingFluidType = new BoardFluid(WASTE_FLUIDNAME_STRING);
            List<IDropletSource> dropletSources = new List<IDropletSource>(oldFluidType.dropletSources);
            foreach (var dropletSource in dropletSources)
            {
                dropletSource.SetFluidType(overwrittingFluidType);
            }
        }

        private int DoGarbageCollection(int currentTime, FluidBlock operation, BoardFluid oldFluidType)
        {
            int numberOfDropletsToRoute = oldFluidType.GetNumberOfDropletsAvailable();
            WasteModule waste = (WasteModule)StaticModules[WASTE_MODULE_NAME];
            waste.GetInputLayout().Droplets[0].FakeSetFluidType(oldFluidType);
            Droplet dropletInput = waste.GetInputLayout().Droplets[0];

            List<Route> wasteRoutes = new List<Route>();
            for (int i = 0; i < numberOfDropletsToRoute; i++)
            {
                Route route = Router.RouteSingleDropletToModule(waste, board, currentTime, dropletInput);
                wasteRoutes.Add(route);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                currentTime = route.getEndTime() + 1;
            }
            if (wasteRoutes.Count > 0)
            {
                operation.WasteRoutes.Add(oldFluidType.FluidName, wasteRoutes);
            }
            return currentTime;
        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public int ListScheduling<T>(DFG<Block> dfg, CommandExecutor<T> executor)
        {
            rectanglesAtDifferentTimes.Clear();
            ScheduledOperations.Clear();
            OutputtedDroplets.Clear();
            NewVariablesCreatedInThisScope.Clear();

            //Setup:
            int currentTime = 0;
            rectanglesAtDifferentTimes.Add(currentTime, board.CopyAllRectangles());

            Assay assay = new Assay(dfg);
            foreach (Block nextOperation in assay)
            {
                int oldTime = currentTime;
                nextOperation.Update<T>(Variables, executor, FluidVariableLocations);
                switch (nextOperation)
                {
                    case VariableBlock varBlock:
                        UpdateVariables<T>(varBlock, executor);
                        UpdateSchedule(nextOperation, currentTime, oldTime);
                        assay.UpdateReadyOperations(nextOperation);
                        break;
                    case Union unionBlock:
                        currentTime = HandleUnionOperation(currentTime, unionBlock);
                        UpdateSchedule(nextOperation, currentTime, oldTime);
                        assay.UpdateReadyOperations(nextOperation);
                        break;
                    case StaticDeclarationBlock decBlock:
                        assay.UpdateReadyOperations(nextOperation);
                        break;
                    case Fluid renameBlock:
                        currentTime = HandleFluidTransfers(currentTime, renameBlock);
                        UpdateSchedule(nextOperation, currentTime, oldTime);
                        assay.UpdateReadyOperations(nextOperation);
                        break;
                    case SetArrayFluid arrayRenameBlock:
                        currentTime = HandleFluidTransfers(currentTime, arrayRenameBlock);
                        UpdateSchedule(nextOperation, currentTime, oldTime);
                        assay.UpdateReadyOperations(nextOperation);
                        break;
                    case FluidBlock fluidBlock:
                        currentTime = HandleFluidOperations(currentTime, fluidBlock);
                        UpdateSchedule(nextOperation, currentTime, oldTime);
                        break;
                    default:
                        throw new InternalRuntimeException("The given block/operation type is unhandeled by the scheduler. " + Environment.NewLine + "The operation is: " + nextOperation.ToString());
                }

                //When operations finishes, while the routing associated with nextOperation was performed, 
                //this needs to be handled. Note that handleFinishingOperations will also wait for operations to finish, 
                //in the case that there are no more operations that can be executed, before this happen:
                currentTime = HandleFinishingOperations(nextOperation, currentTime, assay);
            }

            if (CurrentlyRunningOpertions.Count > 0)
            {
                throw new InternalRuntimeException("ERROR!!!");
            }
            
            SortScheduledOperations();
            return GetCompletionTime();
        }

        private int HandleFluidOperations(int currentTime, FluidBlock topPriorityOperation)
        {
            //If nextOperation is associated with a static module, 
            //this needs to be chosen as the module to execute the operation.
            //Else a module that can execute the operation needs to be found and placed on the board:
            Module operationExecutingModule = (topPriorityOperation is StaticUseageBlock staticOperation) ?
                                               StaticModules[staticOperation.ModuleName] :
                                               getAndPlaceFirstPlaceableModule(topPriorityOperation, board);
            topPriorityOperation.Bind(operationExecutingModule, FluidVariableLocations);

            //If the module can't be placed, one must wait until there is enough space for it:
            if (operationExecutingModule == null) throw new RuntimeException("Not enough space for a module: this is not handeled yet");

            //Now all the droplet that the module should operate on, needs to be delivered to it.
            //By construction, there will be a route from the droplets to the module, 
            //and so it will always be possible for this routing to be done:
            currentTime = RouteDropletsToModule(currentTime, topPriorityOperation);
            return currentTime;
        }

        private void SortScheduledOperations()
        {
            ScheduledOperations = ScheduledOperations.OrderBy(x => x.StartTime).ThenBy(x => x.EndTime).ToList();
        }

        private int ExtractAndReassignDroplets(int currentTime, FluidBlock nextOperation, int requiredDroplets, BoardFluid targetFluidType, BoardFluid inputFluid)
        {
            //First it is checked if there is there even any fluid that needs to be transfered:
            if (requiredDroplets == 0)
            {
                return currentTime;
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
                        Droplet droplet = new Droplet(targetFluidType, NameOfInputFluids);
                        droplet.SetFluidConcentrations(inputModule);
                        bool couldPlace = board.FastTemplatePlace(droplet);
                        if (!couldPlace)
                        {
                            throw new RuntimeException("Not enough space for the fluid transfer.");
                        }
                        Route route = Router.RouteDropletToNewPosition(inputModule, droplet, board, currentTime);
                        currentTime = route.getEndTime() + 1;
                        dropletRoutes.Add(route);
                    }
                    if (numberOfDropletsTransfered == requiredDroplets) break;
                }
                if (numberOfDropletsTransfered != requiredDroplets)
                {
                    throw new RuntimeException("Not enough droplets available. Fluid name: " + inputFluid.FluidName);
                }
            }
            else
            {
                currentTime += 1; //Necessary for the recording of the board below.
            }

            rectanglesAtDifferentTimes.Add(currentTime, board.CopyAllRectangles());
            currentTime += 2;
            return currentTime;
        }

        private int HandleFluidTransfers(int currentTime, FluidBlock nextOperation)
        {
            FluidInput input = nextOperation.InputFluids[0];
            //setting a fluid variable to itself is usually used to initilize
            //a variable with 0 fluid in it. So don't do anything if it already
            //exists and wants to transfer all liquid.
            if (nextOperation.OutputVariable == input.OriginalFluidName && 
                input.UseAllFluid &&
                FluidVariableLocations.ContainsKey(nextOperation.OutputVariable))
            {
                return currentTime;
            }

            int requiredDroplets = input.GetAmountInDroplets(FluidVariableLocations);

            //If there already exists droplets with the target fluid type (what the droplets should be renamed to),
            //then they are overwritten. But if the overwritten droplets are the required droplets themselves
            //then do nothing as they would otherwise be removed and then they can't be used.
            BoardFluid targetFluidType;
            if (nextOperation.OutputVariable == input.OriginalFluidName &&
                FluidVariableLocations.ContainsKey(nextOperation.OutputVariable))
            {
                targetFluidType = FluidVariableLocations[nextOperation.OutputVariable];
            }
            else
            {
                (targetFluidType, currentTime) = RecordNewFluidType(nextOperation.OutputVariable, currentTime, nextOperation);
            }

            FluidVariableLocations.TryGetValue(input.OriginalFluidName, out BoardFluid inputFluid);
            if (inputFluid == null)
            {
                throw new InternalRuntimeException($"Fluid of type \"{input.OriginalFluidName}\" was to be transfered, but fluid of this type do not exist (or have ever been created).");
            }

            currentTime = ExtractAndReassignDroplets(currentTime, nextOperation, requiredDroplets, targetFluidType, inputFluid);
            currentTime++;
            return currentTime;
        }

        private void UpdateVariables<T>(VariableBlock varBlock, CommandExecutor<T> executor)
        {
            (string variableName, float value) = varBlock.ExecuteBlock(Variables, executor, FluidVariableLocations);
            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                throw new InvalidNumberException(varBlock.BlockID, value);
            }
            if (!Variables.ContainsKey(variableName))
            {
                Variables.Add(variableName, value);
                NewVariablesCreatedInThisScope.Add(variableName);
            }
            else
            {
                Variables[variableName] = value;
            }
        }

        private int HandleUnionOperation(int currentTime, Union nextOperation){
            FluidInput input1 = nextOperation.InputFluids[0];
            FluidInput input2 = nextOperation.InputFluids[1];
            int requiredDroplets1 = input1.GetAmountInDroplets(FluidVariableLocations);
            int requiredDroplets2 = input2.GetAmountInDroplets(FluidVariableLocations);

            //First all the droplets are assigned to an intermediate fluidtype,
            //and then to the actual target fluidtype. This is necessary for the case of an union,
            //where the target is also the input.
            BoardFluid intermediateFluidtype;
            (intermediateFluidtype, currentTime) = RecordNewFluidType(RENAME_FLUIDNAME_STRING, currentTime, nextOperation);

            FluidVariableLocations.TryGetValue(input1.OriginalFluidName, out BoardFluid inputFluid1);
            if (inputFluid1 == null)
            {
                (inputFluid1, currentTime) = RecordNewFluidType(input1.OriginalFluidName, currentTime, nextOperation);
            }

            FluidVariableLocations.TryGetValue(input2.OriginalFluidName, out BoardFluid inputFluid2);
            if (inputFluid2 == null)
            {
                (inputFluid2, currentTime) = RecordNewFluidType(input2.OriginalFluidName, currentTime, nextOperation);
            }

            currentTime = ExtractAndReassignDroplets(currentTime, nextOperation, requiredDroplets1, intermediateFluidtype, inputFluid1);
            currentTime = ExtractAndReassignDroplets(currentTime, nextOperation, requiredDroplets2, intermediateFluidtype, inputFluid2);
            BoardFluid targetFluidtype;
            (targetFluidtype, currentTime) = RecordNewFluidType(nextOperation.OutputVariable, currentTime, nextOperation);
            int targetRequiredDroplets = requiredDroplets1 + requiredDroplets2;
            currentTime = ExtractAndReassignDroplets(currentTime, nextOperation, targetRequiredDroplets, targetFluidtype, intermediateFluidtype);
            FluidVariableLocations.Remove(RENAME_FLUIDNAME_STRING);
            NewVariablesCreatedInThisScope.Remove(RENAME_FLUIDNAME_STRING);

            return currentTime;
        }

        private int RouteDropletsToModule(int startTime, FluidBlock topPriorityOperation)
        {
            int finishedRoutingTime;
            if (topPriorityOperation is OutputUsage || topPriorityOperation is WasteUsage)
            {
                finishedRoutingTime = Router.RouteDropletsToOutput(board, startTime, topPriorityOperation, FluidVariableLocations);
                if (topPriorityOperation is OutputUsage outputOperation)
                {
                    List<IDropletSource> dropletsRoutedToOutput;
                    if (OutputtedDroplets.ContainsKey(outputOperation.ModuleName))
                    {
                        dropletsRoutedToOutput = OutputtedDroplets[outputOperation.ModuleName];
                    }
                    else dropletsRoutedToOutput = new List<IDropletSource>();
                    var routes = topPriorityOperation.InputRoutes.Values.Flatten();
                    foreach (Route route in routes)
                    {
                        dropletsRoutedToOutput.Add(route.routedDroplet);
                    }
                    OutputtedDroplets[outputOperation.ModuleName] = dropletsRoutedToOutput;
                }
            }
            else
            {
                finishedRoutingTime = Router.RouteDropletsToModule(board, startTime, topPriorityOperation);
            }
            return finishedRoutingTime;
        }
        
        public int HandleFinishingOperations(Block nextOperation, int currentTime, Assay assay)
        {
            /*(*)TODO fix edge case, where the drops are routed/operations are scheduled, 
             * so that in the mean time, some operations finishes. This might lead to routing problems.
             */

            //If some operations finishes (or one needs to wait for this to happen, before any more scheduling can happen), 
            //the board needs to be saved:
            if (AreOperationsFinishing(currentTime, assay) && !(nextOperation is VariableBlock))
            {
                rectanglesAtDifferentTimes.Add(currentTime, board.CopyAllRectangles());
            }

            //In the case that operations are finishing (or there are no operations that can be executed, before this is true),
            //the finishing operations droplets needs to be placed on the board,
            //and operations that now might be able to run, needs to be marked as such:
            while (AreOperationsFinishing(currentTime, assay))
            {
                List<FluidBlock> nextBatchOfFinishedOperations = GetNextBatchOfFinishedOperations();

                //In the case that the operations have finished while routing was performed, 
                //it is still impossible to go back in time. Therefore, the max of the two are chosen.
                currentTime = Math.Max(nextBatchOfFinishedOperations.Last().EndTime + 1, currentTime + 1);
                foreach (var finishedOperation in nextBatchOfFinishedOperations)
                { 
                    BoardFluid dropletOutputFluid;
                    if (!(finishedOperation is StaticUseageBlock))
                    {
                        //If a module is not static, and it is not used anymore, it is "disolved",
                        //leaving the droplets that is inside the module behind:
                        finishedOperation.UpdateInternalDropletConcentrations();
                        (dropletOutputFluid, currentTime) = RecordNewFluidType(finishedOperation.OutputVariable, currentTime, finishedOperation);
                        List<Droplet> replacingDroplets = board.replaceWithDroplets(finishedOperation.BoundModule, dropletOutputFluid);
                    }
                    else {
                        if (finishedOperation is HeaterUsage heaterOperation)
                        {
                            //When a heater operation has finished, the droplets inside the heater needs to be moved out of the module,
                            //so that it can be used again, by other droplets:

                            //General method:
                            //ExtractInternalDropletsAndPlaceThemOnTheBoard(board, finishedOperation);

                            //For the special case that the heater has size 3x3, with only one droplet inside it:
                            (dropletOutputFluid, currentTime) = RecordNewFluidType(finishedOperation.OutputVariable, currentTime, finishedOperation);
                            Droplet droplet = new Droplet(dropletOutputFluid);
                            droplet.SetFluidConcentrations(heaterOperation.InputRoutes.First().Value.First().routedDroplet);
                            bool couldBePlaced = board.FastTemplatePlace(droplet);

                            //Temporarily placing a droplet on the initial position of the heater, for routing purposes:
                            Droplet routingDroplet = new Droplet(new BoardFluid("Routing @ droplet"));
                            routingDroplet.Shape = Rectangle.Translocate(routingDroplet.Shape, heaterOperation.BoundModule.Shape.x, heaterOperation.BoundModule.Shape.y);

                            board.UpdateGridAtGivenLocation(routingDroplet, heaterOperation.BoundModule.Shape);
                            if (!couldBePlaced) throw new RuntimeException("Not enough space available to place a Droplet.");
                            Route dropletRoute = Router.RouteDropletToNewPosition(routingDroplet, droplet, board, currentTime);
                            currentTime = dropletRoute.getEndTime() + 1;
                            heaterOperation.OutputRoutes.Add(heaterOperation.OutputVariable, new List<Route>() { dropletRoute});
                            heaterOperation.EndTime = currentTime;
                            currentTime++;
                            
                            board.UpdateGridAtGivenLocation(heaterOperation.BoundModule, heaterOperation.BoundModule.Shape);

                            //Now the heater is not occupied anymore: a new heater operation can be executed:
                            ((HeaterModule)heaterOperation.BoundModule).IsInUse = false;
                        }
                    }
                    assay.UpdateReadyOperations(finishedOperation);
                }
                rectanglesAtDifferentTimes.Add(currentTime, board.CopyAllRectangles());
            }

            return currentTime;
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

        private bool AreOperationsFinishing(int startTime, Assay assay)
        {
            return CurrentlyRunningOpertions.Count > 0 && (assay.IsEmpty()  || startTime >= CurrentlyRunningOpertions.First().EndTime);
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