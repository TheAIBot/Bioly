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
        public readonly int x;
        public readonly int y;
        public readonly RoutingInformation previous;
        public readonly int distanceFromSource;

        public RoutingInformation(int x, int y, RoutingInformation prev, int distance)
        {
            this.x = x;
            this.y = y;
            previous = prev;
            distanceFromSource = distance;
        }

        public override bool Equals(object obj)
        {
            return  obj is RoutingInformation routingObject &&
                    this.x == routingObject.x && 
                    this.y == routingObject.y;
        }
    }
}
