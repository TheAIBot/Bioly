using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.RuntimeExceptions
{
    public class RuntimeException : BlockException
    {
        public RuntimeException(string id, string message) : base(id, message)
        {

        }
    }
}
