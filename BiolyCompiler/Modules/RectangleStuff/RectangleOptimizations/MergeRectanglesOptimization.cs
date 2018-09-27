using BiolyCompiler.Architechtures;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Modules.HelperObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations
{
    public static class MergeRectanglesOptimization
    {
        public static Rectangle TryMergeOptimization(Board board, Rectangle rectangle)
        {
            Rectangle bestMerge = null;
            int bestScore = 0;
            RectangleSide bestMergeSide = RectangleSide.None;

            //First find the best pair of rectangles to merge if there is any
            foreach (Rectangle candidate in rectangle.AdjacentRectangles)
            {
                //Can only merge two empty rectangles
                if (!board.EmptyRectangles.ContainsKey(candidate))
                {
                    continue;
                }

                RectangleSide mergeSide = GetMergeSideIfAny(rectangle, candidate);
                if (mergeSide != RectangleSide.None)
                {
                    int score = rectangle.GetArea() + candidate.GetArea();
                    if (score > bestScore)
                    {
                        bestMerge = candidate;
                        bestScore = score;
                        bestMergeSide = mergeSide;
                    }
                }
            }

            //If a pair was found then merge them
            if (bestMerge != null)
            {
                Rectangle mergedRectangle = MergeRectangles(rectangle, bestMerge, bestMergeSide);

                //Remove old rectangles and add the new rectangle to the board
                board.EmptyRectangles.Remove(rectangle);
                board.EmptyRectangles.Remove(bestMerge);
                board.EmptyRectangles.Add(mergedRectangle, mergedRectangle);

                return mergedRectangle;
            }

            return null;
        }

        private static Rectangle MergeRectangles(Rectangle first, Rectangle second, RectangleSide side)
        {
            Rectangle mergedRectangle;
            switch (side)
            {
                case RectangleSide.Left:
                    mergedRectangle = new Rectangle(first.width + second.width, first.height, second.x, first.y);
                    break;
                case RectangleSide.Right:
                    mergedRectangle = new Rectangle(first.width + second.width, first.height, first.x, first.y);
                    break;
                case RectangleSide.Top:
                    mergedRectangle = new Rectangle(first.width, first.height + second.height, first.x, first.y);
                    break;
                case RectangleSide.Bottom:
                    mergedRectangle = new Rectangle(first.width, first.height + second.height, first.x, second.y);
                    break;
                default:
                    throw new InternalRuntimeException("A rectangle can only be joined on the sides left, right, top or bottom, not " + side.ToString());
            }

            Rectangle[] oldRectangles = new Rectangle[]
            {
                first,
                second
            };
            Rectangle.ReplaceRectangles(oldRectangles, mergedRectangle);

            return mergedRectangle;
        }

        public static RectangleSide GetMergeSideIfAny(Rectangle first, Rectangle second)
        {
            //They can merge if the rectangles line up on a side. They can only line up on one side.
            //Line up means that they are right next to each other and that they have the same side length
            if (first.width == second.width &&
                first.x == second.x)
            {
                //Below
                if (first.y == second.getTopmostYPosition() + 1)
                {
                    return RectangleSide.Bottom;
                }
                //Above
                else if (first.getTopmostYPosition() + 1 == second.y)
                {
                    return RectangleSide.Top;
                }
            }
            else if (first.height == second.height &&
                     first.y == second.y)
            {
                //Left
                if (first.x == second.getRightmostXPosition() + 1)
                {
                    return RectangleSide.Left;
                }
                //Right
                else if (first.getRightmostXPosition() + 1 == second.x)
                {
                    return RectangleSide.Right;
                }
            }

            return RectangleSide.None;
        }
    }
}
