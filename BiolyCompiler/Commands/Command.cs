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
        public readonly ElectrodeStatus status;

        public Command(int x, int y, ElectrodeStatus status, int time)
        {
            this.x = x;
            this.y = y;
            this.status = status;
            this.time = time;
        }
    }
}
