using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public class If : IControlBlock
    {
        public const string XmlTypeName = "controls_if";
        public readonly IReadOnlyList<Conditional> IfStatements;

        public If(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            List<Conditional> conditionals = new List<Conditional>();

            int ifCounter = 0;
            XmlNode ifNode = node.GetNodeWithAttributeValue($"IF{ifCounter}");
            while (ifNode != null)
            {
                XmlNode decidingNode = ifNode.FirstChild;
                VariableBlock decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(decidingNode, dfg, mostRecentRef);
                XmlNode guardedDFGNode = node.GetNodeWithAttributeValue($"DO{ifCounter}").FirstChild;
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg);

                conditionals.Add(new Conditional(decidingBlock, guardedDFG, nextDFG));

                ifCounter++;
                ifNode = node.GetNodeWithAttributeValue($"IF{ifCounter}");
            }

            XmlNode edgeNode = node.GetNodeWithAttributeValue("ELSE");
            if (edgeNode != null)
            {
                XmlNode guardedDFGNode = node.GetNodeWithAttributeValue($"DO{ifCounter}").FirstChild;
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg);

                conditionals.Add(new Conditional(null, guardedDFG, nextDFG));
            }
            this.IfStatements = conditionals;
        }
    }
}
