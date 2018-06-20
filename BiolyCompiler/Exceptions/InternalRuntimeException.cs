using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions
{
    public class InternalRuntimeException : RuntimeException
    {
        public InternalRuntimeException(string message) : base(message)
        {

        }
    }
}
