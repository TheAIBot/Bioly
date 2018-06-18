using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions
{
    public class RuntimeException : BlockException
    {
        public RuntimeException(string id, string message) : base(id, message)
        {

        }

        public RuntimeException(string message) : base("", message)
        {

        }
    }
}
