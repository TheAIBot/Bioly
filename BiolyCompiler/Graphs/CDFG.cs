using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Blocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class CDFG
    {
        private readonly CFG<DFG<Block>> graph = new CFG<DFG<Block>>();
    }
}
