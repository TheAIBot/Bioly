using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.RuntimeExceptions
{
    class ArrayIndexOutOfRange : RuntimeException
    {
        public ArrayIndexOutOfRange(string id, string arrayName, int arrayLength, int invalidIndex) : base(id, $"Can't access index {invalidIndex} of array {arrayName}. The valid index range is 0 to {arrayLength - 1}.")
        {

        }
    }
}
