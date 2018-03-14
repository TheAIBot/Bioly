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
        //Used by the FTP aalgorithm for deleting rectangles.
        public HashSet<Rectangle>  AdjacentRectangles    = new HashSet<Rectangle>();
        public bool isEmpty = true;
        //public List<Rectangle> bottomAdjacentRectangles = new List<Rectangle>();
        //public List<Rectangle> leftAdjacentRectangles   = new List<Rectangle>();
        //public List<Rectangle> rightAdjacentRectangles  = new List<Rectangle>();


        public Rectangle(int width, int height)
        {
            this.height = height;
            this.width  = width; 
        }

        public Rectangle(int width, int height, int x, int y) : this(width, height)
        {
            PlaceAt(x, y);
        }
        
        public Rectangle(Rectangle rectangle) : this(rectangle.width, rectangle.height, rectangle.x, rectangle.y) { }

        public void PlaceAt(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool DoesFit(Module module)
        {
            return module.shape.height <= this.height && module.shape.width <= this.width;
        }

        public int GetArea()
        {
            return height * width;
        }

        public Tuple<Rectangle, Rectangle> SplitIntoSmallerRectangles(Module module)
        {
            //The module is placed in the lower left corner of the rectangle.

            //Uses the  Shorter Segment (SSEG) approach to splitting the rectangle in to smaller pieces, after placing the module.
            //This means it will place the module in the recangle, and split the remaining area into two rectangle, 
            //based on which segments extending from the rectangle (see FTP algorithm papier) that are shortest:
            Rectangle TopRectangle;
            Rectangle RightRectangle;
            int VerticalSegmentLenght   = this.height - module.shape.height;
            int HorizontalSegmentLenght = this.width  - module.shape.width;
            if (HorizontalSegmentLenght <= VerticalSegmentLenght)
            {
                //Split at the horizontal line segment:
                TopRectangle   = new Rectangle(this.width, VerticalSegmentLenght);
                RightRectangle = new Rectangle(HorizontalSegmentLenght, module.shape.height);
            } else
            {
                //Split at the vertical line segment:
                TopRectangle   = new Rectangle(module.shape.width, VerticalSegmentLenght);
                RightRectangle = new Rectangle(HorizontalSegmentLenght, this.height);
            }
            module.shape.PlaceAt(this.x, this.y);
            TopRectangle.PlaceAt(this.x, module.shape.getTopmostYPosition() + 1);
            RightRectangle.PlaceAt(module.shape.getRightmostXPosition() + 1, this.y);

            //If the line segments has size = 0, the rectangles has an area of 0, 
            //and as such they can be discarded:

            if (VerticalSegmentLenght == 0) TopRectangle = null;
            else {
                ComputeAdjacencyList(TopRectangle);
                TopRectangle.AdjacentRectangles.Add(module.shape);
                module.shape.AdjacentRectangles.Add(TopRectangle);
            }
            if (HorizontalSegmentLenght == 0) RightRectangle = null;
            else {
                ComputeAdjacencyList(RightRectangle);
                RightRectangle.AdjacentRectangles.Add(module.shape);
                module.shape.AdjacentRectangles.Add(RightRectangle);
            }

            if (TopRectangle != null && RightRectangle != null)
            {
                TopRectangle.AdjacentRectangles.Add(RightRectangle);
                RightRectangle.AdjacentRectangles.Add(TopRectangle);
            }
            
            ComputeAdjacencyList(module.shape);
            

            RemoveAdjacencies();

            return new Tuple<Rectangle, Rectangle>(TopRectangle, RightRectangle);
        }

        private void RemoveAdjacencies()
        {
            foreach (var AdjacentRectangle in AdjacentRectangles)
            {
                AdjacentRectangle.AdjacentRectangles.Remove(this);
            }
        }


        public void MergeWithOtherRectangles(Board board)
        {
            //Recursive
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
                    return null;
                    break;
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

        private void ComputeAdjacencyList(Rectangle rectangle)
        {
            foreach (var formerAdjacentRectangle in AdjacentRectangles) {
                if (rectangle.IsAdjacent(formerAdjacentRectangle)) {
                    rectangle.AdjacentRectangles.Add(formerAdjacentRectangle);
                    formerAdjacentRectangle.AdjacentRectangles.Add(rectangle);
                }
            }
            //Also do for the other sides.
        }

        
        public bool IsAdjacent(Rectangle rectangle)
        {
            //Adjaceny depends on which side that the rectangles are closest - left, right top or bottom.
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

        public override string ToString()
        {
            return "Rectangle. Width = " + width + ", Height = " + height + ", x = " + x + ", y = " + y; 
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
