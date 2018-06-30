using BiolyCompiler.Architechtures;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using MoreLinq;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler.Routing
{
    public class Router
    {
        private static Droplet FAKE_DROPLET = new Droplet(new BoardFluid("FAKE - DROPLET"));

        public static int RouteDropletsToModule(Board board, int currentTime, FluidBlock operation)
        {
            int originalStartTime = currentTime;
            //The order in which to route droplets to the module (where possible deadlocks are avoided)
            List<Droplet> internalDropletRoutingOrder = GetModulesDropletRoutingOrder(operation, board);

            foreach (var dropletInput in internalDropletRoutingOrder)
            {
                Route route = RouteSingleDropletToModule(operation, board, currentTime, dropletInput);
                //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                currentTime = route.getEndTime() + 1;
                board.UpdateGridAtGivenLocation(FAKE_DROPLET, dropletInput.Shape);
            }
            board.UpdateGridAtGivenLocation(operation.BoundModule, operation.BoundModule.Shape);
            return currentTime;
        }


        /// <summary>
        /// Routes droplets to the output bound to the operation. The number of droplets routed,
        /// is equal to. The routing starts at currentTime, and is based on the Board board given.
        /// 
        /// It is neccessary to seperate the routing to an output from the normal routing,
        /// as outputs requires that multiple droplets are routed to the exact same location on the module.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="currentTime"></param>
        /// <param name="outputOperation"></param>
        /// <returns></returns>
        public static int RouteDropletsToOutput(Board board, int currentTime, FluidBlock outputOperation, Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            int originalStartTime = currentTime;
            if (outputOperation.BoundModule.GetInputLayout().Droplets.Count == 0)
            {
                throw new RuntimeException("No droplets with name " + outputOperation.InputFluids.First().OriginalFluidName + " available to output.");
            }
            Droplet inputLocation = outputOperation.BoundModule.GetInputLayout().Droplets[0];
            foreach (var fluid in outputOperation.InputFluids)
            {
                int amountToTransfer = fluid.GetAmountInDroplets(FluidVariableLocations);
                for (int i = 0; i < amountToTransfer; i++)
                {
                    Route route = RouteSingleDropletToModule(outputOperation, board, currentTime, inputLocation);
                    //The route is scheduled sequentially, so the end time of the current route (+1) should be the start of the next.
                    //This will give an overhead of +1 for the operation starting time, for each droplet routed:
                    currentTime = route.getEndTime() + 1;
                }
            }
            return currentTime;
        }

        private static List<Droplet> GetModulesDropletRoutingOrder(FluidBlock operation, Board board)
        {
            List<Droplet> dropletRoutingOrder = new List<Droplet>();
            ModuleLayout inputLayout = operation.BoundModule.GetInputLayout();
            (var dropletInputs, var layoutEmptyRectangles) = GetDropletInputsAndInitiallyEmptyRectangles(inputLayout);
            HashSet<Rectangle> duplicateLayoutEmptyRectangles = new HashSet<Rectangle>(layoutEmptyRectangles);
            HashSet<Rectangle> outsideEmptyRectangle = operation.BoundModule.Shape.AdjacentRectangles
                                                       .Where(rectangle => rectangle.isEmpty)
                                                       .ToHashSet();

            foreach (var outsideRectangle in outsideEmptyRectangle) {
                foreach (var insideRectangle in layoutEmptyRectangles) {
                    outsideRectangle.ConnectIfAdjacent(insideRectangle);
                }
            }

            //A droplet needs to be routed to all interal droplets of the module.
            while (dropletInputs.Count > 0)
            {
                bool hasDropletBeenRouted = false;
                foreach (var dropletInput in dropletInputs)
                {
                    if (Board.DoesNotBlockConnectionToSourceEmptyRectangles(dropletInput, outsideEmptyRectangle, layoutEmptyRectangles))
                    {
                        hasDropletBeenRouted = true;
                        dropletInputs.Remove(dropletInput);
                        layoutEmptyRectangles.Remove(dropletInput.Shape);
                        dropletRoutingOrder.Add(dropletInput);
                        break;
                    }

                }
                if (!hasDropletBeenRouted)
                {
                    throw new InternalRuntimeException("It is not possible to route a droplet to every internal droplet inside a module. This should always be possible." +
                                        "The module is: " + operation.BoundModule.ToString());
                }
            }


            foreach (var outsideRectangle in outsideEmptyRectangle) {
                foreach (var insideRectangle in duplicateLayoutEmptyRectangles) {
                    outsideRectangle.AdjacentRectangles.Remove(insideRectangle);
                    insideRectangle.AdjacentRectangles.Remove(outsideRectangle);
                }
            }

            //And because if one has a 3x3 module with a droplet inside it, the code above completly disconnects it
            //from all its adjacent rectangles, it is necessary to connect it again:

            foreach (var outsideRectangle in outsideEmptyRectangle)
            {
                outsideRectangle.ConnectIfAdjacent(operation.BoundModule.Shape);
            }


            return dropletRoutingOrder;
        }
        

        private static (HashSet<Droplet>, HashSet<Rectangle>) GetDropletInputsAndInitiallyEmptyRectangles(ModuleLayout inputLayout)
        {
            HashSet<Droplet> dropletInputs = new HashSet<Droplet>(inputLayout.Droplets);
            HashSet<Rectangle> emptyRectangles = new HashSet<Rectangle>(inputLayout.EmptyRectangles);
            //Before any droplets have been placed, the internal layout is empty.
            dropletInputs.ForEach(droplet => emptyRectangles.Add(droplet.Shape));
            return (dropletInputs, emptyRectangles);
        }


        public static Route RouteSingleDropletToModule(Module module, Board board, int currentTime, Droplet dropletInput)
        {
            BoardFluid InputFluidType = dropletInput.GetFluidType();
            if (InputFluidType.GetNumberOfDropletsAvailable() < 1) throw new RuntimeException("There isn't enough droplets of type " + InputFluidType.FluidName +
                                                                                              " avaiable, to satisfy the requirement of the module: " + module.ToString());
            //Routes a droplet of type InputFluid to the module.
            Route route = DetermineRouteToModule(haveReachedDropletOfTargetType(dropletInput), module, dropletInput, board, currentTime); //Will be included as part of a later step.
            if (route == null) throw new InternalRuntimeException("No route found. This should not be possible.");
            
            //The droplet has been "used up"/it is now inside a module, 
            //so it needs to be removed from its original position:
            RemoveRoutedDropletFromBoard(board, route);
            return route;
        }

        public static Route RouteSingleDropletToModule(FluidBlock operation, Board board, int currentTime, Droplet dropletInput)
        {
            BoardFluid InputFluidType = dropletInput.GetFluidType();
            Route route = RouteSingleDropletToModule(operation.BoundModule, board, currentTime, dropletInput);

            //The route is added to the module's routes:
            List<Route> inputRoutes;
            operation.InputRoutes.TryGetValue(InputFluidType.FluidName, out inputRoutes);
            if (inputRoutes == null)
            {
                inputRoutes = new List<Route>();
                operation.InputRoutes.Add(InputFluidType.FluidName, inputRoutes);
            }
            inputRoutes.Add(route);
            return route;
        }

        private static void RemoveRoutedDropletFromBoard(Board board, Route route)
        {
            //The droplet routed is used by the module, and as such it can be removed from the board,
            //unless it comes from a spawner:
            switch (route.routedDroplet)
            {
                case Droplet dropletSource:
                    dropletSource.GetFluidType().dropletSources.Remove(dropletSource);
                    board.FastTemplateRemove(dropletSource);
                    break;
                case InputModule dropletSource:
                    if (1 < dropletSource.DropletCount) dropletSource.DecrementDropletCount();
                    else if (dropletSource.DropletCount == 1) {
                        dropletSource.GetFluidType().dropletSources.Remove(dropletSource);
                        dropletSource.DecrementDropletCount();
                        //board.FastTemplateRemove(dropletSource); 
                    }
                    else throw new InternalRuntimeException("The droplet spawner has a negative droplet count. Droplet source: " + dropletSource.ToString());
                    break;
                default:
                    throw new InternalRuntimeException("Unhandled droplet source: " + route.routedDroplet.ToString());
            }
        }

        public static Route DetermineRouteToModule(Func<Module, RoutingInformation, bool> hasReachedTarget, Module sourceModule, IDropletSource targetInput, Board board, int startTime)
        {
            //Dijkstras algorithm, based on the one seen on wikipedia.
            //Finds the route from the module to route to (source module), to the closest droplet of type targetFluidType,
            //and then inverts the route.
            (var visitedNodes, var queue) = SetUpInitialDijsktraGraph(targetInput, board);

            while (queue.Count > 0)
            {
                RoutingInformation currentNode = queue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.x, currentNode.y];

                if (isUnreachableNode(currentNode))
                    throw new InternalRuntimeException("No route to the desired component could be found. Desired droplet type: " + targetInput.GetFluidType().FluidName);
                else if (hasReachedTarget(moduleAtCurrentNode, currentNode)) //Have reached the desired target
                    return GetRouteFromSourceToTarget(currentNode, (IDropletSource)moduleAtCurrentNode, startTime);
                //No collisions with other modules are allowed (except the starting module):
                else if (hasCollisionWithOtherModules(sourceModule, moduleAtCurrentNode))
                    continue;

                //go through all neighbors
                UpdateAllNeighborPriorities(board, visitedNodes, queue, currentNode);
            }
            //If no route was found:
            throw new InternalRuntimeException("No route to the desired component could be found");
        }

        private static (Dictionary<RoutingInformation, RoutingInformation>, Queue<RoutingInformation>) SetUpInitialDijsktraGraph(IDropletSource targetInput, Board board)
        {
            //RoutingInformation[,] dijkstraGraph = createDijkstraGraph(board);
            (int startingXPos, int startingYPos) = targetInput.GetMiddleOfSource();
            //RoutingInformation source = dijkstraGraph[startingXPos, startingYPos];
            RoutingInformation source = new RoutingInformation(startingXPos, startingYPos);
            Dictionary<RoutingInformation, RoutingInformation> visistedNodes = new Dictionary<RoutingInformation, RoutingInformation>();
            visistedNodes.Add(source,source);
            source.distanceFromSource = 0;

            Queue<RoutingInformation> queue = new Queue<RoutingInformation>();
            queue.Enqueue(source);
            return (visistedNodes, queue);
        }
        

        private static void UpdateAllNeighborPriorities(Board board, Dictionary<RoutingInformation, RoutingInformation> visistedNodes, Queue<RoutingInformation> queue, RoutingInformation currentNode)
        {
            if (0 < currentNode.x)
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x - 1, currentNode.y);
            if (0 < currentNode.y)
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x, currentNode.y - 1);
            if (currentNode.x < board.width - 1)
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x + 1, currentNode.y);
            if (currentNode.y < board.heigth - 1)
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x, currentNode.y + 1);
        }

        private static void UpdateNeighborPriority(Queue<RoutingInformation> queue, RoutingInformation currentNode, Dictionary<RoutingInformation, RoutingInformation> visistedNodes, int neighborXPos, int neighborYPos)
        {
            RoutingInformation neighbor;
            RoutingInformation newNeighborNode = new RoutingInformation(neighborXPos, neighborYPos);
            if (visistedNodes.TryGetValue(newNeighborNode, out neighbor))
                return; // A shorter path to the node has already been found.
            else {
                neighbor = newNeighborNode;
                visistedNodes.Add(neighbor, neighbor);
            }
            //Unit lenght distances, and thus the distance is incremented with a +1.
            int distanceToNeighborFromCurrent = currentNode.distanceFromSource + 1;
            if (distanceToNeighborFromCurrent < neighbor.distanceFromSource)
            {
                neighbor.distanceFromSource = distanceToNeighborFromCurrent;
                neighbor.previous = currentNode;
                queue.Enqueue(neighbor);
            }
        }

        public List<Droplet> GetInternalDropletExstractionOrder(HeaterModule module)
        {
            List<Droplet> dropletRoutingOrder = new List<Droplet>();
            ModuleLayout outputLayout = module.GetOutputLayout();
            HashSet<Droplet> internalDroplets = outputLayout.Droplets.ToHashSet();
            HashSet<Rectangle> internalEmptyRectangles = outputLayout.EmptyRectangles.ToHashSet();
            HashSet<Rectangle> internalRectangles = new HashSet<Rectangle>(internalEmptyRectangles);
            internalEmptyRectangles.UnionWith(internalDroplets.Select(droplet => droplet.Shape));
            HashSet<Rectangle> outsideEmptyRectangle = module.Shape.AdjacentRectangles
                                                       .Where(rectangle => rectangle.isEmpty)
                                                       .ToHashSet();

            foreach (var externalRectangle in outsideEmptyRectangle)
                foreach (var internalRectangle in internalRectangles)
                    externalRectangle.ConnectIfAdjacent(internalRectangle);
            
            //Breadth first seach, starting from the outside empty rectangles, to find the correct order of extraction:
            


            foreach (var externalRectangle in outsideEmptyRectangle)
            {
                foreach (var internalRectangle in internalRectangles)
                {
                    externalRectangle.AdjacentRectangles.Remove(internalRectangle);
                    internalRectangle.AdjacentRectangles.Remove(externalRectangle);
                }
            }
            return null;
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
