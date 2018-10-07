using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Routing
{
    [Flags]
    public enum RouteDirection : byte
    {
        None  = 0b0000_0000,
        Start = 0b0000_0001,
        Left  = 0b0000_0010,
        Right = 0b0000_0100,
        Up    = 0b0000_1000,
        Down  = 0b0001_0000
    }
}
