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
