using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class ParseException : BlockException
    {
        public ParseException(string id, string message) : base(id, message)
        {
        }
    }
}
