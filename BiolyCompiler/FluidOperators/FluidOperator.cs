using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.FluidOperators
{
    public abstract class FluidOperator
    {
        public abstract bool TryDeserialize();

        public abstract string GetName();
    }
}
