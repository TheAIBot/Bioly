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
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Schedule
    {
        Dictionary<int, Board> boardAtDifferentTimes = new Dictionary<int, Board>();
        Dictionary<string, Droplet> FluidVariableLocations = new Dictionary<string, Droplet>();
        SimplePriorityQueue<Block> runningOperations = new SimplePriorityQueue<Block>();
        List<Block> ScheduledOperations = new List<Block>();
        public const int DROP_MOVEMENT_TIME = 1;
        public const int IGNORED_TIME_DIFFERENCE = 100;

        public Schedule(){

        }


        public static Tuple<int, Schedule> SequentialScheduling(Assay assay, Architechture architecture, ModuleLibrary library)
        {
            Board board = new Board(architecture.width, architecture.heigth);
            assay.calculateCriticalPath();
            library.allocateModules(assay);
            library.sortLibrary();
            List<Block> readyOperations = assay.getReadyOperations();
            while(readyOperations.Count > 0)
            {
                Block operation = removeOperation(readyOperations);
                Module module = library.getOptimalModule(operation);
                board.removeAllDroplets();
                bool canBePlaced = board.sequentiallyPlace(module);
                if (!canBePlaced)
                {
                    throw new Exception("Not enough space for module: " + module.ToString() + 
                                        ", on board: " + board.ToString());
                }
                bool canDropletsBeStored = board.placeAllDroplets();
                if (!canDropletsBeStored)
                {
                    throw new Exception("Not enough space for the droplets, and for the module: " + module.ToString() +
                                        ", on board: " + board.ToString());
                }
                operation.Bind(module);
                //updateSchedule(operation);
                waitForAFinishedOperation();

                board = getCurrentBoard();
                assay.updateReadyOperations(operation);
                readyOperations = assay.getReadyOperations();
                
                //It routes after placement of component.

            }

            return null;
        }

        private static Board getCurrentBoard()
        {
            throw new NotImplementedException();
        }

        private int updateSchedule(Block operation, Route routeToOperationModule)
        {
            operation.startTime = routeToOperationModule.getEndTime();
            operation.endTime = operation.startTime + operation.boundModule.operationTime;
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
            
            //Place the modules that are fixed on the board, 
            //so that the dynamic algorithm doesn't have to handle this.
            //PlaceFixedModules(); //TODO implement.

            //(*) TODO sammenlign igen med algoritmen set i artiklen
            
            //Many optimization can be made to this method (later)
            int startTime = 0;
            assay.calculateCriticalPath();
            library.allocateModules(assay); 
            library.sortLibrary();
            List<Block> readyOperations = assay.getReadyOperations();
            boardAtDifferentTimes.Add(startTime, board);
            board = board.Copy();
            //Continue until all operations have been scheduled:
            while(readyOperations.Count > 0){
                Block operation = removeOperation(readyOperations);
                Module module   = library.getAndPlaceFirstPlaceableModule(operation, board); //Also called place
                //If the module can't be placed, one must wait until there is enough space for it:
                if (module != null){
                    operation.Bind(module); 
                    Droplet sourceModule;
                    bool doSourceExist = FluidVariableLocations.TryGetValue(operation.InputVariables[0], out sourceModule);
                    if (!doSourceExist) throw new Exception("The source \"" + operation.InputVariables[0] + "\" for the operation \"" + operation.ToString() + "\" do not exist.");
                    Route route   = determineRouteToModule(sourceModule, module, board, startTime); //Will be included as part of a later step.
                    //TODO (*) If it can't be routed
                    if (route == null) {
                        operation.Unbind(module);
                        waitForAFinishedOperation();
                        if (runningOperations.Count == 0) throw new Exception("The scheduling can't be made: the routing can't be made.");
                    } else{
                        startTime = updateSchedule(operation, route);
                        if (!sourceModule.isStaticModule()) {
                            board.FastTemplateRemove(sourceModule);
                        }
                        board = board.Copy();
                    }
                }
                else {
                    waitForAFinishedOperation();
                    if (runningOperations.Count == 0) throw new Exception("The scheduling can't be made: there aren't enough space for module: " + module.ToString());
                }



                /*(*)TODO fix edge case, where the drops are routed/operations are scheduled, 
                 * so that in the mean time, some operations finishes. This might lead to routing problems.
                */
                //If all the operations that can currently be scheduled, have been scheduled, the board needs to be saved.
                if (readyOperations.Count == 0 && runningOperations.Count > 0)
                {
                    boardAtDifferentTimes.Add(startTime, board);
                }

                //If there aren't any operations that can be started, wait until there are:
                while (readyOperations.Count == 0 && runningOperations.Count > 0)
                {
                    List<Block> nextBatchOfFinishedOperations = getNextBatchOfFinishedOperations();
                    startTime = nextBatchOfFinishedOperations.Last().endTime + 1;
                    foreach (var finishedOperation in nextBatchOfFinishedOperations)
                    {
                        if (!finishedOperation.boundModule.isStaticModule())
                        {
                            board.replaceWithDroplets(finishedOperation);
                        }
                        assay.updateReadyOperations(finishedOperation);
                    }
                    boardAtDifferentTimes.Add(startTime, board);
                    readyOperations = assay.getReadyOperations();
                    board = board.Copy();
                }
            }
            if (assay.dfg.Nodes.Exists(node => !node.value.hasBeenScheduled)) throw new Exception("There were operations that couldn't be scheduled.");
            ScheduledOperations.Sort((x,y) => (x.startTime < y.startTime || (x.startTime == y.startTime && x.endTime <= y.endTime)) ? 0: 1);
            return getCompletionTime();
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

        private static int GetRunningOperationsCount()
        {
            throw new NotImplementedException();
        }

        private static int updateSchedule(Block operation, Schedule schedule)
        {
            throw new NotImplementedException();
        }

        public int getCompletionTime(){
            return 0;
        }

        public static void updateReadyOperations(Assay assay, int newTime, List<Block> readyOperations){
            
        }

        public static Route determineRouteToModule(Module sourceModule, Module targetModule, Board board, int startTime){

            //Dijkstras algorithm, based on the one seen on wikipedia.
            Node<RoutingInformation>[,] dijkstraGraph = createDijkstraGraph(board);
            Node<RoutingInformation> source = board.getOperationFluidPlacementOnBoard(sourceModule, dijkstraGraph);
            source.value.distanceFromSource = 0;
            SimplePriorityQueue<Node<RoutingInformation>, int> priorityQueue = new SimplePriorityQueue<Node<RoutingInformation>, int>();
            foreach (var node in dijkstraGraph) priorityQueue.Enqueue(node, node.value.distanceFromSource);
            while (priorityQueue.Count > 0) {
                Node<RoutingInformation> currentNode = priorityQueue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.value.x, currentNode.value.y];
                if (currentNode.value.distanceFromSource == Int32.MaxValue) throw new Exception("No route to the desired component could be found");
                else if (moduleAtCurrentNode == targetModule) return GetRouteFromSourceToTarget(currentNode, startTime); //Have reached the desired module
                //No collisions with other modules are allowed (except the starting module):
                else if (moduleAtCurrentNode != null && moduleAtCurrentNode != sourceModule) continue;
                foreach (var neighbor in currentNode.getOutgoingEdges())
                {
                    //Unit lenght distances, and thus the distance is with a +1.
                    int distanceToNeighborFromCurrent = currentNode.value.distanceFromSource + 1; 
                    if (distanceToNeighborFromCurrent < neighbor.value.distanceFromSource )
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

        public static int updateSchedule(Block operation, Schedule schedule, Route route){
            return 0;
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
