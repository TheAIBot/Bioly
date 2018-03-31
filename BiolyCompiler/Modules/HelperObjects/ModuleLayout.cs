using System;
using System.Collections.Generic;

namespace BiolyCompiler.Modules
{
    public class ModuleLayout
    {
        //The empty rectangles and the output locations should partition the whole module, with no overlap.
        //It should also be done in such a way that the fast template placement merges everything correctly.
        public List<Rectangle> EmptyRectangles;
        public List<Droplet> OutputDroplets;
        public readonly int width, height;

        public ModuleLayout(int width, int height, List<Rectangle> EmptyRectangles, List<Droplet> OutputLocations)
        {
            this.width = width;
            this.height = height;
            CheckIsValidModuleDivision(EmptyRectangles, OutputLocations);
            ConnectAdjacentRectangles(EmptyRectangles, OutputLocations);
            this.EmptyRectangles = EmptyRectangles;
            this.OutputDroplets = OutputLocations;
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
                        if (grid[i + rectangle.x, j + rectangle.y] == true) throw new Exception("In the current module, there is an overlap of rectangles");
                        else grid[i + rectangle.x, j + rectangle.y] = true;
                    }
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (grid[i,j] == false) throw new Exception("The given module layout does not divide the module perfectly up into droplets and empty rectangles, as required");
                }
            }
        }

        public void ChangeOutputType(BoardFluid fluidType)
        {
            OutputDroplets.ForEach(droplet => droplet.SetFluidType(fluidType));
        }
    }
}