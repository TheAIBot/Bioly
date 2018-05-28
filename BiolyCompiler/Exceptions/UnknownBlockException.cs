using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class UnknownBlockException : ParseException
    {
        public UnknownBlockException(string id) : base(id, "Encountered an unknown block. This block can't be used.")
        {
        }
    }
}
