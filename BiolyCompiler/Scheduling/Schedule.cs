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

        private void updateSchedule(Block operation, int startTime)
        {
            ScheduledOperations.Add(operation);
            operation.startTime = startTime;
            if (operation is VariableBlock) return;
            else
            {
                FluidBlock fluidOperation = operation as FluidBlock;
                fluidOperation.endTime = fluidOperation.startTime + fluidOperation.boundModule.OperationTime;
                CurrentlyRunningOpertions.Enqueue(fluidOperation, operation.endTime);
            }
        }

        private static void waitForAFinishedOperation()
        {
            throw new NotImplementedException();
        }

        public void TransferFluidVariableLocationInformation(Dictionary<string, BoardFluid> FluidLocationInformation)
        {
            FluidLocationInformation.ForEach(pair => FluidVariableLocations.Add(pair.Key, pair.Value));
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
                    updateSchedule(nextOperation, startTime);
                    assay.updateReadyOperations(nextOperation);
                }
                else // nextOperation is FluidBlock
                {
                    FluidBlock topPriorityOperation = nextOperation as FluidBlock;
                    Module operationExecutingModule = library.getAndPlaceFirstPlaceableModule(topPriorityOperation, board); //Also called place
                    topPriorityOperation.Bind(operationExecutingModule);
                    allUsedModules.Add(operationExecutingModule);

                    Debug.WriteLine(board.print(allUsedModules));

                    //If the module can't be placed, one must wait until there is enough space for it:
                    if (operationExecutingModule == null) throw new Exception("Not enough space for a module: this is not handeled yet");
                    
                    CurrentlyRunningOpertions.ToList().OrderBy(element => element.startTime).ForEach(element => Debug.WriteLine(element.OutputVariable + ", " + element.startTime + ", " + element.endTime));
                    

                    //Now all the droplet that the module should operate on, needs to be delivered to it.
                    //By construction, there will be a route from the droplets to the module, 
                    //and so it will always be possible for this routing to be done:
                    startTime = RouteDropletsToModule(operationExecutingModule, board, startTime, topPriorityOperation);
                    Debug.WriteLine(board.print(allUsedModules));
                    
                    //Note that handleFinishingOperations will also wait for operations to finish, 
                    //in the case that there are no more operations that can be executed, before this happen:
                    (startTime, board) = handleFinishingOperations(startTime, assay, board);
                    readyOperations = assay.getReadyOperations();
                    Debug.WriteLine(board.print(allUsedModules));
                }
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
                    if (!finishedOperation.boundModule.isStaticModule())
                    {
                        BoardFluid dropletOutputFluid;
                        FluidVariableLocations.TryGetValue(finishedOperation.OutputVariable, out dropletOutputFluid);
                        //If it is the first time this type of fluid has been outputed, record it:
                        if (dropletOutputFluid == null)
                        {
                            dropletOutputFluid = new BoardFluid(finishedOperation.OutputVariable);
                            FluidVariableLocations.Add(finishedOperation.OutputVariable, dropletOutputFluid);
                        }
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

        private bool canExecuteMoreOperations(List<Block> readyOperations)
        {
            return readyOperations.Count > 0;
        }

        private bool areOperationsFinishing(int startTime, List<Block> readyOperations)
        {
            return CurrentlyRunningOpertions.Count > 0 && (readyOperations.Count == 0  || startTime >= CurrentlyRunningOpertions.First().endTime);
        }

        public int RouteDropletsToModule(Module operationExecutingModule, Board board, int startTime, FluidBlock topPriorityOperation)
        {
            foreach (var InputFluid in topPriorityOperation.InputVariables)
            {
                BoardFluid InputFluidType;
                bool doSourceExist = FluidVariableLocations.TryGetValue(InputFluid.FluidName, out InputFluidType);
                if (!doSourceExist) throw new Exception("The source \"" + InputFluid + "\" for the operation \"" + topPriorityOperation.ToString() + "\" do not exist.");
                if (InputFluidType.droplets.Count < InputFluid.GetAmountInDroplets()) throw new Exception("The module requires " + InputFluid.GetAmountInDroplets() + " droplets, of type " + InputFluid.FluidName +
                                                                                                          ", but only " + InputFluidType.droplets.Count + " is available. The module is: " + operationExecutingModule.ToString());
                startTime = RouteGivenNumberOfDropletsOfGivenType(operationExecutingModule, board, startTime, InputFluidType, InputFluid.GetAmountInDroplets());
            }
            updateSchedule(topPriorityOperation, startTime);
            return startTime;
        }

        private static int RouteGivenNumberOfDropletsOfGivenType(Module operationExecutingModule, Board board, int startTime, BoardFluid inputFluidType, int numberOfDropletsToRoute)
        {
            List<Route> inputRoutes = new List<Route>();
            for (int i = 0; i < numberOfDropletsToRoute; i++)
            {
                //Routes a droplet of type InputFluid to the module.
                Route route = DetermineRouteToModule(inputFluidType, operationExecutingModule, board, startTime); //Will be included as part of a later step.
                if (route == null) throw new Exception("No route found. This should not be possible.");
                inputRoutes.Add(route);
                //The droplet routed is used by the module, and as such it can be removed from the board:
                board.FastTemplateRemove(route.routedDroplet);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                startTime = route.getEndTime() + 1;
            }
            operationExecutingModule.InputRoutes.Add(inputFluidType.FluidName, inputRoutes);
            return startTime;
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

        public int getCompletionTime(){
            return ScheduledOperations.Max(operation => operation.endTime);
        }

        public static Route DetermineRouteToModule(BoardFluid targetFluidType, Module sourceModule, Board board, int startTime){

            //Dijkstras algorithm, based on the one seen on wikipedia.
            //Finds the route from the module to route to (source module), to the closest droplet of type targetFluidType,
            //and then inverts the route.
            Node<RoutingInformation>[,] dijkstraGraph = createDijkstraGraph(board);
            Node<RoutingInformation> source = board.getSourceNodeForSourceModule(sourceModule, dijkstraGraph);
            source.value.distanceFromSource = 0;
            SimplePriorityQueue<Node<RoutingInformation>, int> priorityQueue = new SimplePriorityQueue<Node<RoutingInformation>, int>();
            foreach (var node in dijkstraGraph) priorityQueue.Enqueue(node, node.value.distanceFromSource);


            while (priorityQueue.Count > 0)
            {

                Node<RoutingInformation> currentNode = priorityQueue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.value.x, currentNode.value.y];

                if (isUnreachableNode(currentNode)) throw new Exception("No route to the desired component could be found");
                else if (haveReachedDropletOfTargetType(targetFluidType, moduleAtCurrentNode)) return GetRouteFromSourceToTarget(currentNode, (Droplet) moduleAtCurrentNode, startTime); //Have reached the desired module
                //No collisions with other modules are allowed (except the starting module):
                else if (hasNoCollisionWithOtherModules(sourceModule, moduleAtCurrentNode)) continue;

                foreach (var neighbor in currentNode.getOutgoingEdges())
                {
                    //Unit lenght distances, and thus the distance is with a +1.
                    int distanceToNeighborFromCurrent = currentNode.value.distanceFromSource + 1;
                    if (distanceToNeighborFromCurrent < neighbor.value.distanceFromSource)
                    {
                        neighbor.value.distanceFromSource = distanceToNeighborFromCurrent;
                        neighbor.value.previous = currentNode;
                        priorityQueue.UpdatePriority(neighbor, distanceToNeighborFromCurrent);
                    }
                }

            }
            //If no route was found:
            return null;
        }

        private static bool hasNoCollisionWithOtherModules(Module sourceModule, Module moduleAtCurrentNode)
        {
            return moduleAtCurrentNode != null && moduleAtCurrentNode != sourceModule;
        }

        private static bool isUnreachableNode(Node<RoutingInformation> currentNode)
        {
            return currentNode.value.distanceFromSource == Int32.MaxValue;
        }

        private static bool haveReachedDropletOfTargetType(BoardFluid targetFluidType, Module moduleAtCurrentNode)
        {
            Droplet droplet = moduleAtCurrentNode as Droplet;
            return droplet != null && droplet.fluidType.Equals(targetFluidType);
        }

        private static Route GetRouteFromSourceToTarget(Node<RoutingInformation> currentNode, Droplet routedDroplet, int startTime)
        {
            List<Node<RoutingInformation>> routeNodes = new List<Node<RoutingInformation>>();
            while(currentNode.value.previous != null)
            {
                routeNodes.Add(currentNode);
                currentNode = currentNode.value.previous;
            }
            routeNodes.Add(currentNode);
            routeNodes.Reverse();
            Route route = new Route(routeNodes, routedDroplet, startTime);
            return route;
        }

        private static Node<RoutingInformation>[,] createDijkstraGraph(Board board)
        {
            Node<RoutingInformation>[,] dijkstraGraph = new Node<RoutingInformation>[board.width, board.heigth];
            for (int i = 0; i < dijkstraGraph.GetLength(0); i++) { 
                for (int j = 0; j < dijkstraGraph.GetLength(1); j++) {
                    dijkstraGraph[i, j] = new Node<RoutingInformation>(new RoutingInformation());
                    dijkstraGraph[i, j].value.x = i;
                    dijkstraGraph[i, j].value.y = j;
                }
            }
            //Adding edges:
            for (int i = 0; i < dijkstraGraph.GetLength(0); i++) {
                for (int j = 0; j < dijkstraGraph.GetLength(1); j++) {
                    if (0 < i) dijkstraGraph[i, j].AddOutgoingEdge(dijkstraGraph[i - 1, j]);
                    if (0 < j) dijkstraGraph[i, j].AddOutgoingEdge(dijkstraGraph[i, j - 1]);
                    if (i < board.width - 1 ) dijkstraGraph[i, j].AddOutgoingEdge(dijkstraGraph[i + 1, j]);
                    if (j < board.heigth - 1) dijkstraGraph[i, j].AddOutgoingEdge(dijkstraGraph[i, j + 1]);
                }
            }
            return dijkstraGraph;
        }

        public static Block removeOperation(List<Block> readyOperations){
            Block topPrioriyOperation = readyOperations.MaxBy(operation => operation.priority);
            readyOperations.Remove(topPrioriyOperation);
            return topPrioriyOperation;
        }
                
    }


    public class Route{
        public List<Node<RoutingInformation>> route;
        public readonly Droplet routedDroplet;
        public int startTime;

        public Route(List<Node<RoutingInformation>> route, Droplet routedDroplet, int startTime)
        {
            this.route = route;
            this.routedDroplet = routedDroplet;
            this.startTime = startTime;
        }

        public int getEndTime(){
            //Minus 1 to route.Count, as the initial position of the drop is included in the route.
            return startTime + (route.Count - 1) * Schedule.DROP_MOVEMENT_TIME;
        }

        public override String ToString()
        {
            String routeString = "StartTime = " + startTime + ", EndTime = " + getEndTime() + ". Route = [";
            for (int i = 0; i < route.Count; i++)
            {
                routeString += "(" + route[i].value.x + ", " + route[i].value.y + ")";
                if (i != route.Count - 1) routeString += ", ";
            }

            routeString += "]";
            return routeString;
            
        }
    }
}
