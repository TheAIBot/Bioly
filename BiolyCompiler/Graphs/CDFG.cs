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

        public Direct AddCDFG(CDFG cdfg, DFG<Block> from)
        {
            cdfg.Nodes.ForEach(x => this.Nodes.Add(x));

            Direct connection = new Direct(new Conditional(null, null, cdfg.StartDFG));
            //Nodes.Add((connection, from));

            return connection;
        }

        public CDFG Copy()
        {
            CDFG copy = new CDFG();
            Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys = new Dictionary<DFG<Block>, DFG<Block>>();
            foreach ((IControlBlock control, DFG<Block> dfg) in Nodes)
            {
                DFG<Block> copyDFG = null;
                if (knownDFGCopys.ContainsKey(dfg))
                {
                    copyDFG = knownDFGCopys[dfg];
                }
                else
                {
                    copyDFG = dfg.Copy();
                    knownDFGCopys.Add(dfg, copyDFG);
                }

                copy.AddNode(control?.Copy(copyDFG, knownDFGCopys), copyDFG);
            }

            copy.StartDFG = knownDFGCopys[StartDFG];
            return copy;
        }
    }
}
