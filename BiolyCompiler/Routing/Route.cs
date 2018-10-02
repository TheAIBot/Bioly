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
        public readonly Point[] route;
        public readonly IDropletSource routedDroplet;
        public readonly int startTime;

        public Route(Point[] route, IDropletSource routedDroplet, int startTime)
        {
            this.route = route;
            this.routedDroplet = routedDroplet;
            this.startTime = startTime;
        }

        public int getEndTime()
        {
            int temp = startTime;
            //Minus 1 to route.Count, as the initial position of the drop is included in the route.
            return ToCommands(ref temp).Last().Time;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"StartTime = {startTime}, EndTime = {getEndTime()}. Route = [");

            for (int i = 0; i < route.Length; i++)
            {
                builder.Append($"({route[i].X}, {route[i].Y})");
                if (i != route.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append("]");
            return builder.ToString();
        }

        public List<Command> ToCommands(ref int time)
        {
            List<Command> commands = new List<Command>();
            if (route.Length > 0)
            {
                Point toTurnOff = route[0];
                commands.Add(new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_ON, time));
                time++;

                for (int i = 1; i < route.Length; i++)
                {
                    commands.Add(new Command(route[i].X, route[i].Y, CommandType.ELECTRODE_ON, time));
                    time++;
                    commands.Add(new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_OFF, time));
                    commands.Add(new Command(route[i].X, route[i].Y, CommandType.ELECTRODE_ON, time));
                    time++;
                    toTurnOff = route[i];
                }

                commands.Add(new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_OFF, time));
            }

            return commands;
        }
    }
}
