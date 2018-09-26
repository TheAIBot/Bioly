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
            Droplet[] internalDropletRoutingOrder = GetModulesDropletRoutingOrder(operation.BoundModule, board);

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

        private static Droplet[] GetModulesDropletRoutingOrder(Module module, Board board)
        {
            ModuleLayout inputLayout = module.GetInputLayout();
            HashSet<Droplet> dropletInputs = new HashSet<Droplet>(inputLayout.Droplets);
            Rectangle[] layoutRectangles = inputLayout.GetAllRectanglesIncludingDroplets();
            //Also contains rectangles for droplets but in this case they are considered
            //empty rectangles because they aren't filled with droplet yet
            HashSet<Rectangle> layoutEmptyRectangles = layoutRectangles.ToHashSet();
            HashSet<Rectangle> outsideEmptyRectangle = module.Shape.AdjacentRectangles
                                                       .Where(rectangle => rectangle.isEmpty)
                                                       .ToHashSet();

            Rectangle.ReplaceRectangles(module.Shape, layoutRectangles);

            Droplet[] dropletRoutingOrder = new Droplet[dropletInputs.Count];
            int dropletIndex = 0;

            //A droplet needs to be routed to all interal droplets of the module.
            while (dropletInputs.Count > 0)
            {
                bool hasDropletBeenRouted = false;
                foreach (var dropletInput in dropletInputs)
                {
                    if (Board.DoesNotBlockConnectionToSourceEmptyRectangles(dropletInput, outsideEmptyRectangle, layoutEmptyRectangles))
                    {
                        hasDropletBeenRouted = true;
                        dropletRoutingOrder[dropletIndex++] = dropletInput;
                        dropletInputs.Remove(dropletInput);
                        //A droplet has now been placed in that module so
                        //it's rectangle is no longer empty
                        layoutEmptyRectangles.Remove(dropletInput.Shape);
                        break;
                    }

                }
                if (!hasDropletBeenRouted)
                {
                    throw new InternalRuntimeException("It is not possible to route a droplet to every internal droplet inside a module. This should always be possible." +
                                        "The module is: " + module.ToString());
                }
            }

            Rectangle.ReplaceRectangles(layoutRectangles, module.Shape);

            return dropletRoutingOrder;
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
                    else if (dropletSource.DropletCount == 1)
                    {
                        dropletSource.GetFluidType().dropletSources.Remove(dropletSource);
                        dropletSource.DecrementDropletCount();
                        //board.FastTemplateRemove(dropletSource); 
                    }
                    else
                    {
                        throw new InternalRuntimeException("The droplet spawner has a negative droplet count. Droplet source: " + dropletSource.GetFluidType().FluidName);
                    }
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
            (int startingXPos, int startingYPos) = targetInput.GetMiddleOfSource();
            bool[,] visitedNodes = new bool[board.width, board.heigth];
            RoutingInformation source = new RoutingInformation(startingXPos, startingYPos, null, 0);
            visitedNodes[startingXPos, startingYPos] = true;

            Queue<RoutingInformation> queue = new Queue<RoutingInformation>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                RoutingInformation currentNode = queue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.x, currentNode.y];

                if (isUnreachableNode(currentNode))
                {
                    throw new InternalRuntimeException("No route to the desired component could be found. Desired droplet type: " + targetInput.GetFluidType().FluidName);
                }
                else if (hasReachedTarget(moduleAtCurrentNode, currentNode)) //Have reached the desired target
                {
                    return GetRouteFromSourceToTarget(currentNode, (IDropletSource)moduleAtCurrentNode, startTime);
                }
                //No collisions with other modules are allowed (except the starting module):
                else if (hasCollisionWithOtherModules(sourceModule, moduleAtCurrentNode))
                {
                    continue;
                }

                //go through all neighbors
                UpdateAllNeighborPriorities(board, visitedNodes, queue, currentNode);
            }
            //If no route was found:
            throw new InternalRuntimeException("No route to the desired component could be found");
        }
        

        private static void UpdateAllNeighborPriorities(Board board, bool[,] visistedNodes, Queue<RoutingInformation> queue, RoutingInformation currentNode)
        {
            if (0 < currentNode.x)
            {
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x - 1, currentNode.y);
            }
            if (0 < currentNode.y)
            {
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x, currentNode.y - 1);
            }
            if (currentNode.x < board.width - 1)
            {
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x + 1, currentNode.y);
            }
            if (currentNode.y < board.heigth - 1)
            {
                UpdateNeighborPriority(queue, currentNode, visistedNodes, currentNode.x, currentNode.y + 1);
            }
        }

        private static void UpdateNeighborPriority(Queue<RoutingInformation> queue, RoutingInformation currentNode, bool[,] visistedNodes, int neighborXPos, int neighborYPos)
        {
            if (visistedNodes[neighborXPos, neighborYPos])
            {
                return; // A shorter path to the node has already been found.
            }

            RoutingInformation neighbor = new RoutingInformation(neighborXPos, neighborYPos, currentNode, currentNode.distanceFromSource + 1);
            visistedNodes[neighborXPos, neighborYPos] = true;
            queue.Enqueue(neighbor);
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
                int newX = routeInfo.x + ((dropletMiddleX - routeInfo.x > 0) ? 1 : -1);
                routeInfo = new RoutingInformation(newX, routeInfo.y, routeInfo, 0);
            }
            while (dropletMiddleY - routeInfo.y != 0)
            {
                int newY = routeInfo.y + ((dropletMiddleY - routeInfo.y > 0) ? 1 : -1);
                routeInfo = new RoutingInformation(routeInfo.x, newY, routeInfo, 0);
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
    }
}
