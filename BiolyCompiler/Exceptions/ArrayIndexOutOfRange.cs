using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions.RuntimeExceptions
{
    class ArrayIndexOutOfRange : RuntimeException
    {
        public ArrayIndexOutOfRange(string id, string arrayName, int arrayLength, int invalidIndex) : 
            base(id, arrayLength == 0 ? "Array length is zero. Can't put anything inside the array." : $"Can't access index {invalidIndex} of array {arrayName}. The valid index range is 0 to {arrayLength - 1}.")
        {

        }
    }
}
