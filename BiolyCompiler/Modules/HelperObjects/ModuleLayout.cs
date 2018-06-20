using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiolyCompiler.Modules
{
    public class ModuleLayout
    {
        //The empty rectangles and the output locations should partition the whole module, with no overlap.
        //It should also be done in such a way that the fast template placement merges everything correctly.
        public List<Rectangle> EmptyRectangles;
        public List<Droplet> Droplets;
        public int width  { get; protected set; }
        public int height { get; protected set; }


        protected ModuleLayout(List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations)
        {
            this.EmptyRectangles = EmptyRectangles;
            this.Droplets = OutputLocations;
        }

        public ModuleLayout(int width, int height, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations) : this(EmptyRectangles, OutputLocations)
        {
            this.width  = width;
            this.height = height;
            CheckIsValidModuleDivision(EmptyRectangles, OutputLocations);
            ConnectAdjacentRectangles(EmptyRectangles, OutputLocations);
        }

        public ModuleLayout(Rectangle moduleShape, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations) : this(moduleShape.width, moduleShape.height, EmptyRectangles, OutputLocations)
        {
        }


        private void ConnectAdjacentRectangles(List<Rectangle> emptyRectangles, List<Droplet> outputLocations)
        {
            List<Rectangle> AllRectangles = new List<Rectangle>(emptyRectangles);
            outputLocations.ForEach(droplet => AllRectangles.Add(droplet.Shape));
            //For each pair of rectangles, if they are adjacent, connect them.
            //As the graph of the rectangle is planar, i think it should be possible to do in linear time - Jesper
            for (int i = 0; i < AllRectangles.Count; i++)
            {
                for (int j = i+1; j < AllRectangles.Count; j++)
                {
                    if (AllRectangles[i].IsAdjacent(AllRectangles[j]))
                    {
                        AllRectangles[i].AdjacentRectangles.Add(AllRectangles[j]);
                        AllRectangles[j].AdjacentRectangles.Add(AllRectangles[i]);
                    }
                }
            }

        }

        private void CheckIsValidModuleDivision(List<Rectangle> emptyRectangles, List<Droplet> outputLocations)
        {
            bool[,] grid = new bool[width, height];
            List<Rectangle> AllRectangles = new List<Rectangle>(emptyRectangles);
            outputLocations.ForEach(droplet => AllRectangles.Add(droplet.Shape));
            foreach (var rectangle in AllRectangles)
            {
                for (int i = 0; i < rectangle.width; i++){
                    for (int j = 0; j < rectangle.height; j++)
                    {
                        if (grid[i + rectangle.x, j + rectangle.y] == true) throw new InternalRuntimeException("In the current module, there is an overlap of rectangles");
                        else grid[i + rectangle.x, j + rectangle.y] = true;
                    }
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (grid[i,j] == false) throw new InternalRuntimeException("The given module layout does not divide the module perfectly up into droplets and empty rectangles, as required");
                }
            }
        }

        public void ChangeFluidType(BoardFluid fluidType)
        {
            Droplets.ForEach(droplet => droplet.SetFluidType(fluidType));
        }

        public List<Rectangle> getAllRectanglesIncludingDroplets()
        {
            List<Rectangle> allRectangles = new List<Rectangle>(EmptyRectangles);
            allRectangles.AddRange(Droplets.Select(droplet => droplet.Shape));
            return allRectangles;
        }

        public void Reposition(int x, int y)
        {
            //Changing the position of the rectangles and droplets changes their hashcodes, which are used for adjacencies.
            //Therefore it is "necessary" to recalculate them again. It can be made more efficient, if so desired, so that it runs in O(|E|) time.
            foreach (var rectangle in EmptyRectangles)
            {
                rectangle.x += x;
                rectangle.y += y;
                rectangle.AdjacentRectangles.Clear();
            }
            foreach (var droplet in Droplets)
            {
                droplet.Shape.x += x;
                droplet.Shape.y += y;
                droplet.Shape.AdjacentRectangles.Clear();
            }

            ConnectAdjacentRectangles(EmptyRectangles, Droplets);
        }

        /// <summary>
        /// Creates a copy of the ModuleLayout. Note that it works by copying the rectangles and droplets that the layout is made of,
        /// and creating a new layout based on this, so if any modifications have been made to the layout,
        /// where the creation of the layout does not handle this, it will not be taken into account when creating the copy.
        /// 
        /// This can include things like changing the adjacencies of the empty rectangles.
        /// </summary>
        /// <returns></returns>
        public ModuleLayout GetCopy()
        {
            List<Rectangle> CopyEmptyRectangles = new List<Rectangle>();
            List<Droplet> CopyOutputDroplets = new List<Droplet>();
            EmptyRectangles.ForEach(rectangle => CopyEmptyRectangles.Add(new Rectangle(rectangle)));
            Dictionary<String, BoardFluid> differentFluidTypes = new Dictionary<string, BoardFluid>();
            foreach (var droplet in Droplets)
            {
                Droplet CopyDroplet;
                if (droplet.GetFluidType() != null)
                {
                    BoardFluid fluidType;
                    differentFluidTypes.TryGetValue(droplet.GetFluidType().FluidName, out fluidType);
                    if (fluidType == null)
                    {
                        fluidType = new BoardFluid(droplet.GetFluidType().FluidName);
                        differentFluidTypes.Add(fluidType.FluidName, fluidType);
                    }
                    CopyDroplet = new Droplet(fluidType);
                } else CopyDroplet = new Droplet();
                CopyDroplet.Shape.PlaceAt(droplet.Shape.x, droplet.Shape.y);
                CopyOutputDroplets.Add(CopyDroplet);
            }

            return new ModuleLayout(new Rectangle(width, height), CopyEmptyRectangles, CopyOutputDroplets);
        }
    }
}