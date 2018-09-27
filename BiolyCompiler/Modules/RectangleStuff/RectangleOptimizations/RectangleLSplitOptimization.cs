using BiolyCompiler.Architechtures;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Modules.HelperObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations
{
    public static class RectangleLSplitOptimization
    {
        public static Rectangle[] SplitMerge(Board board, Rectangle rectangle)
        {
            foreach (var adjacentRectangle in rectangle.AdjacentRectangles)
            {
                //Can only L split two empty rectangles
                if (!board.EmptyRectangles.ContainsKey(adjacentRectangle))
                {
                    continue;
                }

                (var formsLSegment, var side, var extendDirection) = FormsLSegment(rectangle, adjacentRectangle);
                if (formsLSegment && IsLSplitWorthIt(rectangle, adjacentRectangle, side))
                {
                    Rectangle[] newRectangles = GetLShapeInformation(rectangle, adjacentRectangle, side, extendDirection);
                    Rectangle[] oldRectangles = new Rectangle[]
                    {
                        rectangle,
                        adjacentRectangle
                    };
                    Rectangle.ReplaceRectangles(oldRectangles, newRectangles);

                    oldRectangles.ForEach(x => board.EmptyRectangles.Remove(x));
                    newRectangles.ForEach(x => board.EmptyRectangles.Add(x, x));

                    return newRectangles;
                }
            }

            return null;
        }

        private static (bool, RectangleSide, RectangleSide) FormsLSegment(Rectangle rectangle, Rectangle adjacentRectangle)
        {
            //It forms an L segment, if for any of the corners of rectangle, adjacent rectangle "starts" there:
            //for example if the lower right corner of rectangle is at the same position of the lower left corner of adjacentRectangle.
            var rectangleEdges = rectangle.GetRectangleCorners();
            var adjacentEdges = adjacentRectangle.GetRectangleCorners();

            if (rectangleEdges.lowerRight == adjacentEdges.lowerLeft) return (true, RectangleSide.Right, RectangleSide.Top);   //It is to the right, and it extends upwards.
            else if (rectangleEdges.topRight == adjacentEdges.topLeft) return (true, RectangleSide.Right, RectangleSide.Bottom);//It is to the right, and it extends downwards.

            else if (rectangleEdges.topRight == adjacentEdges.lowerRight) return (true, RectangleSide.Top, RectangleSide.Left);
            else if (rectangleEdges.topLeft == adjacentEdges.lowerLeft) return (true, RectangleSide.Top, RectangleSide.Right);

            else if (rectangleEdges.topLeft == adjacentEdges.topRight) return (true, RectangleSide.Left, RectangleSide.Bottom);
            else if (rectangleEdges.lowerLeft == adjacentEdges.lowerRight) return (true, RectangleSide.Left, RectangleSide.Top);

            else if (rectangleEdges.lowerLeft == adjacentEdges.topLeft) return (true, RectangleSide.Bottom, RectangleSide.Right);
            else if (rectangleEdges.lowerRight == adjacentEdges.topRight) return (true, RectangleSide.Bottom, RectangleSide.Left);

            else return (false, RectangleSide.None, RectangleSide.None);
        }

        private static bool IsLSplitWorthIt(Rectangle first, Rectangle second, RectangleSide side)
        {
            int horizontalSegment;
            int verticalSegment;
            bool isAlreadyOptimallySplit;

            //Check if splitting even improves the rectangles
            if (side.IsVertical())
            {
                if (first.width < second.width)
                {
                    horizontalSegment = first.width;
                    verticalSegment = second.height;
                }
                else
                {
                    horizontalSegment = second.width;
                    verticalSegment = first.height;
                }
                isAlreadyOptimallySplit = Rectangle.ShouldSplitAtHorizontalLineSegment(verticalSegment, horizontalSegment);
            }
            else if (side.IsHorizontal())
            {
                if (first.height < second.height)
                {
                    horizontalSegment = second.width;
                    verticalSegment = first.height;
                }
                else
                {
                    horizontalSegment = first.width;
                    verticalSegment = second.height;
                }
                isAlreadyOptimallySplit = !Rectangle.ShouldSplitAtHorizontalLineSegment(verticalSegment, horizontalSegment);
            }
            else
            {
                throw new InternalRuntimeException("Logic error.");
            }

            bool doesSplitMatter = verticalSegment != horizontalSegment;
            return doesSplitMatter && !isAlreadyOptimallySplit;
        }

        private static Rectangle[] GetLShapeInformation(Rectangle first, Rectangle second, RectangleSide side, RectangleSide extendDirection)
        {
            Rectangle smaller;
            Rectangle bigger;
            if (side.IsVertical() && first.width < second.width ||
                side.IsHorizontal() && first.height < second.height)
            {
                smaller = first;
                bigger = second;
            }
            else
            {
                smaller = second;
                bigger = first;
                side = side.OppositeDirection();
            }


            if (side.IsVertical())
            {
                int movedXPos = extendDirection == RectangleSide.Right ? smaller.width : 0;
                int movedYPos = side == RectangleSide.Bottom ? bigger.height : 0;
                Rectangle newSmaller = new Rectangle(smaller.width, smaller.height + bigger.height, smaller.x, smaller.y - movedYPos);
                Rectangle newBigger = new Rectangle(bigger.width - smaller.width, bigger.height, bigger.x + movedXPos, bigger.y);
                return new Rectangle[] { newSmaller, newBigger };
            }
            else
            {
                int movedXPos = side == RectangleSide.Left ? bigger.width : 0;
                int movedYPos = extendDirection == RectangleSide.Top ? smaller.height : 0;
                Rectangle newSmaller = new Rectangle(smaller.width + bigger.width, smaller.height, smaller.x - movedXPos, smaller.y);
                Rectangle newBigger = new Rectangle(bigger.width, bigger.height - smaller.height, bigger.x, bigger.y + movedYPos);
                return new Rectangle[] { newSmaller, newBigger };
            }
        }
    }
}
