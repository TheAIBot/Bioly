using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Routing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyTests.TestObjects;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.FluidicInputs;
//using MoreLinq;

namespace BiolyTests.RoutingTests
{
    [TestClass]
    public class TestRouting
    {
        [TestMethod]
        public void TestDetermineRouteToModuleNoObstacles()
        {
            FluidBlock operation = new TestBlock(new List<FluidBlock>(), null, new TestModule());
            Module sourceModule = new TestModule();
            BoardFluid fluidType = new BoardFluid("test");
            Droplet droplet = new Droplet(fluidType);
            Droplet inputLocation = new Droplet(fluidType);
            inputLocation.Shape.x = 0;
            inputLocation.Shape.y = 0;
            sourceModule.Shape.x = 0;
            sourceModule.Shape.y = 0;
            droplet.Shape.x = 10;
            droplet.Shape.y = 10;
            operation.Bind(sourceModule, null);
            Board board = new Board(20, 20);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet, droplet.Shape);
            //inputLocation shouldn't be placed, as it "inside" the module.

            int startTime = 55;
            Route route = Router.DetermineRouteToModule(Router.haveReachedDropletOfTargetType(inputLocation), sourceModule, inputLocation, board, startTime);
            Assert.IsTrue(IsAnActualRoute(route, board));
            Assert.IsTrue(HasNoCollisions(route, board, sourceModule, droplet), "Has detected collision while this shouldn't be possible");
            Assert.IsTrue(HasCorrectStartAndEnding(route, board, droplet, inputLocation));
            Assert.AreEqual(droplet.Shape.x + droplet.Shape.y + 1, route.route.Count);
            Assert.AreEqual(startTime + (droplet.Shape.x + droplet.Shape.y)*2 + 1, route.getEndTime());
        }



        [TestMethod]
        public void TestDetermineRouteToModuleWithObstacles()
        {
            Module sourceModule = new TestModule();
            BoardFluid fluidType = new BoardFluid("test");
            Droplet droplet = new Droplet(fluidType);
            Module blockingModule = new TestModule(3, 15, 2000);
            Droplet inputLocation = new Droplet(fluidType);
            inputLocation.Shape.x = 0;
            inputLocation.Shape.y = 0;
            sourceModule.Shape.x = 0;
            sourceModule.Shape.y = 0;
            droplet.Shape.x = 10;
            droplet.Shape.y = 10;
            blockingModule.Shape.x = 5;
            blockingModule.Shape.y = 0;
            Board board = new Board(20, 20);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet, droplet.Shape);
            board.UpdateGridWithModulePlacement(blockingModule, blockingModule.Shape);


            int startTime = 55;
            Route route = Router.DetermineRouteToModule(Router.haveReachedDropletOfTargetType(inputLocation), sourceModule, inputLocation, board, startTime);
            Assert.IsTrue(IsAnActualRoute(route, board));
            Assert.IsTrue(HasNoCollisions(route, board, sourceModule, droplet), "Obstacle not avoided: the path has a collisition");
            Assert.IsTrue(HasCorrectStartAndEnding(route, board, droplet, inputLocation));
            //The manhatten distance to the target, is the lenght of the direct path to the target.
            //As the placed module should block the way somewhat, the path should be longer:
            Assert.IsTrue(route.getEndTime() > startTime + droplet.Shape.x + droplet.Shape.y);
        }


        [TestMethod]
        public void TestMultipleDropsSameTypeToOneModuleRouting()
        {
            Schedule schedule = new Schedule();

            Module sourceModule = new TestModule(2, 0);
            BoardFluid fluidType1 = new BoardFluid("test1");
            sourceModule.GetInputLayout().ChangeFluidType(fluidType1);
            Droplet droplet1 = new Droplet(fluidType1);
            Droplet droplet2 = new Droplet(fluidType1);
            Droplet droplet3 = new Droplet(fluidType1);
            sourceModule.Shape.x = 0 ; sourceModule.Shape.y = 0;

            droplet1.Shape.x     = 10; droplet1.Shape.y     = 10;

            droplet2.Shape.x     = 0 ; droplet2.Shape.y     = 10;

            droplet3.Shape.x     = 10; droplet3.Shape.y     = 0;

            Board board = new Board(20, 20);
            Dictionary<string, BoardFluid> kage = new Dictionary<string, BoardFluid>();
            kage.Add("test1", fluidType1);
            schedule.TransferFluidVariableLocationInformation(kage);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet1, droplet1.Shape);
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            TestBlock testOperation = new TestBlock(new List<FluidInput>() { new BasicInput("",  fluidType1.FluidName, fluidType1.FluidName, 2, false) }, null, null);
            testOperation.Bind(sourceModule, null);
            sourceModule.RepositionLayout();
            //A fake empty rectangle is added to the adjacent rectangles of sourceModule,
            //as the routing requires at least one adjacent empty rectangle: this clearly exists in this case:
            Rectangle fakeRectangle = new Rectangle(1, 1, 4, 3);
            Assert.IsTrue(sourceModule.Shape.ConnectIfAdjacent(fakeRectangle));

            int startTime = 55;
            int endtime = Router.RouteDropletsToModule(board, startTime, testOperation);
            Assert.AreEqual(1, testOperation.InputRoutes.Count);
            Assert.IsTrue(testOperation.InputRoutes.TryGetValue(fluidType1.FluidName, out List<Route> routes));
            Assert.AreEqual(2, routes.Count);
            //Droplet 2 and 3 should have been routed to the module, as droplet 1 is further away
            Assert.IsFalse(routes.Select(route => route.routedDroplet).Contains(droplet1 as Droplet));
            Assert.IsTrue( routes.Select(route => route.routedDroplet).Contains(droplet2 as Droplet));
            Assert.IsTrue( routes.Select(route => route.routedDroplet).Contains(droplet3 as Droplet));
            Assert.IsTrue(routes[0].startTime        == startTime);
            Assert.IsTrue(routes[0].getEndTime() + 1 == routes[1].startTime);
            Assert.IsTrue(routes[1].getEndTime() + 1 == endtime);
            Assert.AreEqual(((droplet3.Shape.x)*2 + 1) + 1 + ((droplet2.Shape.y + 3)*2 + 1) + 1 + startTime, endtime);

            //The modules will be placed again, to check hasCorrectStartAndEnding:
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            Assert.IsTrue(HasCorrectStartAndEnding(routes[0], board, droplet3, sourceModule.GetInputLayout().Droplets[0]));
            Assert.IsTrue(HasCorrectStartAndEnding(routes[1], board, droplet2, sourceModule.GetInputLayout().Droplets[1]));
            Assert.IsTrue(IsAnActualRoute(routes[0], board));
            Assert.IsTrue(HasNoCollisions(routes[0], board, sourceModule, droplet3), "Obstacle not avoided: the path has a collisition");
            Assert.IsTrue(IsAnActualRoute(routes[1], board));
            Assert.IsTrue(HasNoCollisions(routes[1], board, sourceModule, droplet2), "Obstacle not avoided: the path has a collisition");
        }

        [TestMethod]
        public void TestMultipleDropsDifferentTypeOneBoardWithOneTypeToModuleRouting()
        {
            Schedule schedule = new Schedule();

            Module sourceModule = new TestModule(2, 0);
            BoardFluid fluidType1 = new BoardFluid("test1");
            BoardFluid fluidType2 = new BoardFluid("test2");
            Droplet droplet1 = new Droplet(fluidType1);
            Droplet droplet2 = new Droplet(fluidType1);
            Droplet droplet3 = new Droplet(fluidType2);
            sourceModule.Shape.x = 0;
            sourceModule.Shape.y = 0;
            droplet1.Shape.x = 10;
            droplet1.Shape.y = 10;
            droplet2.Shape.x = 0;
            droplet2.Shape.y = 10;
            droplet3.Shape.x = 10;
            droplet3.Shape.y = 0;
            Board board = new Board(20, 20);
            Dictionary<string, BoardFluid> kage = new Dictionary<string, BoardFluid>();
            kage.Add("test1", fluidType1);
            kage.Add("test2", fluidType2);
            schedule.TransferFluidVariableLocationInformation(kage);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet1, droplet1.Shape);
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            TestBlock testOperation = new TestBlock(new List<FluidInput>() { new BasicInput("", fluidType1.FluidName, fluidType1.FluidName, 1, false) , new BasicInput("", fluidType2.FluidName, fluidType2.FluidName, 1, false) }, null, null);
            testOperation.Bind(sourceModule, null);
            sourceModule.RepositionLayout();
            //A fake empty rectangle is added to the adjacent rectangles of sourceModule,
            //as the routing requires at least one adjacent empty rectangle: this clearly exists in this case:
            Rectangle fakeRectangle = new Rectangle(1, 1, 4, 3);
            Assert.IsTrue(sourceModule.Shape.ConnectIfAdjacent(fakeRectangle));

            int startTime = 55;
            int endtime = Router.RouteDropletsToModule(board, startTime, testOperation);
            Assert.AreEqual(2, testOperation.InputRoutes.Count);
            Assert.IsTrue(testOperation.InputRoutes.TryGetValue(fluidType1.FluidName, out List<Route> routes1));
            Assert.AreEqual(1, routes1.Count);
            Assert.IsTrue(testOperation.InputRoutes.TryGetValue(fluidType2.FluidName, out List<Route> routes2));
            Assert.AreEqual(1, routes2.Count);
            //Droplet 1 and 2 should have been routed to the module, as they are of the correct type.
            Assert.IsTrue(routes1.Select(route => route.routedDroplet).Contains(droplet2 as Droplet));
            Assert.IsTrue(routes2.Select(route => route.routedDroplet).Contains(droplet3 as Droplet));
            Assert.IsTrue(routes1[0].startTime == startTime);
            Assert.IsTrue(routes1[0].getEndTime() + 1 == routes2[0].startTime);
            Assert.IsTrue(routes2[0].getEndTime() + 1 == endtime);
            Assert.AreEqual((10 + 1 + 7 + 1)*2 + startTime, endtime);
            //The modules will be placed again, to check hasCorrectStartAndEnding:
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            Assert.IsTrue(HasCorrectStartAndEnding(routes1[0], board, droplet2, sourceModule.GetInputLayout().Droplets[0])); //Droplet 2 is close, and should therefore be routed first.
            Assert.IsTrue(HasCorrectStartAndEnding(routes2[0], board, droplet3, sourceModule.GetInputLayout().Droplets[1]));
            Assert.IsTrue(IsAnActualRoute(routes1[0], board));
            Assert.IsTrue(IsAnActualRoute(routes2[0], board));
        }


        [TestMethod]
        public  void TestRoutingFromDropletSpawner()
        {
            Board board = new Board(20, 20);
            FluidBlock operation = new TestBlock(new List<FluidBlock>(), null, new TestModule());
            int capacity = 5;
            BoardFluid fluidType = new BoardFluid("test");
            Module sourceModule = new TestModule(capacity - 2,0);
            sourceModule.GetInputLayout().ChangeFluidType(fluidType);
            InputModule dropletSpawner = new InputModule(fluidType, capacity);
            board.FastTemplatePlace(dropletSpawner);
            Assert.AreEqual(0, dropletSpawner.Shape.x);
            Assert.AreEqual(0, dropletSpawner.Shape.y);
            sourceModule.Shape.x = 10;
            sourceModule.Shape.y = 10;
            operation.Bind(sourceModule, null);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            sourceModule.RepositionLayout();
            //A fake empty rectangle is added to the adjacent rectangles of sourceModule,
            //as the routing requires at least one adjacent empty rectangle: this clearly exists in this case:
            Rectangle fakeRectangle = new Rectangle(1, 1, 17, 9);
            Assert.IsTrue(sourceModule.Shape.ConnectIfAdjacent(fakeRectangle));

            int startTime = 55;
            Schedule schedule = new Schedule();
            int endTime = Router.RouteDropletsToModule(board, startTime, operation);
            Assert.AreEqual(2, dropletSpawner.DropletCount);
            Assert.AreEqual(2, board.PlacedModules.Count);
            Assert.AreEqual(1, operation.InputRoutes.Count);
            Assert.AreEqual(capacity - 2, operation.InputRoutes[fluidType.FluidName].Count);
            for (int i = 0; i < operation.InputRoutes[fluidType.FluidName].Count; i++)
            {
                Route route = operation.InputRoutes[fluidType.FluidName][i];
                Assert.IsTrue(IsAnActualRoute(route, board));
                Assert.IsTrue(HasNoCollisions(route, board, sourceModule, dropletSpawner), "Has detected collision while this shouldn't be possible");
                Assert.IsTrue(HasCorrectStartAndEnding(route, board, dropletSpawner, sourceModule.GetInputLayout().Droplets[i]));
            }
        }

        [TestMethod]
        public void TestTwoModulesRoute()
        {
            int[] boardArray =
            {
                -1, -1, -2, -2,
                -1, -1, -2, -2,
                -1, -1, -2, -2,
                -1, -1, -2, -2,
                -1, -1, -2, -2,
            };

            CheckAllRoutes(boardArray, 4);
        }

        [TestMethod]
        public void TestTwoModulesRouteWithSeperator()
        {
            int[] boardArray =
            {
                -1, -1, 1, -2, -2,
                -1, -1, 1, -2, -2,
                -1, -1, 1, -2, -2,
                -1, -1, 1, -2, -2,
                -1, -1, 1, -2, -2,
            };

            CheckAllRoutes(boardArray, 5);
        }

        [TestMethod]
        public void TestSimpleRoute()
        {
            int[] boardArray =
            {
                -1, -1, -2, -2, -2, -3, -3,
                -1, -1, -2, -2, -2, -3, -3,
                -1, -1,  1,  1,  1, -3, -3,
                -1, -1, -4, -4, -4, -3, -3,
                -1, -1, -4, -4, -4, -3, -3,
            };

            CheckAllRoutes(boardArray, 7);
        }

        [TestMethod]
        public void TestBranchRoute()
        {
            int[] boardArray =
            {
                -1, -1, -1, 1, 1, -3, -3, -3, -3, 3, 3, -4, -4,
                -1, -1, -1, 1, 1, -3, -3, -3, -3, 3, 3, -4, -4,
                -2, -2, -2, 1, 1, -3, -3, -3, -3, 3, 3, -5, -5,
                -2, -2, -2, 2, 2,  2,  2,  2,  2, 2, 2, -5, -5,
                -2, -2, -2, 2, 2,  2,  2,  2,  2, 2, 2, -5, -5,
                -2, -2, -2, 4, 4, -7, -7, -7, -7, 5, 5, -5, -5,
                -6, -6, -6, 4, 4, -7, -7, -7, -7, 5, 5, -8, -8,
                -6, -6, -6, 4, 4, -7, -7, -7, -7, 5, 5, -8, -8
            };

            CheckAllRoutes(boardArray, 13);
        }

        private static void CheckAllRoutes(int[] boardArray, int boardWidth)
        {
            var boardData = RectangleTestTools.ArrayToRectangles(boardArray, boardWidth, "random fluid name");

            foreach (var startData in boardData.modules)
            {
                foreach (var endData in boardData.modules)
                {
                    if (startData.Item1 == endData.Item1)
                    {
                        continue;
                    }

                    CheckRoute(boardData, boardWidth, startData.Item2, endData.Item2);
                }
            }
        }

        private static void CheckRoute((Board board, List<(Rectangle, int)> rectangles, List<(Module, int)> modules) boardData, int boardWidth, int startModuleID, int endModuleID)
        {
            Module startModule = boardData.modules.Single(x => x.Item2 == startModuleID).Item1;
            Module endModule   = boardData.modules.Single(x => x.Item2 == endModuleID).Item1;

            Route route = Router.DetermineRouteToModule(Router.haveReachedSpecifficModule(endModule), startModule, (IDropletSource)startModule, boardData.board, 10);

            string errorMessage = RouteOnBoard(boardData.rectangles.Select(x => x.Item1).ToList(), boardData.board.width, boardData.board.heigth, route);
            Assert.IsTrue(route.route.Count > 0);
            Assert.IsTrue(HasNoCollisions(route, boardData.board, startModule, (IDropletSource)endModule), errorMessage);
            Assert.IsTrue(HasCorrectStartAndEnding(route, boardData.board, (IDropletSource)endModule, (IDropletSource)startModule), errorMessage);
            Assert.IsTrue(IsAnActualRoute(route, boardData.board), errorMessage);
        }

        private static string RouteOnBoard(List<Rectangle> rectangles, int width, int height, Route route)
        {
            int[][] map = RectangleTestTools.RectangleIntMap(rectangles, width, height);
            string[][] stringMap = map.Select(x => x.Select(y => String.Format("{0,2}", y)).ToArray()).ToArray();

            foreach (var position in route.route)
            {
                stringMap[position.y][position.x] = " #";
            }

            return Environment.NewLine + String.Join(Environment.NewLine, stringMap.Select(x => String.Join(", ", x)));
        }

        public static bool HasNoCollisions(Route route, Board board, Module sourceModule, IDropletSource targetDroplet)
        {
            for (int i = 0; i < route.route.Count; i++)
            {
                RoutingInformation node = route.route[i];
                if (board.grid[node.x, node.y] != null && 
                    board.grid[node.x, node.y] != sourceModule && 
                    board.grid[node.x, node.y] != targetDroplet)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool HasCorrectStartAndEnding(Route route, Board board, IDropletSource source, IDropletSource inputLocation)
        {
            RoutingInformation startOfPath = route.route[0];
            (int sourceX, int sourceY) = source.GetMiddleOfSource();
            (int inputX , int inputY ) = inputLocation.GetMiddleOfSource();
            return  sourceX == startOfPath.x &&
                    sourceY == startOfPath.y &&
                    route.route.Last().x == inputX &&
                    route.route.Last().y == inputY;
        }

        private static bool IsAnActualRoute(Route route, Board board)
        {
            if (!IsPlacedOnTheBoard(route.route[0].x, route.route[0].y, board)) return false;
            for (int i = 1; i < route.route.Count; i++)
            {
                RoutingInformation priorPlacement = route.route[i - 1];
                RoutingInformation currentPlacement = route.route[i];
                if (!IsPlacedOnTheBoard(currentPlacement.x, currentPlacement.y, board))
                {
                    return false;
                }
                //The current place on the route must adjacent to the place just before it:
                if (Math.Abs(currentPlacement.y - priorPlacement.y) + Math.Abs(currentPlacement.x - priorPlacement.x) != 1)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsPlacedOnTheBoard(int x, int y, Board board)
        {
            return (0 <= x && x < board.width &&
                    0 <= y && y < board.heigth);
        }


    }
}
