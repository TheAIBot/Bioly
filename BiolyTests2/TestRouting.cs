using System;
using BiolyCompiler.BlocklyParts.Blocks;
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
//using MoreLinq;

namespace BiolyTests.RoutingTests
{
    [TestClass]
    public class TestRouting
    {
        [TestMethod]
        public void TestDetermineRouteToModuleNoObstacles()
        {
            Block operation = new Sensor(null, null, null);
            Module sourceModule = new SensorModule();
            Module targetModule = new SensorModule();
            sourceModule.shape.x = 0;
            sourceModule.shape.y = 0;
            targetModule.shape.x = 10;
            targetModule.shape.y = 10;
            operation.Bind(sourceModule);
            Board board = new Board(20,20);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.shape);
            board.UpdateGridWithModulePlacement(targetModule, targetModule.shape);
            
            int startTime = 55;
            Route route = Schedule.determineRouteToModule(sourceModule, targetModule, board, startTime);
            Assert.IsTrue(isAnActualRoute(route, board));
            Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Has detected collision while this shouldn't be possible");
            Assert.IsTrue(hasCorrectStartAndEnding(route, board, sourceModule, targetModule));
            Assert.AreEqual(route.getEndTime(), startTime + targetModule.shape.x + targetModule.shape.y);            
        }



        [TestMethod]
        public void TestDetermineRouteToModuleWithObstacles()
        {
            Module sourceModule = new SensorModule();
            Module targetModule = new SensorModule();
            Module blockingModule = new MixerModule(3,15,2000);
            sourceModule.shape.x = 0;
            sourceModule.shape.y = 0;
            targetModule.shape.x = 10;
            targetModule.shape.y = 10;
            blockingModule.shape.x = 5;
            blockingModule.shape.y = 0;
            Board board = new Board(20, 20);
            board.UpdateGridWithModulePlacement(sourceModule, sourceModule.shape);
            board.UpdateGridWithModulePlacement(targetModule, targetModule.shape);
            board.UpdateGridWithModulePlacement(blockingModule, blockingModule.shape);


            int startTime = 55;
            Route route = Schedule.determineRouteToModule(sourceModule, targetModule, board, startTime);
            Assert.IsTrue(isAnActualRoute(route, board));
            Assert.IsTrue(hasNoCollisions(route, board, sourceModule), "Obstacle not avoided: the path has a collisition");
            Assert.IsTrue(hasCorrectStartAndEnding(route, board, sourceModule, targetModule));
            //The manhatten distance to the target, is the lenght of the direct path to the target.
            //As the placed module should block the way somewhat, the path should be longer:
            Assert.IsTrue(route.getEndTime() > startTime + targetModule.shape.x + targetModule.shape.y);
        }


        [TestMethod]
        public void TestMultipleDropsToOneModuleRouting()
        {
            Assert.Fail("Not implemented yet.");
        }

        private bool hasNoCollisions(Route route, Board board, Module sourceModule)
        {
            //The last node is not counted, as it should hopefully be at a target module.
            for (int i = 0; i < route.route.Count - 1; i++)
            {
                Node<RoutingInformation> node = route.route[i];
                if (board.grid[node.value.x, node.value.y] != null && board.grid[node.value.x, node.value.y] != sourceModule) return false;
            }
            return true;
        }

        private bool hasCorrectStartAndEnding(Route route, Board board, Module sourceModule, Module targetModule)
        {
            RoutingInformation startOfPath = route.route[0].value;
            return  sourceModule.shape.x == startOfPath.x &&
                    sourceModule.shape.y == startOfPath.y &&
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
