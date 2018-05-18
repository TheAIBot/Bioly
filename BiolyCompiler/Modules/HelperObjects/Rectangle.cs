using System;
using System.Collections.Generic;
using System.Linq;
using BiolyCompiler.Architechtures;
using BiolyCompiler.Modules.RectangleSides;

namespace BiolyCompiler.Modules
{
    public class Rectangle
    {
        public int height, width;
        public int x, y; //Coordinates for lower left corner.
        //Used by the FTP algorithm for deleting rectangles.
        public HashSet<Rectangle>  AdjacentRectangles    = new HashSet<Rectangle>();
        public bool isEmpty = true;


        public Rectangle(int width, int height)
        {
            if (width < 0 || height < 0) throw new Exception("A rectangle must have a non-negative height and width: (width, height)=(" + width + ", " + height + ") is not allowed.");
            this.height = height;
            this.width = width;
        }

        public Rectangle(int width, int height, int x, int y) : this(width, height)
        {
            PlaceAt(x, y);
        }
        
        public Rectangle(Rectangle rectangle) : this(rectangle.width, rectangle.height, rectangle.x, rectangle.y) { isEmpty = rectangle.isEmpty; }

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

        public void ConnectIfAdjacent(Rectangle insideRectangle)
        {
            if (this.IsAdjacent(insideRectangle))
            {
                this.AdjacentRectangles.Add(insideRectangle);
                insideRectangle.AdjacentRectangles.Add(this);
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
            Rectangle TopRectangle;
            Rectangle RightRectangle;
            int VerticalSegmentLenght = this.height - rectangle.height;
            int HorizontalSegmentLenght = this.width - rectangle.width;
            if (ShouldSplitAtHorizontalLineSegment(VerticalSegmentLenght, HorizontalSegmentLenght)){ //Split at the horizontal line segment:
                TopRectangle = new Rectangle(this.width, VerticalSegmentLenght);
                RightRectangle = new Rectangle(HorizontalSegmentLenght, rectangle.height);
            }else { //Split at the vertical line segment:
                TopRectangle = new Rectangle(rectangle.width, VerticalSegmentLenght);
                RightRectangle = new Rectangle(HorizontalSegmentLenght, this.height);
            }

            rectangle.PlaceAt(this.x, this.y);
            TopRectangle.PlaceAt(this.x, rectangle.getTopmostYPosition() + 1);
            RightRectangle.PlaceAt(rectangle.getRightmostXPosition() + 1, this.y);

            //If the line segments has size = 0, the rectangles has an area of 0, 
            //and as such they can be discarded:

            if (VerticalSegmentLenght == 0) TopRectangle = null;
            else {
                ComputeAdjacencyList(TopRectangle);
                TopRectangle.AdjacentRectangles.Add(rectangle);
                rectangle.AdjacentRectangles.Add(TopRectangle);
            }
            if (HorizontalSegmentLenght == 0) RightRectangle = null;
            else
            {
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
            CheckThatTheRectanglesDividesTheRectanglePerfectly(splittingRectangle1, splittingRectangle2);
            RemoveAdjacencies();
            splittingRectangle1.AdjacentRectangles.Add(splittingRectangle2);
            splittingRectangle2.AdjacentRectangles.Add(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle1);
            ComputeAdjacencyList(splittingRectangle2);
        }

        private void CheckThatTheRectanglesDividesTheRectanglePerfectly(Rectangle splittingRectangle1, Rectangle splittingRectangle2)
        {
            if (splittingRectangle1.getArea() + splittingRectangle2.GetArea() != this.getArea())
                throw new Exception("The sum of the area of the two rectangles that are supposed to split the rectangle into two, " +
                                    "do not equal the area of the split rectangle.");
            else if (splittingRectangle1.width == 0 || splittingRectangle1.height == 0 ||
                     splittingRectangle2.width == 0 || splittingRectangle2.height == 0)
                throw new Exception("The two rectangles that are supposed to split the rectangle, must both have a non-zero size.");
            //else if (splittingRectangle1.isOverlappingWith(splittingRectangle2))
            //    throw new Exception("The two rectangles that are supposed to split the rectangle into two are overlapping");
            else if (!(splittingRectangle1.isCompletlyInside(this) && splittingRectangle2.isCompletlyInside(this)))
                throw new Exception("At least one of the two rectangles that are supposed to split the rectangle into two, are not competly contained in the rectangle.");
        }

        private bool isOverlappingWith(Rectangle splittingRectangle)
        {
            //Based on https://stackoverflow.com/questions/306316/determine-if-two-rectangles-overlap-each-other
            bool xOverlap = valueInRange(x, splittingRectangle.x, splittingRectangle.x + splittingRectangle.width) ||
                            valueInRange(splittingRectangle.x, x, x + width);

            bool yOverlap = valueInRange(y, splittingRectangle.y, splittingRectangle.y + splittingRectangle.height) ||
                            valueInRange(splittingRectangle.y, y, y + height);

            return xOverlap && yOverlap;
        }

        private bool valueInRange(int value, int min, int max)
        {
            return min <= value && value <= max;
        }

        private bool isCompletlyInside(Rectangle rectangle)
        {
            return (rectangle.x <= x && x + width  <= rectangle.x + rectangle.width  && 
                    rectangle.y <= y && y + height <= rectangle.y + rectangle.height);
        }

        private int getArea()
        {
            return width * height;
        }

        private static bool ShouldSplitAtHorizontalLineSegment(int VerticalSegmentLenght, int HorizontalSegmentLenght)
        {
            return HorizontalSegmentLenght <= VerticalSegmentLenght;
        }

        private void RemoveAdjacencies()
        {
            foreach (var adjacentRectangle in AdjacentRectangles)
            {
                adjacentRectangle.AdjacentRectangles.Remove(this);
            }
            //Should not be necessary:
            //AdjacentRectangles.Clear();
        }


        public void MergeWithOtherRectangles(Board board)
        {
            //Recursivly merge with neighboring rectangles, which sides lines up perfectly with the current rectangle:
            foreach (var AdjacentRectangle in AdjacentRectangles)
            {
                if (!AdjacentRectangle.isEmpty) continue;
                (RectangleSide side, bool canMerge) = this.CanMerge(AdjacentRectangle);
                if (canMerge) {
                    Rectangle mergedRectangle = MergeWithRectangle(side, AdjacentRectangle);
                    board.EmptyRectangles.Remove(AdjacentRectangle);
                    board.EmptyRectangles.Remove(this);
                    board.EmptyRectangles.Add(mergedRectangle);
                    //Continue the merging with the new rectangle!
                    mergedRectangle.MergeWithOtherRectangles(board);
                    return;
                }
            }
            //The last merges rectangle will be placed here.
            //SplitMerge();
        }

        public Rectangle MergeWithRectangle(RectangleSide side, Rectangle adjacentRectangle)
        {
            Rectangle mergedRectangle;
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
                    throw new Exception("A rectangle can only be joined on the sides left, right, top or bottom, not " + side.ToString());
            } 
            //Updating adjacent rectangles:
            mergedRectangle.AdjacentRectangles.UnionWith(this.AdjacentRectangles);
            mergedRectangle.AdjacentRectangles.UnionWith(adjacentRectangle.AdjacentRectangles);
            mergedRectangle.AdjacentRectangles.Remove(adjacentRectangle);
            mergedRectangle.AdjacentRectangles.Remove(this);
            //Duplicates have been removed automaticly, as AdjacentRectangles is a set.
            //The adjacent rectangles own adjacent rectangles also needs to be updated.
            foreach (var rectangle in mergedRectangle.AdjacentRectangles)
            {
                rectangle.AdjacentRectangles.Remove(adjacentRectangle);
                rectangle.AdjacentRectangles.Remove(this);
                rectangle.AdjacentRectangles.Add(mergedRectangle);
            }
            return mergedRectangle;
        }

        private void SplitMerge()
        {
            throw new NotImplementedException();
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
            foreach (var formerAdjacentRectangle in AdjacentRectangles) {
                if (newRectangle.IsAdjacent(formerAdjacentRectangle)) {
                    newRectangle.AdjacentRectangles.Add(formerAdjacentRectangle);
                    formerAdjacentRectangle.AdjacentRectangles.Add(newRectangle);
                }
            }
            //Also do for the other sides.
        }

        
        public bool IsAdjacent(Rectangle rectangle)
        {
            //Adjacency depends on which side that the rectangles are closest - left, right top or bottom.
            Boolean isAdjacentToTheLeft  = (rectangle.getRightmostXPosition() + 1 == this.x   && isOverlappingInterval(y, getTopmostYPosition()  , rectangle.y, rectangle.getTopmostYPosition()));
            Boolean isAdjacentBelow      = (rectangle.getTopmostYPosition()   + 1 == this.y   && isOverlappingInterval(x, getRightmostXPosition(), rectangle.x, rectangle.getRightmostXPosition()));
            Boolean isAdjacentToTheRight = (rectangle.x == this.getRightmostXPosition() + 1   && isOverlappingInterval(y, getTopmostYPosition()  , rectangle.y, rectangle.getTopmostYPosition()));
            Boolean isAdjacentAbove      = (rectangle.y == this.getTopmostYPosition() + 1     && isOverlappingInterval(x, getRightmostXPosition(), rectangle.x, rectangle.getRightmostXPosition()));
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
            //there might be more than one unique set of heigh, width, x, and y values,
            //that could result in that value.

            //The +1 is to avoid everything becoming 0, if one value is 0.
            return (height+1) * (width+1) * (x+1) * (y+1);
        }

        public override bool Equals(object obj)
        {
            Rectangle rectangleObj = obj as Rectangle;
            if (rectangleObj == null)
                return false;
            else return rectangleObj.height == height &&
                        rectangleObj.width  == width  &&
                        rectangleObj.x      == x      &&
                        rectangleObj.y      == y;
            //It will not compare adjacency lists.
        }
    }
}
