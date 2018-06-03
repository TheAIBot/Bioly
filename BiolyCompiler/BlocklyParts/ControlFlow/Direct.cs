using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Direct : IControlBlock
    {
        public readonly Conditional Cond;

        public Direct(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            //XmlParser.ParseAndAddNodeToDFG(ref node, dfg, parserInfo);

            DFG<Block> nextDFG = XmlParser.ParseDFG(node, parserInfo, false, false);

            this.Cond = new Conditional(null, null, nextDFG);
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
    }
}
