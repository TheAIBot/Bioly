using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public class Command
    {
        public readonly int x;
        public readonly int y;
        public readonly int time;
        public readonly CommandType type;

        public Command(int x, int y, CommandType type, int time)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.time = time;
        }
    }
}
