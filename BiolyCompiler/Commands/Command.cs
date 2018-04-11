using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public class Command
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Time;
        public readonly CommandType Type;

        public Command(int x, int y, CommandType type, int time)
        {
            this.X = x;
            this.Y = y;
            this.Type = type;
            this.Time = time;
        }
    }
}
