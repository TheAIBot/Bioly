using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiolyCompiler.Routing
{
    public class Route
    {
        public List<RoutingInformation> route;
        public readonly Droplet routedDroplet;
        public int startTime;

        public Route(List<RoutingInformation> route, Droplet routedDroplet, int startTime)
        {
            this.route = route;
            this.routedDroplet = routedDroplet;
            this.startTime = startTime;
        }

        public int getEndTime()
        {
            //Minus 1 to route.Count, as the initial position of the drop is included in the route.
            return startTime + (route.Count - 1) * Schedule.DROP_MOVEMENT_TIME;
        }

        public override String ToString()
        {
            String routeString = "StartTime = " + startTime + ", EndTime = " + getEndTime() + ". Route = [";
            for (int i = 0; i < route.Count; i++)
            {
                routeString += "(" + route[i].x + ", " + route[i].y + ")";
                if (i != route.Count - 1) routeString += ", ";
            }

            routeString += "]";
            return routeString;

        }
    }
}
