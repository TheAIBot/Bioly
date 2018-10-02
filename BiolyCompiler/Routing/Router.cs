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

        public static Route DetermineRouteToModule(Func<Module, bool> hasReachedTarget, Module sourceModule, IDropletSource targetInput, Board board, int startTime)
        {
            //Dijkstras algorithm, based on the one seen on wikipedia.
            //Finds the route from the module to route to (source module), to the closest droplet of type targetFluidType,
            //and then inverts the route.
            (int startingXPos, int startingYPos) = targetInput.GetMiddleOfSource();
            RouteDirection[,] routeMap = new RouteDirection[board.width, board.heigth];
            routeMap[startingXPos, startingYPos] = RouteDirection.Start;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(startingXPos, startingYPos));

            while (queue.Count > 0)
            {
                Point currentNode = queue.Dequeue();
                Module moduleAtCurrentNode = board.grid[currentNode.X, currentNode.Y];

                if (hasReachedTarget(moduleAtCurrentNode)) //Have reached the desired target
                {
                    return GetRouteFromSourceToTarget(currentNode, routeMap, (IDropletSource)moduleAtCurrentNode, startTime);
                }
                //No collisions with other modules are allowed (except the starting module):
                else if (hasCollisionWithOtherModules(sourceModule, moduleAtCurrentNode))
                {
                    continue;
                }

                //go through all neighbors
                UpdateAllNeighborPriorities(board, routeMap, queue, currentNode);
            }
            //If no route was found:
            throw new InternalRuntimeException("No route to the desired component could be found");
        }
        

        private static void UpdateAllNeighborPriorities(Board board, RouteDirection[,] routeMap, Queue<Point> queue, Point currentPos)
        {
            if (0 < currentPos.X)
            {
                UpdateNeighborPriority(queue, routeMap, currentPos.X - 1, currentPos.Y, RouteDirection.Right);
            }
            if (0 < currentPos.Y)
            {
                UpdateNeighborPriority(queue, routeMap, currentPos.X, currentPos.Y - 1, RouteDirection.Up);
            }
            if (currentPos.X < board.width - 1)
            {
                UpdateNeighborPriority(queue, routeMap, currentPos.X + 1, currentPos.Y, RouteDirection.Left);
            }
            if (currentPos.Y < board.heigth - 1)
            {
                UpdateNeighborPriority(queue, routeMap, currentPos.X, currentPos.Y + 1, RouteDirection.Down);
            }
        }

        private static void UpdateNeighborPriority(Queue<Point> queue, RouteDirection[,] routeMap, int neighborXPos, int neighborYPos, RouteDirection directionToCurrentPos)
        {
            if (routeMap[neighborXPos, neighborYPos] != RouteDirection.None)
            {
                return; // A shorter path to the node has already been found.
            }

            routeMap[neighborXPos, neighborYPos] = directionToCurrentPos;
            queue.Enqueue(new Point(neighborXPos, neighborYPos));
        }

        private static bool hasCollisionWithOtherModules(Module sourceModule, Module moduleAtCurrentNode)
        {
            return !(moduleAtCurrentNode == null || moduleAtCurrentNode == sourceModule);
        }

        public static Func<Module, bool> haveReachedSpecifficModule(Object targetModule) {
            return (moduleAtCurrentNode) => targetModule.Equals(moduleAtCurrentNode);
        }

        public static Func<Module, bool> haveReachedDropletOfTargetType(Droplet targetDroplet)
        {
            return (moduleAtCurrentNode) => moduleAtCurrentNode is IDropletSource dropletSource &&
                                            targetDroplet       is IDropletSource dropletTarget &&
                                            dropletSource.GetFluidType().Equals(dropletTarget.GetFluidType());
        }

        private static Route GetRouteFromSourceToTarget(Point routeInfo, RouteDirection[,] routeMap, IDropletSource routedDroplet, int startTime)
        {
            (int dropletMiddleX, int dropletMiddleY) = routedDroplet.GetMiddleOfSource();

            int movementsToCenter = Math.Abs(dropletMiddleX - routeInfo.X) + Math.Abs(dropletMiddleY - routeInfo.Y);
            Point[] toDropletCenter = new Point[movementsToCenter];
            int toCenterIndex = 0;

            RouteDirection xDirection = (dropletMiddleX - routeInfo.X > 0) ? RouteDirection.Right : RouteDirection.Left;
            RouteDirection yDirection = (dropletMiddleY - routeInfo.Y > 0) ? RouteDirection.Up : RouteDirection.Down;

            Point currentPos = routeInfo;
            while (dropletMiddleX - currentPos.X != 0)
            {
                currentPos = currentPos.Move(xDirection);
                toDropletCenter[toCenterIndex++] = currentPos;
            }
            while (dropletMiddleY - currentPos.Y != 0)
            {
                currentPos = currentPos.Move(yDirection);
                toDropletCenter[toCenterIndex++] = currentPos;
            }

            List<Point> dropletToStart = new List<Point>();
            currentPos = routeInfo;
            do
            {
                dropletToStart.Add(currentPos);
                currentPos = currentPos.Move(routeMap[currentPos.X, currentPos.Y]);
            } while (routeMap[currentPos.X, currentPos.Y] != RouteDirection.Start);
            dropletToStart.Add(currentPos);

            Point[] wholeRoute = new Point[dropletToStart.Count + toDropletCenter.Length];
            Array.Reverse(toDropletCenter);
            Array.Copy(toDropletCenter, 0, wholeRoute, 0, toDropletCenter.Length);
            dropletToStart.CopyTo(wholeRoute, toDropletCenter.Length);

            Route route = new Route(wholeRoute, routedDroplet, startTime);
            return route;
        }

        public static Route RouteDropletToNewPosition(Module oldDropletPosition, Droplet newDropletPosition, Board board, int startTime)
        {
            Route route = DetermineRouteToModule(haveReachedSpecifficModule(oldDropletPosition), newDropletPosition, newDropletPosition, board, startTime);
            return route;
        }
    }
}
