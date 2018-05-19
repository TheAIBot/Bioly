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

        public override bool Equals(object obj)
        {
            if (obj is Command command)
            {
                return this.X == command.X && 
                       this.Y == command.Y && 
                       this.Time == command.Time && 
                       this.Type == command.Type;
            }
            return false;
        }

        public override string ToString()
        {
            return $"x: {X}, y: {Y}, T: {Time}";
        }
    }
}
