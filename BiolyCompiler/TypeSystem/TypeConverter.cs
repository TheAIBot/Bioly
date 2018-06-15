using BiolyCompiler.Exceptions.ParserExceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.TypeSystem
{
    public static class TypeConverter
    {
        public static string ToReadableString(this VariableType type)
        {
            switch (type)
            {
                case VariableType.FLUID:
                    return "fluid";
                case VariableType.HEATER:
                    return "heater";
                case VariableType.OUTPUT:
                    return "output";
                case VariableType.NUMBER:
                    return "number";
                case VariableType.FLUID_ARRAY:
                    return "fluid array";
                case VariableType.NUMBER_ARRAY:
                    return "number array";
                default:
                    throw new InternalParseException("Can't make readable version of the VariableType. Type: " + type.ToString());
            }
        }
    }
}
