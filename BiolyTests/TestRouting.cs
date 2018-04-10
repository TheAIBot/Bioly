using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
using BiolyCompiler.Routing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyTests.TestObjects;
using BiolyCompiler.BlocklyParts.Misc;
//using MoreLinq;

namespace BiolyTests.RoutingTests
{
    [TestClass]
    public class TestRouting
    {
        [TestMethod]
        public void TestDetermineRouteToModuleNoObstacles()
        {
            FluidBlock operation = new TestBlock(new List<string>(), null, new TestModule());
            Module  sourceModule = new TestModule();
            BoardFluid fluidType = new BoardFluid("test");
            Droplet droplet = new Droplet(fluidType);
            sourceModule.Shape.x = 0;
            sourceModule.Shape.y = 0;
            droplet.Shape.x = 10;
            droplet.Shape.y = 10;
            operation.Bind(sourceModule);
            Board board = new Board(20,20);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet, droplet.Shape);
            
            int startTime = 55;
            Route route = Schedule.DetermineRouteToModule(fluidType, sourceModule, board, startTime);
            Assert.IsTrue(isAnActualRoute(route, board));
            Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Has detected collision while this shouldn't be possible");
            Assert.IsTrue(hasCorrectStartAndEnding(route, board, sourceModule, droplet));
            Assert.AreEqual(route.getEndTime(), startTime + droplet.Shape.x + droplet.Shape.y);            
        }



        [TestMethod]
        public void TestDetermineRouteToModuleWithObstacles()
        {
            Module sourceModule = new TestModule();
            BoardFluid fluidType = new BoardFluid("test");
            Module droplet = new Droplet(fluidType);
            Module blockingModule = new TestModule(3,15,2000);
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
            Route route = Schedule.DetermineRouteToModule(fluidType, sourceModule, board, startTime);
            Assert.IsTrue(isAnActualRoute(route, board));
            Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Obstacle not avoided: the path has a collisition");
            Assert.IsTrue(hasCorrectStartAndEnding(route, board, sourceModule, droplet));
            //The manhatten distance to the target, is the lenght of the direct path to the target.
            //As the placed module should block the way somewhat, the path should be longer:
            Assert.IsTrue(route.getEndTime() > startTime + droplet.Shape.x + droplet.Shape.y);
        }


        [TestMethod]
        public void TestMultipleDropsSameTypeToOneModuleRouting()
        {
            Schedule schedule = new Schedule();

            Module sourceModule = new TestModule();
            BoardFluid fluidType1 = new BoardFluid("test1");
            Module droplet1 = new Droplet(fluidType1);
            Module droplet2 = new Droplet(fluidType1);
            Module droplet3 = new Droplet(fluidType1);
            sourceModule.Shape.x = 0;
            sourceModule.Shape.y = 0;
            droplet1.Shape.x = 10;
            droplet1.Shape.y = 10;
            droplet2.Shape.x =  0;
            droplet2.Shape.y = 10;
            droplet3.Shape.x = 10;
            droplet3.Shape.y =  0;
            Board board = new Board(20, 20);
            Dictionary<string, BoardFluid> kage = new Dictionary<string, BoardFluid>();
            kage.Add("test1", fluidType1);
            schedule.TransferFluidVariableLocationInformation(kage);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);
            board.UpdateGridWithModulePlacement(droplet1, droplet1.Shape);
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            TestBlock testBlock = new TestBlock(new List<FluidInput>() { new FluidInput(fluidType1.FluidName, 2, false) }, null, null);
            testBlock.Bind(sourceModule);
            
            int startTime = 55;
            int endtime = schedule.RouteDropletsToModule(sourceModule, board, startTime, testBlock);
            Assert.AreEqual(1, sourceModule.InputRoutes.Count);
            Assert.IsTrue(sourceModule.InputRoutes.TryGetValue(fluidType1.FluidName, out List<Route> routes));
            Assert.AreEqual(2, routes.Count);
            //Droplet 2 and 3 should have been routed to the module, as droplet 1 is further away.
            Assert.IsFalse(routes.Select(route => route.routedDroplet).Contains(droplet1 as Droplet));
            Assert.IsTrue( routes.Select(route => route.routedDroplet).Contains(droplet2 as Droplet));
            Assert.IsTrue( routes.Select(route => route.routedDroplet).Contains(droplet3 as Droplet));
            Assert.IsTrue(routes[0].startTime        == startTime);
            Assert.IsTrue(routes[0].getEndTime() + 1 == routes[1].startTime);
            Assert.IsTrue(routes[1].getEndTime() + 1 == endtime);
            Assert.AreEqual(10 + 1 + 10 + 1 + startTime, endtime);
            //The modules will be placed again, to check hasCorrectStartAndEnding:
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            board.UpdateGridWithModulePlacement(droplet3, droplet3.Shape);
            Assert.IsTrue(hasCorrectStartAndEnding(routes[0], board, sourceModule, droplet2));
            Assert.IsTrue(hasCorrectStartAndEnding(routes[1], board, sourceModule, droplet3));
            for (int i = 0; i < routes.Count; i++)
            {
                Route route = routes[i];
                Assert.IsTrue(isAnActualRoute(route, board));
                Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Obstacle not avoided: the path has a collisition");
            }
        }

        [TestMethod]
        public void TestMultipleDropsDifferentTypeToOneModuleRouting()
        {
            Schedule schedule = new Schedule();

            Module sourceModule = new TestModule();
            BoardFluid fluidType1 = new BoardFluid("test1");
            BoardFluid fluidType2 = new BoardFluid("test2");
            Module droplet1 = new Droplet(fluidType1);
            Module droplet2 = new Droplet(fluidType1);
            Module droplet3 = new Droplet(fluidType2);
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
            TestBlock testBlock = new TestBlock(new List<FluidInput>() { new FluidInput(fluidType1.FluidName, 2, false) }, null, null);
            testBlock.Bind(sourceModule);

            int startTime = 55;
            int endtime = schedule.RouteDropletsToModule(sourceModule, board, startTime, testBlock);
            Assert.AreEqual(1, sourceModule.InputRoutes.Count);
            Assert.IsTrue(sourceModule.InputRoutes.TryGetValue(fluidType1.FluidName, out List<Route> routes));
            Assert.AreEqual(2, routes.Count);
            //Droplet 1 and 2 should have been routed to the module, as they are of the correct type.
            Assert.IsTrue(routes.Select(route => route.routedDroplet).Contains(droplet1 as Droplet));
            Assert.IsTrue(routes.Select(route => route.routedDroplet).Contains(droplet2 as Droplet));
            Assert.IsFalse(routes.Select(route => route.routedDroplet).Contains(droplet3 as Droplet));
            Assert.IsTrue(routes[0].startTime == startTime);
            Assert.IsTrue(routes[0].getEndTime() + 1 == routes[1].startTime);
            Assert.IsTrue(routes[1].getEndTime() + 1 == endtime);
            Assert.AreEqual(20 + 1 + 10 + 1 + startTime, endtime);
            //The modules will be placed again, to check hasCorrectStartAndEnding:
            board.UpdateGridWithModulePlacement(droplet1, droplet1.Shape);
            board.UpdateGridWithModulePlacement(droplet2, droplet2.Shape);
            Assert.IsTrue(hasCorrectStartAndEnding(routes[0], board, sourceModule, droplet2)); //Droplet 2 is close, and should therefore be routed first.
            Assert.IsTrue(hasCorrectStartAndEnding(routes[1], board, sourceModule, droplet1));
            for (int i = 0; i < routes.Count; i++)
            {
                Route route = routes[i];
                Assert.IsTrue(isAnActualRoute(route, board));
                //It will have a collision, as when routing droplet 1 to the source, droplet 2 will already have been moved.
                //Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Obstacle not avoided: the path has a collisition");
            }
        }


        [TestMethod]
        public void TestRoutingFromDropletSpawner()
        {
            Board board = new Board(20, 20);
            FluidBlock operation = new TestBlock(new List<string>(), null, new TestModule());
            Module sourceModule = new TestModule();
            BoardFluid fluidType = new BoardFluid("test");
            int capacity = 5;
            DropletSpawner dropletSpawner = new DropletSpawner(fluidType, capacity);
            board.FastTemplatePlace(dropletSpawner);
            Assert.AreEqual(0, dropletSpawner.Shape.x);
            Assert.AreEqual(0, dropletSpawner.Shape.y);
            sourceModule.Shape.x = 10;
            sourceModule.Shape.y = 10;
            operation.Bind(sourceModule);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.Shape);

            int startTime = 55;
            for (int i = 0; i < capacity; i++)
            {
                //The droplet spawner should still be placed on the board, as it hasn't been emtied.
                Assert.AreEqual (capacity - i, dropletSpawner.DropletCount);
                Assert.AreEqual(2, board.placedModules.Count);
                Assert.IsTrue(dropletSpawner == board.placedModules.ToList()[0]); //It should be the same spawner, even by reference
                //The route should be correct.
                sourceModule.InputRoutes = new Dictionary<string, List<Route>>(); //To avoid an exception when adding the fluidtype to the dictionary, in the method below.
                Schedule.RouteGivenNumberOfDropletsOfGivenType(sourceModule, board, startTime, fluidType, 1);
                Assert.AreEqual(1, sourceModule.InputRoutes.Count);
                Assert.AreEqual(1, sourceModule.InputRoutes[fluidType.FluidName].Count);
                Route route = sourceModule.InputRoutes[fluidType.FluidName][0];
                Assert.IsTrue(isAnActualRoute(route, board));
                Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Has detected collision while this shouldn't be possible");
                Assert.IsTrue(i == capacity - 1 || hasCorrectStartAndEnding(route, board, sourceModule, dropletSpawner));
                Assert.AreEqual(startTime + sourceModule.Shape.x + sourceModule.Shape.y - (dropletSpawner.Shape.height - 1) - (dropletSpawner.Shape.width - 1), route.getEndTime());
            } 

            //The droplet spawner should be empty, and it shouldn't be placed on the board any longer.
            Assert.AreEqual(0, dropletSpawner.DropletCount);
            Assert.AreEqual(1, board.placedModules.Count);
            Assert.IsTrue(board.placedModules.ToList()[0] == sourceModule);
        }

        private bool hasNoCollisions(Route route, Board board, Module sourceModule)
        {
            //The last node is not counted, as it should hopefully be at a target module.
            for (int i = 0; i < route.route.Count - 1; i++)
            {
                Node<RoutingInformation> node = route.route[i];
                if (board.grid[node.value.x, node.value.y] != null && board.grid[node.value.x, node.value.y] != sourceModule)
                {
                    return false;
                }
            }
            return true;
        }

        private bool hasCorrectStartAndEnding(Route route, Board board, Module sourceModule, Module targetModule)
        {
            RoutingInformation startOfPath = route.route[0].value;
            return  sourceModule.Shape.x == startOfPath.x &&
                    sourceModule.Shape.y == startOfPath.y &&
                    targetModule == board.grid[route.route.Last().value.x, route.route.Last().value.y];
        }

        private bool isAnActualRoute(Route route, Board board)
        {
            if (!isPlacedOnTheBoard(route.route[0].value.x, route.route[0].value.y, board)) return false;
            for (int i = 1; i < route.route.Count; i++)
            {
                RoutingInformation priorPlacement = route.route[i-1].value;
                RoutingInformation currentPlacement = route.route[i].value;
                if (!isPlacedOnTheBoard(currentPlacement.x, currentPlacement.y, board))
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

        private bool isPlacedOnTheBoard(int x, int y, Board board)
        {
            return (0 <= x && x < board.width &&
                    0 <= y && y < board.heigth);
        }


    }
}
