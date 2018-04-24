using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class ParseException : Exception
    {
        public readonly string ID;

        public ParseException(string id, string message) : base(message)
        {
            ID = id;
        }
    }
}
