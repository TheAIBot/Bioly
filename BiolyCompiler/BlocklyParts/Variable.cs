using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class Variable
    {
        private readonly List<IBlockly> Getters = new List<IBlockly>();
        private readonly List<IBlockly> Setters = new List<IBlockly>();

        public void AddGetter(IBlockly block)
        {
            Getters.Add(block);
        }

        public void AddSetter(IBlockly block)
        {
            Setters.Add(block);
        }
    }
}
