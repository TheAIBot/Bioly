using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class Repeat : IControlBlock
    {
        public const string XmlTypeName = "controls_repeat_ext";
        public readonly int Times;
        public readonly Conditional Cond;

        public Repeat(XmlNode node, CDFG cdfg, DFG<Block> dfg)
        {
            this.Times = node.GetNodeWithName("value").TextToInt();

            XmlNode guardedNode = node.GetNodeWithName("statement").FirstChild;
            DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedNode, cdfg);
            DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg);

            this.Cond = new Conditional(null, guardedDFG, nextDFG);
        }
    }
}
