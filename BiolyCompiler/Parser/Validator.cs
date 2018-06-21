using BiolyCompiler.Exceptions.ParserExceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BiolyCompiler.Parser
{
    public static class Validator
    {
        public const string INLINE_PROGRAM_SPECIAL_SEPARATOR = "#¤#";
        public const string FLUID_ARRAY_SPECIAL_SEPARATOR = "@#@";
        private const string SPECIAL_SEPARATORS = "(" + INLINE_PROGRAM_SPECIAL_SEPARATOR + "|" + FLUID_ARRAY_SPECIAL_SEPARATOR + ")";

        public static void ValueWithinRange(string id, float value, float min, float max)
        {
            if (value < min || value > max)
            {
                throw new NumberOutOfRangeException(id, value, min, max);
            }
        }

        public static void CheckVariableName(string id, string variableName)
        {
            //has to start with a character and can't end with a space
            if (!Regex.IsMatch(variableName, $"^[a-zA-Z0-9 ]+({SPECIAL_SEPARATORS}[a-zA-Z0-9 ]+)?$"))
            {
                throw new ParseException(id, "Variable names must only consist of letters(a to z), numbers and spaces.");
            }
        }
    }
}
