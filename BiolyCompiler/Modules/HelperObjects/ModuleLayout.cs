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

            CheckIsValidModuleDivision();
            ConnectAdjacentRectangles();
        }

        public ModuleLayout(Rectangle moduleShape, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations) : this(moduleShape.width, moduleShape.height, EmptyRectangles, OutputLocations)
        {
        }

        private void CheckIsValidModuleDivision()
        {
            bool[,] grid = new bool[width, height];
            Rectangle[] allRectangles = GetAllRectanglesIncludingDroplets();
            foreach (var rectangle in allRectangles)
            {
                for (int x = rectangle.x; x < rectangle.width + rectangle.x; x++)
                {
                    for (int y = rectangle.y; y < rectangle.height + rectangle.y; y++)
                    {
                        if (grid[x, y])
                        {
                            throw new InternalRuntimeException("In the current module, there is an overlap of rectangles");
                        }

                        grid[x, y] = true;
                    }
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y])
                    {
                        throw new InternalRuntimeException("The given module layout does not divide the module perfectly up into droplets and empty rectangles, as required");
                    }
                }
            }
        }

        private void ConnectAdjacentRectangles()
        {
            Rectangle[] allRectangles = GetAllRectanglesIncludingDroplets();
            allRectangles.ForEach(x => x.Connect(allRectangles));
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
            }

            for (int i = 0; i < Droplets.Count; i++)
            {
                Droplets[i].Shape = Rectangle.Translocate(Droplets[i].Shape, x, y);
            }

            ConnectAdjacentRectangles();
        }
    }
}