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
        public readonly int height;
        public readonly int width;
        public readonly int x;
        public readonly int y; //Coordinates for lower left corner.
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
            this.x = x;
            this.y = y;
        }

        public Rectangle(Rectangle rectangle) : this(rectangle.width, rectangle.height, rectangle.x, rectangle.y)
        {
            isEmpty = rectangle.isEmpty;
        }

        public static Rectangle Translocate(Rectangle rectangle, int x, int y)
        {
            Rectangle translocated = new Rectangle(rectangle.width, rectangle.height, rectangle.x + x, rectangle.y + y);
            translocated.isEmpty = rectangle.isEmpty;
            translocated.AdjacentRectangles = rectangle.AdjacentRectangles;
            rectangle.AdjacentRectangles.Clear();

            return translocated;
        }

        public bool DoesRectangleFitInside(Rectangle rectangle)
        {
            return DoesRectangleFitInside(rectangle.width, rectangle.height);;
        }

        public bool DoesRectangleFitInside(int w, int h)
        {
            return w <= this.width && h <= this.height;
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

        public static void ReplaceRectangles(Rectangle toReplace, Rectangle replaceWith)
        {
            ReplaceRectangles(new Rectangle[] { toReplace }, new Rectangle[] { replaceWith });
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
        public static (Rectangle top, Rectangle right, Rectangle newSmaller) SplitIntoSmallerRectangles(Rectangle bigger, Rectangle smaller)
        {
            //need to move the smaller rectangle to the same position as the bigger rectangle
            Rectangle newSmaller = new Rectangle(smaller.width, smaller.height, bigger.x, bigger.y);

            if (bigger.width == newSmaller.width && 
                bigger.height == newSmaller.height)
            {
                return (null, null, newSmaller);
            }

            int VerticalSegmentLenght = bigger.height - newSmaller.height;
            int HorizontalSegmentLenght = bigger.width - newSmaller.width;
            bool doHorizontalSplit = ShouldSplitAtHorizontalLineSegment(VerticalSegmentLenght, HorizontalSegmentLenght);


            Rectangle top = null;
            Rectangle bottom = null;

            if (VerticalSegmentLenght != 0)
            {
                int topRectangleWidth = doHorizontalSplit ? bigger.width : newSmaller.width;
                top = new Rectangle(topRectangleWidth, VerticalSegmentLenght, bigger.x, newSmaller.getTopmostYPosition() + 1);
            }

            if (HorizontalSegmentLenght != 0)
            {
                int rightRectangleHeight = doHorizontalSplit ? newSmaller.height : bigger.height;
                bottom = new Rectangle(HorizontalSegmentLenght, rightRectangleHeight, newSmaller.getRightmostXPosition() + 1, bigger.y);
            }

            return (top, bottom, newSmaller);
        }

        public static bool ShouldSplitAtHorizontalLineSegment(int VerticalSegmentLenght, int HorizontalSegmentLenght)
        {
            return HorizontalSegmentLenght <= VerticalSegmentLenght;
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
