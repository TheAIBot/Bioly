using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiolyCompiler.Routing
{
    public class Router
    {
        public static int RouteDropletsToModule(Module operationExecutingModule, Board board, int currentTime, FluidBlock topPriorityOperation)
        {
            int originalStartTime = currentTime;
            foreach (var dropletInput in operationExecutingModule.GetInputLayout().Droplets)
            {
                Route route = RouteSingleDropletToModule(operationExecutingModule, board, currentTime, dropletInput);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                currentTime = route.getEndTime() + 1;
            }
            return currentTime;
        }

        private static Route RouteSingleDropletToModule(Module operationExecutingModule, Board board, int currentTime, Droplet dropletInput)
        {
            BoardFluid InputFluidType = dropletInput.GetFluidType();
            if (InputFluidType.GetNumberOfDropletsAvailable() < 1) throw new Exception("There isn't enough droplets of type " + InputFluidType.FluidName +
                                                                                       " avaiable, to satisfy the requirement of the module: " + operationExecutingModule.ToString());
            //Routes a droplet of type InputFluid to the module.
            Route route = DetermineRouteToModule(haveReachedDropletOfTargetType(dropletInput), operationExecutingModule, dropletInput, board, currentTime); //Will be included as part of a later step.
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
            //The droplet has been "used up"/it is now inside a module, 
            //so it needs to be removed from its original position:
            RemoveRoutedDropletFromBoard(board, route);

            return route;
        }

        private static void RemoveRoutedDropletFromBoard(Board board, Route route)
        {
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
        }

        public static Route DetermineRouteToModule(Func<Module, RoutingInformation, bool> hasReachedTarget, Module sourceModule, IDropletSource targetInput, Board board, int startTime)
        {
            //Dijkstras algorithm, based on the one seen on wikipedia.
            //Finds the route from the module to route to (source module), to the closest droplet of type targetFluidType,
            //and then inverts the route.
            (var dijkstraGraph, var priorityQueue) = SetUpInitialDijsktraGraph(targetInput, board);

            while (priorityQueue.Count > 0)
            {
                RoutingInformation currentNode = priorityQueue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.x, currentNode.y];

                if (isUnreachableNode(currentNode))
                    throw new Exception("No route to the desired component could be found. Desired droplet type: " + targetInput.GetFluidType().FluidName);
                else if (hasReachedTarget(moduleAtCurrentNode, currentNode)) //Have reached the desired target
                    return GetRouteFromSourceToTarget(currentNode, (IDropletSource)moduleAtCurrentNode, startTime);
                //No collisions with other modules are allowed (except the starting module):
                else if (hasCollisionWithOtherModules(sourceModule, moduleAtCurrentNode))
                    continue;

                //go through all neighbors
                UpdateAllNeighborPriorities(board, dijkstraGraph, priorityQueue, currentNode);

            }
            //If no route was found:
            throw new Exception("No route to the desired component could be found");
        }

        private static (RoutingInformation[,], SimplePriorityQueue<RoutingInformation, int>) SetUpInitialDijsktraGraph(IDropletSource targetInput, Board board)
        {
            RoutingInformation[,] dijkstraGraph = createDijkstraGraph(board);
            (int startingXPos, int startingYPos) = targetInput.GetMiddleOfSource();
            RoutingInformation source = dijkstraGraph[startingXPos, startingYPos];
            source.distanceFromSource = 0;

            SimplePriorityQueue<RoutingInformation, int> priorityQueue = new SimplePriorityQueue<RoutingInformation, int>();
            foreach (var node in dijkstraGraph)
            {
                priorityQueue.Enqueue(node, node.distanceFromSource);
            }
            return (dijkstraGraph, priorityQueue);
        }
        

        private static void UpdateAllNeighborPriorities(Board board, RoutingInformation[,] dijkstraGraph, SimplePriorityQueue<RoutingInformation, int> priorityQueue, RoutingInformation currentNode)
        {
            if (0 < currentNode.x)
                UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x - 1, currentNode.y]);
            if (0 < currentNode.y)
                UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x, currentNode.y - 1]);
            if (currentNode.x < board.width - 1)
                UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x + 1, currentNode.y]);
            if (currentNode.y < board.heigth - 1)
                UpdateNeighborPriority(priorityQueue, currentNode, dijkstraGraph[currentNode.x, currentNode.y + 1]);
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

        public delegate bool TargetFunction(Module moduleAtCurrentNode, RoutingInformation currentNode);

        public static Func<Module, RoutingInformation, bool> haveReachedSpecifficModule(Object targetModule) {
            return (moduleAtCurrentNode, location) => targetModule.Equals(moduleAtCurrentNode);
        }

        public static Func<Module, RoutingInformation, bool> haveReachedDropletOfTargetType(Droplet targetDroplet)
        {
            return (moduleAtCurrentNode, location) => moduleAtCurrentNode is IDropletSource dropletSource &&
                                                      targetDroplet        is IDropletSource dropletTarget &&
                                                      dropletSource.GetFluidType().Equals(dropletTarget.GetFluidType());
        }

        private static Route GetRouteFromSourceToTarget(RoutingInformation routeInfo, IDropletSource routedDroplet, int startTime)
        {
            (int dropletMiddleX, int dropletMiddleY) = routedDroplet.GetMiddleOfSource();
            //Currently, the route ends at the edges of the droplets location: it will need to be routed to the middle:
            while (dropletMiddleX - routeInfo.x != 0)
            {
                RoutingInformation nextPosition = new RoutingInformation(routeInfo.x + ((dropletMiddleX - routeInfo.x > 0) ? 1 : -1), routeInfo.y);
                nextPosition.previous = routeInfo;
                routeInfo = nextPosition;
            }
            while (dropletMiddleY - routeInfo.y != 0)
            {
                RoutingInformation nextPosition = new RoutingInformation(routeInfo.x, routeInfo.y + ((dropletMiddleY - routeInfo.y > 0) ? 1 : -1));
                nextPosition.previous = routeInfo;
                routeInfo = nextPosition;
            }
            //Now the droplet is at the middle.
            List<RoutingInformation> routeNodes = new List<RoutingInformation>();
            while (routeInfo.previous != null)
            {
                routeNodes.Add(routeInfo);
                routeInfo = routeInfo.previous;

            }
            routeNodes.Add(routeInfo);
            //routeNodes.Reverse();
            Route route = new Route(routeNodes, routedDroplet, startTime);
            return route;
        }

        public static Route RouteDropletToNewPosition(Module oldDropletPosition, Droplet newDropletPosition, Board board, int startTime)
        {
            Route route = DetermineRouteToModule(haveReachedSpecifficModule(oldDropletPosition), newDropletPosition, newDropletPosition, board, startTime);
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
    }
}
