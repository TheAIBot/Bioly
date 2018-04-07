using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Routing
{
    /* Information used for routing: contains information required by algorithm like dijkstras algorithm, to find paths.
     * 
     */
    public class RoutingInformation
    {
        public int distanceFromSource = Int32.MaxValue;
        public RoutingInformation previous = null;
        public int x;
        public int y;
    }
}
