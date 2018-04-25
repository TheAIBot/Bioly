﻿using BiolyCompiler.Exceptions.ParserExceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        public static void CheckVariableName(string id, string variableName)
        {
            if (!Regex.IsMatch(variableName, "[a-zA-Z][a-zA-Z0-9]*$"))
            {
                throw new ParseException(id, "Variable names must only consist of letters(a to z), numbers, spaces and underscores and has to start with a letter.");
            }
        }
    }
}
