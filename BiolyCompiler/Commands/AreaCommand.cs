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

        private static readonly Random Rando = new Random(12);

        public AreaCommand(Rectangle shape, CommandType type, int time) : base(shape.x, shape.y, type, time)
        {
            this.ID = shape.ToString().Replace(' ', '-');
            this.Width = shape.width;
            this.Height = shape.height;
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
