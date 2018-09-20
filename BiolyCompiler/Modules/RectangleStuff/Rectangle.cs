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

        public static (Rectangle[] allRectangles, Rectangle newSmaller) SplitIntoSmallerRectangles(Rectangle bigger, Rectangle smaller)
        {
            //need to move the smaller rectangle to the same position as the bigger rectangle
            Rectangle newSmaller = new Rectangle(smaller.width, smaller.height, bigger.x, bigger.y);

            if (bigger.width == newSmaller.width && 
                bigger.height == newSmaller.height)
            {
                return (new Rectangle[] { newSmaller }, newSmaller);
            }

            int VerticalSegmentLenght = bigger.height - newSmaller.height;
            int HorizontalSegmentLenght = bigger.width - newSmaller.width;
            bool doHorizontalSplit = ShouldSplitAtHorizontalLineSegment(VerticalSegmentLenght, HorizontalSegmentLenght);

            List<Rectangle> rectangles = new List<Rectangle>();
            rectangles.Add(newSmaller);

            if (VerticalSegmentLenght != 0)
            {
                int topRectangleWidth = doHorizontalSplit ? bigger.width : newSmaller.width;
                rectangles.Add(new Rectangle(topRectangleWidth, VerticalSegmentLenght, bigger.x, newSmaller.getTopmostYPosition() + 1));
            }

            if (HorizontalSegmentLenght != 0)
            {
                int rightRectangleHeight = doHorizontalSplit ? newSmaller.height : bigger.height;
                rectangles.Add(new Rectangle(HorizontalSegmentLenght, rightRectangleHeight, newSmaller.getRightmostXPosition() + 1, bigger.y));
            }

            return (rectangles.ToArray(), newSmaller);
        }

        public void splitRectangleInTwo(Rectangle splittingRectangle1, Rectangle splittingRectangle2)
        {
            RemoveAdjacencies();
            splittingRectangle1.AdjacentRectangles.Add(splittingRectangle2);
            splittingRectangle2.AdjacentRectangles.Add(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle2);
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
