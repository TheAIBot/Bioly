using System;
using BiolyCompiler.Graphs;
using MoreLinq;
using BiolyCompiler.Modules;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts.Blocks;
using BiolyCompiler.Routing;
using Priority_Queue;
//using BiolyCompiler.Modules.ModuleLibrary;

namespace BiolyCompiler.Scheduling
{

    public class Schedule
    {
        Dictionary<int, Module[][]> boardAtDifferentTimes = new Dictionary<int, Module[][]>();
        public const int DROP_MOVEMENT_TIME = 1;

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
                updateSchedule(operation);
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

        private int updateSchedule(Block operation)
        {
            throw new NotImplementedException();
        }

        private static void waitForAFinishedOperation()
        {
            throw new NotImplementedException();
        }

        /**
            Implements/based on the list scheduling based algorithm found in 
            "Fault-tolerant digital microfluidic biochips - compilation and synthesis" page 72.
         */
        public Tuple<int, Schedule> ListScheduling(Assay assay, Board board, ModuleLibrary library){
            
            //Place the modules that are fixed on the board, 
            //so that the dynamic algorithm doesn't have to handle this.
            //PlaceFixedModules(); //TODO implement.
            
            //Many optimization can be made to this method (later)
            assay.calculateCriticalPath();
            library.allocateModules(assay); 
            library.sortLibrary();
            List<Block> readyOperations = assay.getReadyOperations();
            int startTime = 0;
            while(readyOperations.Count > 0){
                Block operation = removeOperation(readyOperations);
                Module module   = library.getAndPlaceFirstPlaceableModule(operation, board); //Also called place
                //If the module can't be placed, one must wait until there is enough place for it.
                if (module != null){
                    operation.Bind(module); //TODO make modules unique
                    Route route   = determineRouteToModule(operation, module, board, startTime); //Will be included as part of a later step.
                    //TODO (*) If it can't be routed
                    if (route == null)
                    {
                        operation.Unbind(module);
                        waitForAFinishedOperation();
                        module = null;
                    }
                    else startTime = updateSchedule(operation); 
                }
                if (module == null)
                {
                    waitForAFinishedOperation();
                    if (GetRunningOperationsCount() == 0) throw new Exception("The scheduling can't be made: either there aren't enough space for module: " + module.ToString() +
                                                                              ", or the routing can't be made.");
                }

                board = getCurrentBoard();
                updateReadyOperations(assay, startTime, readyOperations);
                readyOperations = assay.getReadyOperations();
            }
            return Tuple.Create(getCompletionTime(), this);
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

        public static Route determineRouteToModule(Block operation, Module targetModule, Board board, int startTime){

            //Dijkstras algorithm, based on the one seen on wikipedia.
            Node<RoutingInformation>[,] dijkstraGraph = createDijkstraGraph(board);
            Node<RoutingInformation> source = board.getOperationFluidPlacementOnBoard(operation, dijkstraGraph);
            source.value.distanceFromSource = 0;
            SimplePriorityQueue<Node<RoutingInformation>, int> priorityQueue = new SimplePriorityQueue<Node<RoutingInformation>, int>();
            foreach (var node in dijkstraGraph) priorityQueue.Enqueue(node, node.value.distanceFromSource);
            while (priorityQueue.Count > 0) {
                Node<RoutingInformation> currentNode = priorityQueue.Dequeue();
                if (currentNode.value.distanceFromSource == Int32.MaxValue) throw new Exception("No route to the desired component could be found");
                else if (board.grid[currentNode.value.x, currentNode.value.y] == targetModule) return GetRouteFromSourceToTarget(currentNode, startTime); //Have reached the desired module
                //No collisions with other modules are allowed:
                else if (board.grid[currentNode.value.x, currentNode.value.y] != null) continue;
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

        public String ToString()
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
