using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.ParserExceptions
{
    public class NotANumberException : ParseException
    {
        public NotANumberException(string id, string number) : base(id, $"{number} is not a number.")
        {
        }
    }
}
