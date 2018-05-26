using BiolyCompiler.Graphs;
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


            this.Cond = new Conditional(null, null, nextDFG);
        }
    }
}
