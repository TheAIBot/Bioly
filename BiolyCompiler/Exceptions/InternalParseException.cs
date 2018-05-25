using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class InternalParseException : Exception
    {
        public InternalParseException(string message) : base(message)
        {
        }

        public InternalParseException(string id, string message) : base(message)
        {
        }
    }
}
