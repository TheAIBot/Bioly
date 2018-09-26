using BiolyCompiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules.HelperObjects
{
    public static class RectangleSideTools
    {
        public static RectangleSide OppositeDirection(this RectangleSide side)
        {
            switch (side)
            {
                case RectangleSide.Left:
                    return RectangleSide.Right;
                case RectangleSide.Right:
                    return RectangleSide.Left;
                case RectangleSide.Top:
                    return RectangleSide.Bottom;
                case RectangleSide.Bottom:
                    return RectangleSide.Top;
                default:
                    throw new InternalRuntimeException($"Can't take the opposite direction of the direction: {side.ToString()}");
            }
        }

        public static bool IsVertical(this RectangleSide side)
        {
            return side == RectangleSide.Top || side == RectangleSide.Bottom;
        }

        public static bool IsHorizontal(this RectangleSide side)
        {
            return side == RectangleSide.Left || side == RectangleSide.Right;
        }
    }
}
