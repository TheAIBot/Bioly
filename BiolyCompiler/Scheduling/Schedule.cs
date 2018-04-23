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
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Schedule
    {
        //Records how the board looks at all the times where the board have been changed. 
        //This is primarily for testing and visulization purposes.
        public Dictionary<int, Board> boardAtDifferentTimes = new Dictionary<int, Board>();
        // For debuging. Used when printing the board to the console, for visulization purposes.
        public List<Module> allUsedModules = new List<Module>(); 
        public Dictionary<string, BoardFluid> FluidVariableLocations = new Dictionary<string, BoardFluid>();
        public Dictionary<string, Module> StaticModules = new Dictionary<string, Module>();
        public SimplePriorityQueue<FluidBlock> CurrentlyRunningOpertions = new SimplePriorityQueue<FluidBlock>();
        public List<Block> ScheduledOperations = new List<Block>();
        public const int DROP_MOVEMENT_TIME = 1; //How many time units it takes for a droplet to move over one electrode.
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
            operation.startTime = startTime;
            if (operation is VariableBlock) return;
            else
            {
                FluidBlock fluidOperation = operation as FluidBlock;
                fluidOperation.boundModule.StartTime = startTime;
                int moduleRunningTime = fluidOperation.boundModule.ToCommands().Last().Time;
                fluidOperation.endTime = fluidOperation.boundModule.StartTime + moduleRunningTime;// currentTime + fluidOperation.boundModule.OperationTime;
                CurrentlyRunningOpertions.Enqueue(fluidOperation, operation.endTime);
            }
        }

        private static void waitForAFinishedOperation()
        {
            throw new NotImplementedException();
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
                    FluidVariableLocations.TryGetValue(input.OutputVariable, out BoardFluid fluidType);
                    if (fluidType == null)
                    {
                        fluidType = new BoardFluid(input.OutputVariable);
                        FluidVariableLocations.Add(input.OutputVariable, fluidType);
                    }
                    InputModule inputModule = new InputModule(fluidType, input.Amount);
                    bool couldBePlaced = board.FastTemplatePlace(inputModule);
                    if (!couldBePlaced) throw new Exception("The input module couldn't be placed. The module is: " + inputModule.ToString());
                    input.boundModule = inputModule;
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
            //(*) TODO sammenlign igen med algoritmen set i artiklen

            //Setup:
            int startTime = 0;
            board = ListSchedulingSetup(assay, board, library, startTime);
            List<Block> readyOperations = assay.getReadyOperations();

            //Continue until all operations have been scheduled:
            while (assay.hasUnfinishedOperations() && canExecuteMoreOperations(readyOperations))
            {
                Block nextOperation = removeOperation(readyOperations);

                if (nextOperation is VariableBlock)
                {
                    //This is a mathematical operation, and it should be scheduled to run as soon as possible
                    updateSchedule(nextOperation, startTime, startTime);
                    assay.updateReadyOperations(nextOperation);
                }
                else if (nextOperation is StaticDeclarationBlock)
                {
                    assay.updateReadyOperations(nextOperation);
                    continue;
                    //throw new Exception("Static module declarations must not be part of the DFG that is being scheduled." +
                    //                    "The operation at fault is: " + nextOperation.ToString());
                }
                else if (nextOperation is FluidBlock topPriorityOperation)
                {
                    Module operationExecutingModule;
                    if (topPriorityOperation is StaticUseageBlock staticOperation)
                        operationExecutingModule = StaticModules[staticOperation.ModuleName];
                    else operationExecutingModule = library.getAndPlaceFirstPlaceableModule(topPriorityOperation, board); //Also called place
                    topPriorityOperation.Bind(operationExecutingModule);
                    allUsedModules.Add(operationExecutingModule);
                    makeDebugCorrectnessChecks(board);

                    //If the module can't be placed, one must wait until there is enough space for it:
                    if (operationExecutingModule == null) throw new Exception("Not enough space for a module: this is not handeled yet");

                    //Now all the droplet that the module should operate on, needs to be delivered to it.
                    //By construction, there will be a route from the droplets to the module, 
                    //and so it will always be possible for this routing to be done:
                    startTime = RouteDropletsToModule(operationExecutingModule, board, startTime, topPriorityOperation);
                    makeDebugCorrectnessChecks(board);

                    //Note that handleFinishingOperations will also wait for operations to finish, 
                    //in the case that there are no more operations that can be executed, before this happen:
                    (startTime, board) = handleFinishingOperations(startTime, assay, board);
                    readyOperations = assay.getReadyOperations();
                    makeDebugCorrectnessChecks(board);
                } else throw new Exception("The given block/operation type is unhandeled by the scheduler. " +
                                           "It is of type: " +  nextOperation.GetType() + ", and it is operation/block: " + nextOperation.ToString());
            }
            if (assay.hasUnfinishedOperations()) throw new Exception("There were operations that couldn't be scheduled.");
            ScheduledOperations.Sort((x, y) => (x.startTime < y.startTime || (x.startTime == y.startTime && x.endTime <= y.endTime)) ? 0 : 1);
            return getCompletionTime();
        }

        private Board ListSchedulingSetup(Assay assay, Board board, ModuleLibrary library, int startTime) {
            assay.calculateCriticalPath();
            library.allocateModules(assay);
            library.sortLibrary();
            allUsedModules.AddRange(board.placedModules);
            boardAtDifferentTimes.Add(startTime, board);
            Debug.WriteLine(board.print(allUsedModules));
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
                startTime = nextBatchOfFinishedOperations.Last().endTime + 1;
                foreach (var finishedOperation in nextBatchOfFinishedOperations)
                {
                    if (!(finishedOperation is StaticUseageBlock))
                    {
                        BoardFluid dropletOutputFluid;
                        FluidVariableLocations.TryGetValue(finishedOperation.OutputVariable, out dropletOutputFluid);
                        //If it is the first time this type of fluid has been outputed, record it:
                        if (dropletOutputFluid == null)
                        {
                            dropletOutputFluid = new BoardFluid(finishedOperation.OutputVariable);
                            FluidVariableLocations.Add(finishedOperation.OutputVariable, dropletOutputFluid);
                        }
                        makeDebugCorrectnessChecks(board);
                        List<Droplet> replacingDroplets = board.replaceWithDroplets(finishedOperation, dropletOutputFluid);
                        allUsedModules.AddRange(replacingDroplets);
                    }
                    assay.updateReadyOperations(finishedOperation);
                }
                boardAtDifferentTimes.Add(startTime, board);
                readyOperations = assay.getReadyOperations();
                board = board.Copy();
            }

            return (startTime, board);
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

        private bool canExecuteMoreOperations(List<Block> readyOperations)
        {
            return readyOperations.Count > 0;
        }

        private bool areOperationsFinishing(int startTime, List<Block> readyOperations)
        {
            return CurrentlyRunningOpertions.Count > 0 && (readyOperations.Count == 0  || startTime >= CurrentlyRunningOpertions.First().endTime);
        }

        public int RouteDropletsToModule(Module operationExecutingModule, Board board, int currentTime, FluidBlock topPriorityOperation)
        {
            int originalStartTime = currentTime;
            foreach (var dropletInput in operationExecutingModule.GetInputLayout().Droplets)
            {
                Route route = RouteSingleDropletToModule(operationExecutingModule, board, currentTime, dropletInput);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                currentTime = route.getEndTime() + 1;
            }
            updateSchedule(topPriorityOperation, currentTime, originalStartTime);
            return currentTime;
        }

        private static Route RouteSingleDropletToModule(Module operationExecutingModule, Board board, int currentTime, Droplet dropletInput)
        {
            BoardFluid InputFluidType = dropletInput.getFluidType();
            if (InputFluidType.GetNumberOfDropletsAvailable() < 1) throw new Exception("There isn't enough droplets of type " + InputFluidType.FluidName +
                                                                                       " avaiable, to satisfy the requirement of the module: " + operationExecutingModule.ToString());
            //Routes a droplet of type InputFluid to the module.
            Route route = DetermineRouteToModule(InputFluidType, operationExecutingModule, dropletInput, board, currentTime); //Will be included as part of a later step.
            if (route == null) throw new Exception("No route found. This should not be possible.");

            //The route is added to the module's routes:
            List<Route> inputRoutes;
            operationExecutingModule.InputRoutes.TryGetValue(InputFluidType.FluidName, out inputRoutes);
            if (inputRoutes == null)
            {
                inputRoutes = new List<Route>();
                operationExecutingModule.InputRoutes.Add(InputFluidType.FluidName, inputRoutes);
            }
            inputRoutes.Add(route);

            //The droplet routed is used by the module, and as such it can be removed from the board,
            //unless it comes from a spawner:
            switch (route.routedDroplet)
            {
                case Droplet dropletSource:
                    board.FastTemplateRemove(dropletSource);
                    break;
                case InputModule dropletSource:
                    if (1 < dropletSource.DropletCount) dropletSource.DecrementDropletCount();
                    else if (dropletSource.DropletCount == 1)
                    {
                        dropletSource.DecrementDropletCount();
                        //board.FastTemplateRemove(dropletSource); 
                    }
                    else
                    {
                        throw new Exception("The droplet spawner has a negative droplet count. Droplet source: " + dropletSource.ToString());
                    }
                    break;
                default:
                    throw new Exception("Unhandled droplet source: " + route.routedDroplet.ToString());
            }

            return route;
        }

        public int getCompletionTime(){
            return ScheduledOperations.Max(operation => operation.endTime);
        }

        public static Route DetermineRouteToModule(BoardFluid targetFluidType, Module sourceModule, Droplet targetInputDroplet, Board board, int startTime)
        {
            //Dijkstras algorithm, based on the one seen on wikipedia.
            //Finds the route from the module to route to (source module), to the closest droplet of type targetFluidType,
            //and then inverts the route.
            RoutingInformation[,] dijkstraGraph = createDijkstraGraph(board);
            (int startingXPos, int startingYPos) = targetInputDroplet.getMiddleOfSource();
            RoutingInformation source = dijkstraGraph[startingXPos, startingYPos];
            source.distanceFromSource = 0;

            SimplePriorityQueue<RoutingInformation, int> priorityQueue = new SimplePriorityQueue<RoutingInformation, int>();
            foreach (var node in dijkstraGraph)
            {
                priorityQueue.Enqueue(node, node.distanceFromSource);
            }

            while (priorityQueue.Count > 0)
            {
                RoutingInformation currentNode = priorityQueue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.x, currentNode.y];

                if (isUnreachableNode(currentNode))
                    throw new Exception("No route to the desired component could be found. Desired droplet type: " + targetFluidType.FluidName);
                else if (haveReachedDropletOfTargetType(targetFluidType, moduleAtCurrentNode, sourceModule, currentNode)) //Have reached the desired module
                    return GetRouteFromSourceToTarget(currentNode, (IDropletSource)moduleAtCurrentNode, startTime); 
                //No collisions with other modules are allowed (except the starting module):
                else if (hasCollisionWithOtherModules(sourceModule, moduleAtCurrentNode))
                    continue;

                //go through all neighbors
                if (0 < currentNode.x)
                    UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x - 1, currentNode.y]);
                if (0 < currentNode.y)
                    UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x, currentNode.y - 1]);
                if (currentNode.x < board.width - 1)
                    UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x + 1, currentNode.y]);
                if (currentNode.y < board.heigth - 1)
                    UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x, currentNode.y + 1]);

            }
            //If no route was found:
            throw new Exception("No route to the desired component could be found");
        }

        private static void UpdateNeighborPriority(SimplePriorityQueue<RoutingInformation, int> priorityQueue, RoutingInformation currentNode, RoutingInformation neighbor)
        {
            //Unit lenght distances, and thus the distance is incremented with a +1.
            int distanceToNeighborFromCurrent = currentNode.distanceFromSource + 1;
            if (distanceToNeighborFromCurrent < neighbor.distanceFromSource)
            {
                neighbor.distanceFromSource = distanceToNeighborFromCurrent;
                neighbor.previous = currentNode;
                priorityQueue.UpdatePriority(neighbor, neighbor.distanceFromSource);
            }
        }

        private static bool hasCollisionWithOtherModules(Module sourceModule, Module moduleAtCurrentNode)
        {
            return !(moduleAtCurrentNode == null || moduleAtCurrentNode == sourceModule);
        }

        private static bool isUnreachableNode(RoutingInformation currentNode)
        {
            return currentNode.distanceFromSource == Int32.MaxValue;
        }

        private static bool haveReachedDropletOfTargetType(BoardFluid targetFluidType, Module moduleAtCurrentNode, Module sourceModule, RoutingInformation location)
        {
            return moduleAtCurrentNode is IDropletSource dropletSource  && 
                   dropletSource.getFluidType().Equals(targetFluidType);
        }
        
        private static Route GetRouteFromSourceToTarget(RoutingInformation routeInfo, IDropletSource routedDroplet, int startTime)
        {
            (int dropletMiddleX, int dropletMiddleY) = routedDroplet.getMiddleOfSource();
            //Currently, the route ends at the edges of the droplets location: it will need to be routed to the middle:
            while (dropletMiddleX - routeInfo.x != 0)
            {
                RoutingInformation nextPosition = new RoutingInformation(routeInfo.x + ((dropletMiddleX - routeInfo.x > 0)? 1: -1), routeInfo.y);
                nextPosition.previous = routeInfo;
                routeInfo = nextPosition;
            }
            while (dropletMiddleY - routeInfo.y != 0)
            {
                RoutingInformation nextPosition = new RoutingInformation(routeInfo.x, routeInfo.y + ((dropletMiddleY - routeInfo.y > 0) ? 1 : -1));
                nextPosition.previous = routeInfo;
                routeInfo = nextPosition;
            }

            List<RoutingInformation> routeNodes = new List<RoutingInformation>();
            while(routeInfo.previous != null)
            {
                routeNodes.Add(routeInfo);
                routeInfo = routeInfo.previous;
            }
            routeNodes.Add(routeInfo);
            //routeNodes.Reverse();
            Route route = new Route(routeNodes, routedDroplet, startTime);
            return route;
        }

        private static RoutingInformation[,] createDijkstraGraph(Board board)
        {
            RoutingInformation[,] dijkstraGraph = new RoutingInformation[board.width, board.heigth];

            for (int x = 0; x < dijkstraGraph.GetLength(0); x++)
            {
                for (int y = 0; y < dijkstraGraph.GetLength(1); y++)
                {
                    dijkstraGraph[x, y] = new RoutingInformation(x, y);
                }
            }

            return dijkstraGraph;
        }

        private void makeDebugCorrectnessChecks(Board board)
        {
            Debug.WriteLine(board.print(allUsedModules));
            CurrentlyRunningOpertions.ToList()
                                     .OrderBy(element => element.startTime)
                                     .ForEach(element => Debug.WriteLine(element.OutputVariable + ", " + element.startTime + ", " + element.endTime));
            checkAdjacencyMatrixCorrectness(board);
        }

        public static void checkAdjacencyMatrixCorrectness(Board board)
        {
            if (!doAdjacencyGraphContainTheCorrectNodes(board))
                throw new Exception("The boards adjacency graph does not match up with the placed modules and empty rectangles.");
        }

        public static Block removeOperation(List<Block> readyOperations){
            Block topPrioriyOperation = readyOperations.MaxBy(operation => operation.priority);
            readyOperations.Remove(topPrioriyOperation);
            return topPrioriyOperation;
        }

        public static bool doAdjacencyGraphContainTheCorrectNodes(Board board)
        {
            //It visits all the modules and rectangles in the graph, 
            //and checks if they are in board.PlacedModules and board.EmptyRectangles respectivly.
            HashSet<Rectangle> emptyVisitedRectangles = new HashSet<Rectangle>();
            HashSet<Rectangle> moduleVisitedRectangles = new HashSet<Rectangle>();

            Rectangle initialRectangle = GetRandomRectangle(board.EmptyRectangles);
            emptyVisitedRectangles.Add(initialRectangle);
            Queue<Rectangle> rectanglesToVisit = new Queue<Rectangle>();
            rectanglesToVisit.Enqueue(initialRectangle);

            while (rectanglesToVisit.Count > 0)
            {
                Rectangle currentRectangle = rectanglesToVisit.Dequeue();
                foreach (var adjacentRectangle in currentRectangle.AdjacentRectangles)
                {
                    if (emptyVisitedRectangles.Contains(adjacentRectangle) || moduleVisitedRectangles.Contains(adjacentRectangle))
                        continue;
                    else
                    {
                        if (adjacentRectangle.isEmpty)
                            emptyVisitedRectangles.Add(adjacentRectangle);
                        else
                            moduleVisitedRectangles.Add(adjacentRectangle);
                        rectanglesToVisit.Enqueue(adjacentRectangle);
                    }
                }
            }

            HashSet<Rectangle> placedModuleRectangles = new HashSet<Rectangle>(board.placedModules.Select(module => module.Shape));


            return isSameSet(emptyVisitedRectangles, board.EmptyRectangles) && isSameSet(moduleVisitedRectangles, placedModuleRectangles);
        }

        private static bool isSameSet(HashSet<Rectangle> set1, HashSet<Rectangle> set2)
        {
            return set1.Count == set2.Count && set1.All(rectangle => set2.Contains(rectangle));
        }

        private static Rectangle GetRandomRectangle(HashSet<Rectangle> set)
        {
            foreach (var rectangle in set)
                return rectangle;
            return null;
        }


    }
}
