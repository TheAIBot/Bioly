using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Conditional
    {
        public readonly VariableBlock DecidingBlock;
        public readonly DFG<Block> GuardedDFG;
        public readonly DFG<Block> NextDFG;

        public Conditional(VariableBlock decidingBlock, DFG<Block> guardedDFG, DFG<Block> nextDFG)
        {
            this.DecidingBlock = decidingBlock;
            this.GuardedDFG = guardedDFG;
            this.NextDFG = nextDFG;
        }


        public Conditional Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys)
        {
            VariableBlock copyDeciding = null;
            if (DecidingBlock != null)
            {
                copyDeciding = (VariableBlock)dfg.Nodes.Single(x => DecidingBlock.OutputVariable == x.value.OutputVariable).value;
            }

            DFG<Block> copyGuarded = null;
            if (GuardedDFG != null && knownDFGCopys.ContainsKey(GuardedDFG))
            {
                copyGuarded = knownDFGCopys[GuardedDFG];
            }
            else if (GuardedDFG != null)
            {
                copyGuarded = GuardedDFG.Copy();
                knownDFGCopys.Add(GuardedDFG, copyGuarded);
            }

            DFG<Block> copyNext = null;
            if (NextDFG != null && knownDFGCopys.ContainsKey(NextDFG))
            {
                copyNext = knownDFGCopys[NextDFG];
            }
            else if (NextDFG != null)
            {
                copyNext = NextDFG.Copy();
                knownDFGCopys.Add(NextDFG, copyNext);
            }

            return new Conditional(copyDeciding, copyGuarded, copyNext);
        }
    }
}
