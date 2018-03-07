using System;
using System.Collections.Generic;
using BiolyCompiler.Architechtures;

namespace BiolyCompiler.Modules
{
    public class Rectangle
    {
        public int height, width;
        public int x, y; //Coordinates for lower left corner.
        //Used by the FTP aalgorithm for deleting rectangles.
        public List<Rectangle>  AdjacentRectangles    = new List<Rectangle>();
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
            return module.shape.height < this.height && module.shape.width < this.width;
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


        //Recursive
        public void MergeWithOtherRectangles(Board board)
        {
            foreach (var AdjacentRectangle in AdjacentRectangles)
            {
                if (this.CanMerge(AdjacentRectangle)) {
                    MergeWithRectangle(AdjacentRectangle);
                    AdjacentRectangle.MergeWithOtherRectangles(board);
                    return;
                }
            }
            //The last merges rectangle will be placed here.
            SplitMerge();
        }

        private void MergeWithRectangle(Rectangle adjacentRectangle)
        {
            throw new NotImplementedException();
        }

        private void SplitMerge()
        {
            throw new NotImplementedException();
        }

        private bool CanMerge(Rectangle adjacentRectangle)
        {
            throw new NotImplementedException();
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
    }
}
