using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Conditional
    {
        public readonly Block DecidingBlock;
        public readonly DFG<Block> GuardedDFG;
        public readonly DFG<Block> NextDFG;

        public Conditional(Block decidingBlock, DFG<Block> guardedDFG, DFG<Block> nextDFG)
        {
            this.DecidingBlock = decidingBlock;
            this.GuardedDFG = guardedDFG;
            this.NextDFG = nextDFG;
        }
    }
}
