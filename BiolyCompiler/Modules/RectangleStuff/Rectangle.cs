using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Modules.HelperObjects;
using BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations;

namespace BiolyCompiler.Modules
{
    public class Rectangle
    {
        public int height;
        public int width;
        public int x;
        public int y; //Coordinates for lower left corner.
        //Used by the FTP algorithm for deleting rectangles.
        public HashSet<Rectangle> AdjacentRectangles = new HashSet<Rectangle>();
        public bool isEmpty = true;


        public Rectangle(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new InternalRuntimeException("A rectangle must have a non-negative height and width: (width, height)=(" + width + ", " + height + ") is not allowed.");
            }
            this.height = height;
            this.width = width;
        }

        public Rectangle(int width, int height, int x, int y) : this(width, height)
        {
            PlaceAt(x, y);
        }

        public Rectangle(Rectangle rectangle) : this(rectangle.width, rectangle.height, rectangle.x, rectangle.y)
        {
            isEmpty = rectangle.isEmpty;
        }

        public void PlaceAt(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool DoesRectangleFitInside(Rectangle rectangle)
        {
            return rectangle.height <= this.height && rectangle.width <= this.width;
        }

        public int GetArea()
        {
            return height * width;
        }

        public bool ConnectIfAdjacent(Rectangle insideRectangle)
        {
            if (this.IsAdjacent(insideRectangle))
            {
                this.AdjacentRectangles.Add(insideRectangle);
                insideRectangle.AdjacentRectangles.Add(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ReplaceRectangles(Rectangle[] toReplace, Rectangle replaceWith)
        {
            ReplaceRectangles(toReplace, new Rectangle[] { replaceWith });
        }
        public static void ReplaceRectangles(Rectangle toReplace, Rectangle[] replaceWith)
        {
            ReplaceRectangles(new Rectangle[] { toReplace }, replaceWith);
        }
        public static void ReplaceRectangles(Rectangle[] toReplace, Rectangle[] replaceWith)
        {
            //Collect all the connections and try them on the new rectangles
            HashSet<Rectangle> allConnections = new HashSet<Rectangle>();
            toReplace.ForEach(x => allConnections.UnionWith(x.AdjacentRectangles));

            //Can't have references to the old rectangles as they are not in use anymore
            toReplace.ForEach(x => allConnections.Remove(x));

            //Some of the new rectangles will lie next to each other
            // and they should ofcourse also be connected
            replaceWith.ForEach(x => allConnections.Add(x));

            //Now remove the old rectangles connections
            toReplace.ForEach(x => x.Disconnect());

            //Now for each new rectangle try the old rectangles connections.
            //As all the rectangles habitate the same area they must have some of the connection
            //in common
            replaceWith.ForEach(x => x.Connect(allConnections));
        }

        public void Disconnect()
        {
            foreach (Rectangle adjacent in AdjacentRectangles)
            {
                adjacent.AdjacentRectangles.Remove(this);
            }
            AdjacentRectangles.Clear();
        }

        public void Connect(ICollection<Rectangle> connectTo)
        {
            foreach (Rectangle potentialConnection in connectTo)
            {
                if (potentialConnection.IsAdjacent(this))
                {
                    this.AdjacentRectangles.Add(potentialConnection);
                    potentialConnection.AdjacentRectangles.Add(this);
                }
            }
        }


        /// <summary>
        /// Given a module, the rectangle is split up into three rectangles, 
        /// with one of the rectangles being the one associated with the module. 
        /// The module is placed in the lower left corner. Other than that, there are a top rectangle directly above it,
        /// and a right rectangle to the right of the module. 
        /// This split is based on the Shorter Segment (SSEG) approach to splitting the rectangle in to smaller pieces.
        /// 
        /// The adjacency graph that this rectangle takes part in, is also updated correctly, with the new rectangles.
        /// 
        /// The method is based on the method described in the article on fast template placement.
        /// 
        /// 
        /// </summary>
        /// <param name="module">The module to be placed in the rectangle.</param>
        /// <returns>(TopRectangle, RightRectangle) from the split. They are null if they have either width = 0 or height = 0.</returns>
        public (Rectangle, Rectangle) SplitIntoSmallerRectangles(Rectangle rectangle)
        {
            //The module is placed in the lower left corner of the rectangle.

            //Uses the  Shorter Segment (SSEG) approach to splitting the rectangle in to smaller pieces, after placing the module.
            //This means it will place the module in the recangle, and split the remaining area into two rectangle, 
            //based on which segments extending from the rectangle (see FTP algorithm papier) that are shortest:

            rectangle.PlaceAt(this.x, this.y);
            Rectangle TopRectangle = null;
            Rectangle RightRectangle = null;
            int VerticalSegmentLenght = this.height - rectangle.height;
            int HorizontalSegmentLenght = this.width - rectangle.width;
            bool doHorizontalSplit = ShouldSplitAtHorizontalLineSegment(VerticalSegmentLenght, HorizontalSegmentLenght);

            if (VerticalSegmentLenght != 0)
            {
                int recWidth = doHorizontalSplit ? this.width : rectangle.width;
                TopRectangle = new Rectangle(recWidth, VerticalSegmentLenght);
                TopRectangle.PlaceAt(this.x, rectangle.getTopmostYPosition() + 1);

                ComputeAdjacencyList(TopRectangle);
                TopRectangle.AdjacentRectangles.Add(rectangle);
                rectangle.AdjacentRectangles.Add(TopRectangle);
            }

            if (HorizontalSegmentLenght != 0)
            {
                int recHeight = doHorizontalSplit ? rectangle.height : this.height;
                RightRectangle = new Rectangle(HorizontalSegmentLenght, recHeight);
                RightRectangle.PlaceAt(rectangle.getRightmostXPosition() + 1, this.y);

                ComputeAdjacencyList(RightRectangle);
                RightRectangle.AdjacentRectangles.Add(rectangle);
                rectangle.AdjacentRectangles.Add(RightRectangle);
            }

            if (TopRectangle != null && RightRectangle != null)
            {
                TopRectangle.AdjacentRectangles.Add(RightRectangle);
                RightRectangle.AdjacentRectangles.Add(TopRectangle);
            }
            RemoveAdjacencies(); //This line must be before the next line, curtesy of the adjacencies of rectangles being hashsets. 
            ComputeAdjacencyList(rectangle);
            return (TopRectangle, RightRectangle);
        }

        public void splitRectangleInTwo(Rectangle splittingRectangle1, Rectangle splittingRectangle2)
        {
            RemoveAdjacencies();
            splittingRectangle1.AdjacentRectangles.Add(splittingRectangle2);
            splittingRectangle2.AdjacentRectangles.Add(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle2);
        }

        private bool valueInRange(int value, int min, int max)
        {
            return min <= value && value <= max;
        }

        public static bool ShouldSplitAtHorizontalLineSegment(int VerticalSegmentLenght, int HorizontalSegmentLenght)
        {
            return HorizontalSegmentLenght <= VerticalSegmentLenght;
        }

        private void RemoveAdjacencies()
        {
            foreach (var adjacentRectangle in AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(this);
            }
        }


        private void TotalRemoveAdjacencies()
        {
            foreach (var adjacentRectangle in AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(this);
            }
            AdjacentRectangles.Clear();
        }


        public bool MergeWithOtherRectangles(Board board)
        {
            //Recursivly merge with neighboring rectangles, which sides lines up perfectly with the current rectangle:
            List<Rectangle> adjacentRectangles = AdjacentRectangles.ToList();
            for (int i = 0; i < adjacentRectangles.Count; i++)
            {
                Rectangle adjacentRectangle = adjacentRectangles[i];
                if (!adjacentRectangle.isEmpty) continue;
                (RectangleSide side, bool canMerge) = this.CanMerge(adjacentRectangle);
                if (canMerge)
                {
                    //Necessary, as the hashcode will change (remind me to never use hashsets again!):
                    board.EmptyRectangles.Remove(this);
                    board.EmptyRectangles.Remove(adjacentRectangle);
                    MergeWithRectangle(side, adjacentRectangle);
                    board.EmptyRectangles.Add(this, this);
                    //Continue the merging with the updated rectangle!
                    MergeWithOtherRectangles(board);
                    return true;
                }
            }
            //If no other merges can be done, some special merge checks must be made.
            return SplitMerge(board);
        }

        public void MergeWithRectangle(RectangleSide side, Rectangle adjacentRectangle)
        {
            Rectangle mergedRectangle;
            //Necessary, because the transformation updates the hashcode of this rectangle:
            foreach (var rectangle in adjacentRectangle.AdjacentRectangles)
            {
                rectangle.AdjacentRectangles.Remove(adjacentRectangle);
            }
            HashSet<Rectangle> copyAdjacentRectangles = new HashSet<Rectangle>(AdjacentRectangles);
            foreach (var rectangle in AdjacentRectangles)
            {
                rectangle.AdjacentRectangles.Remove(this);
            }
            this.AdjacentRectangles.Clear();
            switch (side)
            {
                case RectangleSide.Left:
                    mergedRectangle = new Rectangle(width + adjacentRectangle.width, height, adjacentRectangle.x, y);
                    break;
                case RectangleSide.Right:
                    mergedRectangle = new Rectangle(width + adjacentRectangle.width, height, x, y);
                    break;
                case RectangleSide.Top:
                    mergedRectangle = new Rectangle(width, height + adjacentRectangle.height, x, y);
                    break;
                case RectangleSide.Bottom:
                    mergedRectangle = new Rectangle(width, height + adjacentRectangle.height, x, adjacentRectangle.y);
                    break;
                default:
                    throw new InternalRuntimeException("A rectangle can only be joined on the sides left, right, top or bottom, not " + side.ToString());
            }
            //It is important that it is the current rectangle that is changed.
            this.width = mergedRectangle.width;
            this.height = mergedRectangle.height;
            this.x = mergedRectangle.x;
            this.y = mergedRectangle.y;
            //Updating adjacent rectangles:
            AdjacentRectangles.UnionWith(copyAdjacentRectangles);
            AdjacentRectangles.UnionWith(adjacentRectangle.AdjacentRectangles);
            adjacentRectangle.AdjacentRectangles.Clear();
            AdjacentRectangles.Remove(adjacentRectangle);
            AdjacentRectangles.Remove(this);
            //Duplicates have been removed automaticly, as AdjacentRectangles is a set.
            //The adjacent rectangles own adjacent rectangles also needs to be updated.
            foreach (var rectangle in AdjacentRectangles)
            {
                rectangle.AdjacentRectangles.Add(this);
            }
        }

        public bool SplitMerge(Board board)
        {
            foreach (var adjacentRectangle in AdjacentRectangles)
            {
                if (!adjacentRectangle.isEmpty) continue;
                (var formsLShapedSegment, var side, var extendDirection) = FormsLSegment(this, adjacentRectangle);
                if (formsLShapedSegment)
                {
                    //To avoid eternal recursion, the L shape should only be split, if it is an actual improvement, not if it does not matter.
                    if (IsLSplitWorthIt(adjacentRectangle, side))
                    {
                        //The horizontal and vertical segment of the L shape needs to be found, 
                        //to evaluate whether or not the L shape should split up into different rectangles than rectangle and adjacentRectangle.
                        (var candidateNewRectangle, var candidateNewAdjacentRectangle) = GetLShapeInformation(adjacentRectangle, side, extendDirection);

                        HashSet<Rectangle> allAdjacentRectangles = new HashSet<Rectangle>();
                        allAdjacentRectangles.UnionWith(this.AdjacentRectangles);
                        allAdjacentRectangles.UnionWith(adjacentRectangle.AdjacentRectangles);
                        allAdjacentRectangles.Remove(this);
                        allAdjacentRectangles.Remove(adjacentRectangle);

                        this.TotalRemoveAdjacencies();
                        adjacentRectangle.TotalRemoveAdjacencies();

                        board.EmptyRectangles.Remove(this);
                        board.EmptyRectangles.Remove(adjacentRectangle);

                        this.TransformToGivenRectangle(candidateNewRectangle);
                        adjacentRectangle.TransformToGivenRectangle(candidateNewAdjacentRectangle);

                        board.EmptyRectangles.Add(this, this);
                        board.EmptyRectangles.Add(adjacentRectangle, adjacentRectangle);

                        allAdjacentRectangles.Add(this);
                        allAdjacentRectangles.Add(adjacentRectangle);
                        foreach (var rectangle in allAdjacentRectangles)
                        {
                            adjacentRectangle.ConnectIfAdjacent(rectangle);
                            this.ConnectIfAdjacent(rectangle);
                        }

                        RectangleOptimizations.OptimizeRectangle(board, this);
                        //In the case that adjacentRectangle have been modified in the above merge, 
                        //we must ensure that the rectangle is actually placed on the board:
                        if (board.EmptyRectangles.TryGetValue(adjacentRectangle, out Rectangle adjacentRectangleInDictionary))
                        {
                            RectangleOptimizations.OptimizeRectangle(board, adjacentRectangleInDictionary);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void TransformToGivenRectangle(Rectangle rectangle)
        {
            this.x = rectangle.x;
            this.y = rectangle.y;
            this.width = rectangle.width;
            this.height = rectangle.height;
        }

        private bool IsLSplitWorthIt(Rectangle adjacentRectangle, RectangleSide side)
        {
            int horizontalSegment;
            int verticalSegment;
            bool isAlreadyOptimallySplit;

            //Check if splitting even improves the rectangles
            if (side.IsVertical())
            {
                if (this.width < adjacentRectangle.width)
                {
                    horizontalSegment = this.width;
                    verticalSegment = adjacentRectangle.height;
                }
                else
                {
                    horizontalSegment = adjacentRectangle.width;
                    verticalSegment = this.height;
                }
                isAlreadyOptimallySplit = ShouldSplitAtHorizontalLineSegment(verticalSegment, horizontalSegment);
            }
            else if (side.IsHorizontal())
            {
                if (this.height < adjacentRectangle.height)
                {
                    horizontalSegment = adjacentRectangle.width;
                    verticalSegment = this.height;
                }
                else
                {
                    horizontalSegment = this.width;
                    verticalSegment = adjacentRectangle.height;
                }
                isAlreadyOptimallySplit = !ShouldSplitAtHorizontalLineSegment(verticalSegment, horizontalSegment);
            }
            else
            {
                throw new InternalRuntimeException("Logic error.");
            }

            bool doesSplitMatter = verticalSegment != horizontalSegment;
            return doesSplitMatter && !isAlreadyOptimallySplit;
        }

        private (Rectangle, Rectangle) GetLShapeInformation(Rectangle adjacentRectangle, RectangleSide side, RectangleSide extendDirection)
        {
            Rectangle smaller;
            Rectangle bigger;
            if (side.IsVertical() && this.width < adjacentRectangle.width ||
                side.IsHorizontal() && this.height < adjacentRectangle.height)
            {
                smaller = this;
                bigger = adjacentRectangle;
            }
            else
            {
                smaller = adjacentRectangle;
                bigger = this;
                side = side.OppositeDirection();
            }


            if (side.IsVertical())
            {
                int movedXPos = extendDirection == RectangleSide.Right ? smaller.width : 0;
                int movedYPos = side == RectangleSide.Bottom ? bigger.height : 0;
                Rectangle newSmaller = new Rectangle(smaller.width, smaller.height + bigger.height, smaller.x, smaller.y - movedYPos);
                Rectangle newBigger = new Rectangle(bigger.width - smaller.width, bigger.height, bigger.x + movedXPos, bigger.y);
                return (newSmaller, newBigger);
            }
            else
            {
                int movedXPos = side == RectangleSide.Left ? bigger.width : 0;
                int movedYPos = extendDirection == RectangleSide.Top ? smaller.height : 0;
                Rectangle newSmaller = new Rectangle(smaller.width + bigger.width, smaller.height, smaller.x - movedXPos, smaller.y);
                Rectangle newBigger = new Rectangle(bigger.width, bigger.height - smaller.height, bigger.x, bigger.y + movedYPos);
                return (newSmaller, newBigger);
            }
        }

        private static (bool, RectangleSide, RectangleSide) FormsLSegment(Rectangle rectangle, Rectangle adjacentRectangle)
        {
            //It forms an L segment, if for any of the corners of rectangle, adjacent rectangle "starts" there:
            //for example if the lower right corner of rectangle is at the same position of the lower left corner of adjacentRectangle.
            var rectangleEdges = rectangle.GetRectangleCorners();
            var adjacentEdges = adjacentRectangle.GetRectangleCorners();

            if      (rectangleEdges.lowerRight == adjacentEdges.lowerLeft) return (true, RectangleSide.Right, RectangleSide.Top);   //It is to the right, and it extends upwards.
            else if (rectangleEdges.topRight   == adjacentEdges.topLeft)   return (true, RectangleSide.Right, RectangleSide.Bottom);//It is to the right, and it extends downwards.

            else if (rectangleEdges.topRight == adjacentEdges.lowerRight)  return (true, RectangleSide.Top, RectangleSide.Left);
            else if (rectangleEdges.topLeft  == adjacentEdges.lowerLeft)   return (true, RectangleSide.Top, RectangleSide.Right);

            else if (rectangleEdges.topLeft   == adjacentEdges.topRight)   return (true, RectangleSide.Left, RectangleSide.Bottom);
            else if (rectangleEdges.lowerLeft == adjacentEdges.lowerRight) return (true, RectangleSide.Left, RectangleSide.Top);

            else if (rectangleEdges.lowerLeft  == adjacentEdges.topLeft)   return (true, RectangleSide.Bottom, RectangleSide.Right);
            else if (rectangleEdges.lowerRight == adjacentEdges.topRight)  return (true, RectangleSide.Bottom, RectangleSide.Left);

            else return (false, RectangleSide.None, RectangleSide.None);
        }

        public (RectangleSide, bool) CanMerge(Rectangle adjacentRectangle)
        {
            //They can merge if the rectangles line up on a side. They can only line up on one side.

            //Below:
            if (adjacentRectangle.getTopmostYPosition() + 1 == y &&
                adjacentRectangle.x == x &&
                width == adjacentRectangle.width)
            {
                return (RectangleSide.Bottom, true);
            }
            //Above:
            else if (this.getTopmostYPosition() + 1 == adjacentRectangle.y &&
                     adjacentRectangle.x == x &&
                     width == adjacentRectangle.width)
            {
                return (RectangleSide.Top, true);
            }
            //Left
            else if (adjacentRectangle.getRightmostXPosition() + 1 == x &&
                     adjacentRectangle.y == y &&
                     height == adjacentRectangle.height)
            {
                return (RectangleSide.Left, true);

            }
            //Right
            else if (this.getRightmostXPosition() + 1 == adjacentRectangle.x &&
                     adjacentRectangle.y == y &&
                     height == adjacentRectangle.height)
            {
                return (RectangleSide.Right, true);
            }
            else return (RectangleSide.None, false);
        }

        private void ComputeAdjacencyList(Rectangle newRectangle)
        {
            foreach (var formerAdjacentRectangle in AdjacentRectangles)
            {
                if (newRectangle.IsAdjacent(formerAdjacentRectangle))
                {
                    newRectangle.AdjacentRectangles.Add(formerAdjacentRectangle);
                    formerAdjacentRectangle.AdjacentRectangles.Add(newRectangle);
                }
            }
            //Also do for the other sides.
        }


        public bool IsAdjacent(Rectangle rectangle)
        {
            //Adjacency depends on which side that the rectangles are closest - left, right top or bottom.
            bool isAdjacentToTheLeft = (rectangle.getRightmostXPosition() + 1 == this.x  && isOverlappingInterval(y, getTopmostYPosition(), rectangle.y, rectangle.getTopmostYPosition()));
            bool isAdjacentBelow     = (rectangle.getTopmostYPosition()   + 1 == this.y  && isOverlappingInterval(x, getRightmostXPosition(), rectangle.x, rectangle.getRightmostXPosition()));
            bool isAdjacentToTheRight = (rectangle.x == this.getRightmostXPosition() + 1 && isOverlappingInterval(y, getTopmostYPosition(), rectangle.y, rectangle.getTopmostYPosition()));
            bool isAdjacentAbove      = (rectangle.y == this.getTopmostYPosition()   + 1 && isOverlappingInterval(x, getRightmostXPosition(), rectangle.x, rectangle.getRightmostXPosition()));
            return isAdjacentToTheLeft || isAdjacentToTheRight || isAdjacentBelow || isAdjacentAbove;
        }

        private bool isOverlappingInterval(int Int1Start, int Int1End, int Int2Start, int Int2End)
        {
            //There is !no! overlap, iff one of the intervals starts after the other begins:
            return !(Int1End < Int2Start || Int2End < Int1Start);
        }

        public int getTopmostYPosition()
        {
            return y + height - 1;
        }

        public int getRightmostXPosition()
        {
            return x + width - 1;
        }

        public (int, int) getCenterPosition()
        {
            return (x + width / 2, y + height / 2);
        }

        public override string ToString()
        {
            return "Rectangle. Width = " + width + ", Height = " + height + ", x = " + x + ", y = " + y;
        }

        public override int GetHashCode()
        {
            //It does not guarentee uniqueness, in the sense that for a given hashcode, 
            //there might be more than one unique set of heigth, width, x, and y values,
            //that could result in that value.

            //The +1 is to avoid everything becoming 0, if one value is 0.
            return (height + 1) * (width + 1) * (x + 1) * (y + 1);
        }

        public override bool Equals(object obj)
        {
            Rectangle rectangleObj = obj as Rectangle;
            if (rectangleObj == null)
                return false;
            else return rectangleObj.height == height &&
                        rectangleObj.width == width &&
                        rectangleObj.x == x &&
                        rectangleObj.y == y;
            //It will not compare adjacency lists.
        }

        /// <summary>
        /// Returns the four corners of the rectangle.
        /// </summary>
        /// <returns>(LowerLeft, LowerRight, TopLeft, TopRight)</returns>
        public (Point lowerLeft, Point lowerRight, Point topLeft, Point topRight) GetRectangleCorners()
        {
            return (new Point(x, y), new Point(x + width, y), new Point(x, y + height), new Point(x + width, y + height));
        }
    }
}
