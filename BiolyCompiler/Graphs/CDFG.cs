using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.ControlFlow;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Graphs
{
    public class CDFG
    {
        public readonly List<(IControlBlock control, DFG<Block> dfg)> Nodes = new List<(IControlBlock control, DFG<Block> dfg)>();
        public (IControlBlock control, DFG<Block> dfg) Start;

        public void AddNode(IControlBlock control, DFG<Block> dfg)
        {
            if (Nodes.Count == 0)
            {
                Start = (control, dfg);
            }

            Nodes.Add((control, dfg));
        }
    }
}
