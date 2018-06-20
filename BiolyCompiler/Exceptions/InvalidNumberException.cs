using BiolyCompiler.Exceptions.RuntimeExceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions
{
    public class InvalidNumberException : RuntimeException
    {
        public InvalidNumberException(string id, float number) : base(id, $"Tried to do math with {number} which isn't allowed.{Environment.NewLine}The error may have been caused by diving with zero.")
        {

        }
    }
}
