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

        public Command[] ToCommands(ref int time)
        {
            return Route.ToCommands(route, ref time);
        }

        public static Command[] ToCommands(Point[] route, ref int time)
        {
            if (route.Length == 0)
            {
                return new Command[0];
            }

            Command[] commands = new Command[(route.Length - 1) * 3 + 2];
            Point toTurnOff = route[0];
            int index = 0;

            commands[index++] = new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_ON, time);
            time++;

            for (int i = 1; i < route.Length; i++)
            {
                commands[index++] = new Command(route[i].X, route[i].Y, CommandType.ELECTRODE_ON, time);
                time++;
                commands[index++] = new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_OFF, time);
                commands[index++] = new Command(route[i].X, route[i].Y, CommandType.ELECTRODE_ON, time);
                time++;
                toTurnOff = route[i];
            }

            commands[index++] = new Command(toTurnOff.X, toTurnOff.Y, CommandType.ELECTRODE_OFF, time);

            return commands;
        }
    }
}
