using BiolyCompiler.Commands;
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
        public readonly List<RoutingInformation> route;
        public readonly IDropletSource routedDroplet;
        public readonly int startTime;

        public Route(List<RoutingInformation> route, IDropletSource routedDroplet, int startTime)
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"StartTime = {startTime}, EndTime = {getEndTime()}. Route = [");

            for (int i = 0; i < route.Count; i++)
            {
                builder.Append($"({route[i].x}, {route[i].y})");
                if (i != route.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append("]");
            return builder.ToString();
        }

        public Command[] ToCommands()
        {
            Command[] commands = new Command[(route.Count - 1) * 2 + 1];
            for (int i = 1; i < commands.Length; i++)
            {
                RoutingInformation prevRouteInfo = route[i - 1];
                RoutingInformation currentrouteInfo = route[i];
                commands[i - 1] = new Command(prevRouteInfo.x   , prevRouteInfo.y   , CommandType.ELECTRODE_OFF, i - 1);
                commands[i]     = new Command(currentrouteInfo.x, currentrouteInfo.y, CommandType.ELECTRODE_ON , i);
            }

            return commands;
        }
    }
}
