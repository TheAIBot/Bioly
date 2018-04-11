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

        public AreaCommand(int x, int y, int time, string id, int width, int height) : base(x, y, CommandType.SHOW_AREA, time)
        {
            this.ID = id;
            this.Width = width;
            this.Height = height;
            this.R = (float)Rando.NextDouble();
            this.G = (float)Rando.NextDouble();
            this.B = (float)Rando.NextDouble();
        }

        public AreaCommand(int time, string id) : base(0, 0, CommandType.REMOVE_AREA, time)
        {
            this.ID = id;
            this.Width = 0;
            this.Height = 0;
            this.R = 0;
            this.G = 0;
            this.B = 0;
        }
    }
}
