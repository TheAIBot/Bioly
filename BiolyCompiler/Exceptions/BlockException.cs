using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Exceptions
{
    public class BlockException : Exception
    {
        public readonly string ID;

        public BlockException(string id, string message) : base(message)
        {
            this.BlockID = id;
        }
    }
}
