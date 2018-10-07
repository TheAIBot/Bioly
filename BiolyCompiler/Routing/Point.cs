using BiolyCompiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Routing
{
    public readonly struct Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point Move(RouteDirection direction)
        {
            switch (direction)
            {
                case RouteDirection.None:
                case RouteDirection.Start:
                    throw new InternalRuntimeException("Can't move a point by the direction: " + direction.ToString());
                case RouteDirection.Left:
                    return new Point(X - 1, Y);
                case RouteDirection.Right:
                    return new Point(X + 1, Y);
                case RouteDirection.Up:
                    return new Point(X, Y + 1);
                case RouteDirection.Down:
                    return new Point(X, Y - 1);
                default:
                    throw new InternalRuntimeException("Unable to understand direction: " + direction.ToString());
            }
        }
    }
}
