using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public class BlockExecutionCommand : Command
    {
        public BlockExecutionCommand(int x, int y, CommandType type, int time) : base(x, y, type, time)
        {
        }
    }
}
