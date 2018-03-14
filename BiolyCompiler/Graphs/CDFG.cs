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
        public DFG<Block> StartDFG;

        public void AddNode(IControlBlock control, DFG<Block> dfg)
        {
            Nodes.Add((control, dfg));
        }
    }
}
