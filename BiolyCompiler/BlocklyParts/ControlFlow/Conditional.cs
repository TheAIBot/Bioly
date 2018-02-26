using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.ControlFlow
{
    public class Conditional
    {
        public readonly Bool decidingNode;
        public readonly DFG<Block> dfg;
    }
}
