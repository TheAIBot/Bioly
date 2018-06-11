using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public class AreaCommand : Command
    {
        public readonly string ID;
        public readonly int Width;
        public readonly int Height;
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public AreaCommand(Rectangle shape, CommandType type, int time) : this(shape.x, shape.y, shape.width, shape.height, type, time)
        {
        }
   
        public AreaCommand(int x, int y, int width, int height, CommandType type, int time) : base(x, y, type, time)
        {
            this.ID = $"{x}-{y}-{width}-{height}";
            this.Width = width;
            this.Height = height;
            Random Rando = new Random(x * 2133 + y);
            this.R = (float)Rando.NextDouble();
            this.G = (float)Rando.NextDouble();
            this.B = (float)Rando.NextDouble();
        }

        public override string ToString()
        {
            return $"x: {X}, y: {Y}, w: {Width}, h: {Height}, T: {Time}";
        }
    }
}
