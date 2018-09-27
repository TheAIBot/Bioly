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
        public readonly List<Rectangle> EmptyRectangles;
        public readonly List<Droplet> Droplets;
        public readonly int width;
        public readonly int height;


        protected ModuleLayout(List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations)
        {
            this.EmptyRectangles = EmptyRectangles;
            this.Droplets = OutputLocations;
        }

        public ModuleLayout(int width, int height, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations)
        {
            this.width  = width;
            this.height = height;
            this.EmptyRectangles = EmptyRectangles;
            this.Droplets = OutputLocations;

            CheckIsValidModuleDivision(EmptyRectangles, Droplets);
            ConnectAdjacentRectangles(EmptyRectangles, Droplets);
        }

        public ModuleLayout(Rectangle moduleShape, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations) : this(moduleShape.width, moduleShape.height, EmptyRectangles, OutputLocations)
        {
        }


        private void ConnectAdjacentRectangles(List<Rectangle> emptyRectangles, List<Droplet> outputLocations)
        {
            Rectangle[] allRectangles = GetAllRectanglesIncludingDroplets();
            allRectangles.ForEach(x => x.Connect(allRectangles));
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

        public Rectangle[] GetAllRectanglesIncludingDroplets()
        {
            Rectangle[] allRectangles = new Rectangle[EmptyRectangles.Count + Droplets.Count];

            int index = 0;
            for (int i = 0; i < EmptyRectangles.Count; i++)
            {
                allRectangles[index++] = EmptyRectangles[i];
            }
            for (int i = 0; i < Droplets.Count; i++)
            {
                allRectangles[index++] = Droplets[i].Shape;
            }

            return allRectangles;
        }

        public void Reposition(int x, int y)
        {
            //Changing the position of the rectangles and droplets changes their hashcodes, which are used for adjacencies.
            //Therefore it is "necessary" to recalculate them again. It can be made more efficient, if so desired, so that it runs in O(|E|) time.
            for (int i = 0; i < EmptyRectangles.Count; i++)
            {
                EmptyRectangles[i] = Rectangle.Translocate(EmptyRectangles[i], x, y);
                EmptyRectangles[i].AdjacentRectangles.Clear();
            }

            for (int i = 0; i < Droplets.Count; i++)
            {
                Droplets[i].Shape = Rectangle.Translocate(Droplets[i].Shape, x, y);
                Droplets[i].Shape.AdjacentRectangles.Clear(); ;
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
                }
                else
                {
                    CopyDroplet = new Droplet();
                }
                CopyDroplet.Shape = Rectangle.Translocate(CopyDroplet.Shape, droplet.Shape.x, droplet.Shape.y);
                CopyOutputDroplets.Add(CopyDroplet);
            }

            return new ModuleLayout(new Rectangle(width, height), CopyEmptyRectangles, CopyOutputDroplets);
        }
    }
}