using System;
using BiolyCompiler.Graphs;
using MoreLinq;
using BiolyCompiler.Modules;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts.Blocks;
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
        public Dictionary<int, Board> boardAtDifferentTimes = new Dictionary<int, Board>();
        public Dictionary<string, Droplet> FluidVariableLocations = new Dictionary<string, Droplet>();
        public SimplePriorityQueue<Block> runningOperations = new SimplePriorityQueue<Block>();
        public List<Block> ScheduledOperations = new List<Block>();
        private List<Module> allUsedModules = new List<Module>(); // For debuging. Used when printing the board to the console.
        public const int DROP_MOVEMENT_TIME = 1;
        public const int IGNORED_TIME_DIFFERENCE = 100;

        public Schedule(){

        }
        
        private static Board getCurrentBoard()
        {
            throw new NotImplementedException();
        }

        private int updateSchedule(Block operation, int startTime)
        {
            operation.startTime = startTime;
            operation.endTime   = operation.startTime + operation.boundModule.operationTime;
            runningOperations.Enqueue(operation, operation.endTime);
            ScheduledOperations.Add(operation);
            return operation.startTime;
        }

        private static void waitForAFinishedOperation()
        {
            throw new NotImplementedException();
        }

        public void TransferFluidVariableLocationInformation(Dictionary<string, Droplet> Locations)
        {
            Locations.ForEach(pair => FluidVariableLocations.Add(pair.Key, pair.Value));
        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public int ListScheduling(Assay assay, Board board, ModuleLibrary library){
            //(*) TODO sammenlign igen med algoritmen set i artiklen
            
            //Setup:
            int startTime = 0;
            assay.calculateCriticalPath();
            library.allocateModules(assay); 
            library.sortLibrary();
            allUsedModules.AddRange(board.placedModules);
            List<Block> readyOperations = assay.getReadyOperations();
            boardAtDifferentTimes.Add(startTime, board);
            Debug.WriteLine(board.print(allUsedModules));
            board = board.Copy();
            //Continue until all operations have been scheduled:
            while(assay.hasUnfinishedOperations() && canExecuteMoreOperations(readyOperations))
            {
                Block topPriorityOperation = removeOperation(readyOperations);
                Module operationExecutingModule = library.getAndPlaceFirstPlaceableModule(topPriorityOperation, board); //Also called place
                topPriorityOperation.Bind(operationExecutingModule);
                allUsedModules.Add(operationExecutingModule);

                Debug.WriteLine(board.print(allUsedModules));

                //If the module can't be placed, one must wait until there is enough space for it:
                if (operationExecutingModule == null)
                {
                    throw new Exception("Not enough space for a module: this is not handeled yet");
                    waitForAFinishedOperation();
                    if (runningOperations.Count == 0) throw new Exception("The scheduling can't be made: there aren't enough space for module: " + operationExecutingModule.ToString());
                }

                //Now all the droplet that the module should operate on, needs to be delivered to it.
                //By construction, there will be a route from the droplets to the module, 
                //and so it will always be possible for this routing to be done:
                startTime = RouteDropletsToModule(operationExecutingModule, board, startTime, topPriorityOperation);
                Debug.WriteLine(board.print(allUsedModules));

                runningOperations.ToList().OrderBy(element => element.startTime).ForEach(element => Debug.WriteLine(element.OutputVariable + ", " + element.startTime + ", " + element.endTime));

                //Note that it will also wait for operations to finish, 
                //in the case that there are no more operations that can be executed, before this happen:
                (startTime, board) = handleFinishingOperations(startTime, assay, board);
                readyOperations = assay.getReadyOperations();
                Debug.WriteLine(board.print(allUsedModules));
            }
            if (assay.hasUnfinishedOperations()) throw new Exception("There were operations that couldn't be scheduled.");
            ScheduledOperations.Sort((x,y) => (x.startTime < y.startTime || (x.startTime == y.startTime && x.endTime <= y.endTime)) ? 0: 1);
            return getCompletionTime();
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
                List<Block> nextBatchOfFinishedOperations = getNextBatchOfFinishedOperations();
                startTime = nextBatchOfFinishedOperations.Last().endTime + 1;
                foreach (var finishedOperation in nextBatchOfFinishedOperations)
                {
                    if (!finishedOperation.boundModule.isStaticModule())
                    {
                        Droplet replacingDroplet = board.replaceWithDroplets(finishedOperation);
                        FluidVariableLocations.Add(finishedOperation.OutputVariable, replacingDroplet);
                        allUsedModules.Add(replacingDroplet);
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
            return runningOperations.Count > 0 && (readyOperations.Count == 0  || startTime >= runningOperations.First().endTime);
        }

        private int RouteDropletsToModule(Module operationExecutingModule, Board board, int startTime, Block topPriorityOperation)
        {
            foreach (var Input in topPriorityOperation.InputVariables)
            {
                Droplet InputDroplet;
                bool doSourceExist = FluidVariableLocations.TryGetValue(Input, out InputDroplet);
                if (!doSourceExist) throw new Exception("The source \"" + Input + "\" for the operation \"" + topPriorityOperation.ToString() + "\" do not exist.");
                Route route = determineRouteToModule(InputDroplet, operationExecutingModule, board, startTime); //Will be included as part of a later step.
                if (route == null) throw new Exception("No route found. This should not be possible.");
                operationExecutingModule.InputRoutes.Add(Input, route);
                //One could move this (with some code change) up to before the module is placed.
                board.FastTemplateRemove(InputDroplet);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                startTime = route.getEndTime() + 1;
            }
            updateSchedule(topPriorityOperation, startTime);

            return startTime;
        }

        private List<Block> getNextBatchOfFinishedOperations()
        {
            List<Block> batch = new List<Block>();
            Block nextFinishedOperation = runningOperations.Dequeue();
            batch.Add(nextFinishedOperation);
            //Need to dequeue all operations that has finishes at the same time as nextFinishedOperation.
            //Differences under "IGNORED_TIME_DIFFERENCE" are ignored.
            while (runningOperations.Count > 0 && nextFinishedOperation.endTime >= runningOperations.First.endTime - IGNORED_TIME_DIFFERENCE)
            {
                batch.Add(runningOperations.Dequeue());
            }

            return batch;
        }

        public int getCompletionTime(){
            return ScheduledOperations.Max(operation => operation.endTime);
        }

        public static Route determineRouteToModule(Module sourceModule, Module targetModule, Board board, int startTime){

            //Dijkstras algorithm, based on the one seen on wikipedia.
            Node<RoutingInformation>[,] dijkstraGraph = createDijkstraGraph(board);
            Node<RoutingInformation> source = board.getOperationFluidPlacementOnBoard(sourceModule, dijkstraGraph);
            source.value.distanceFromSource = 0;
            SimplePriorityQueue<Node<RoutingInformation>, int> priorityQueue = new SimplePriorityQueue<Node<RoutingInformation>, int>();
            foreach (var node in dijkstraGraph) priorityQueue.Enqueue(node, node.value.distanceFromSource);


            while (priorityQueue.Count > 0)
            {

                Node<RoutingInformation> currentNode = priorityQueue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.value.x, currentNode.value.y];

                if (isUnreachableNode(currentNode)) throw new Exception("No route to the desired component could be found");
                else if (haveReachedTargetModule(targetModule, moduleAtCurrentNode)) return GetRouteFromSourceToTarget(currentNode, startTime); //Have reached the desired module
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

        private static bool haveReachedTargetModule(Module targetModule, Module moduleAtCurrentNode)
        {
            return moduleAtCurrentNode == targetModule;
        }

        private static Route GetRouteFromSourceToTarget(Node<RoutingInformation> currentNode, int startTime)
        {
            Route route = new Route();
            List<Node<RoutingInformation>> routeNodes = new List<Node<RoutingInformation>>();
            while(currentNode.value.previous != null)
            {
                routeNodes.Add(currentNode);
                currentNode = currentNode.value.previous;
            }
            routeNodes.Add(currentNode);
            routeNodes.Reverse();
            route.route = routeNodes;
            route.startTime = startTime;
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
        public int startTime;

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
