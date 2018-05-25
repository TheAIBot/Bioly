using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class MissingBlockException : ParseException
    {
        public MissingBlockException(string id, string message) : base(id, message)
        {
        }
    }
}
