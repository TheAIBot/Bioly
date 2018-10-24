using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Direct : IControlBlock
    {
        public readonly Conditional Cond;

        public Direct(Conditional cond)
        {
            this.Cond = cond;
        }

        public Direct(XmlNode node, ParserInfo parserInfo)
        {
            DFG<Block> nextDFG = XmlParser.ParseDFG(node, parserInfo, false, false);

            this.Cond = new Conditional(null, null, nextDFG);
        }

        public Direct(DFG<Block> nextDFG) : this(new Conditional(null, null, nextDFG))
        {
        }

        public DFG<Block> GuardedDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return null;
        }

        public DFG<Block> NextDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return Cond.NextDFG;
        }

        public DFG<Block> TryLoop<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return null;
        }

        public IControlBlock Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys)
        {
            return new Direct(Cond.Copy(dfg, knownDFGCopys));
        }

        public IEnumerator<DFG<Block>> GetEnumerator()
        {
            yield return Cond.NextDFG;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IControlBlock GetNewControlWithNewEnd(DFG<Block> dfg)
        {
            return new Direct(new Conditional(Cond.DecidingBlock, Cond.GuardedDFG, dfg));
        }

        public DFG<Block> GetEndDFG()
        {
            return Cond.NextDFG;
        }
    }
}
