using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts.BLocks.FFUs
{
    public abstract class FFU : IBlockly
    {
        public abstract bool TryDeserialize();

        public abstract string GetName();
    }
}
