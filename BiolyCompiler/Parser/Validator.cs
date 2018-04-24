using BiolyCompiler.Exceptions.ParserExceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Parser
{
    public static class Validator
    {
        public static void ValueWithinRange(string id, float value, float min, float max)
        {
            if (value < min || value > max)
            {
                throw new NumberOutOfRangeException(id, value, min, max);
            }
        }
    }
}
