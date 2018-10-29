using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.ControlFlow;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void AddCDFG(CDFG cdfg)
        {
            foreach (var item in cdfg.Nodes)
            {
                Nodes.Add(item);
            }
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

        public DFG<Block> GetEndDFGInFirstScope()
        {
            DFG<Block> endDFG = StartDFG;
            while (endDFG != null)
            {
                IControlBlock control = Nodes.Single(x => x.dfg == endDFG).control;
                if (control != null && control.GetEndDFG() != null)
                {
                    endDFG = control.GetEndDFG();
                }
                else
                {
                    break;
                }
            }

            return endDFG;
        }
    }
}
