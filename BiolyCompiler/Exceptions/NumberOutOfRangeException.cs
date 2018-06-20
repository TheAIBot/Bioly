using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class NumberOutOfRangeException : ParseException
    {
        public NumberOutOfRangeException(string id, float value, float min, float max) : base(id, $"{value} is outside the range {min.ToString("N0")} to {max.ToString("N0")}.")
        {
        }
    }
}
