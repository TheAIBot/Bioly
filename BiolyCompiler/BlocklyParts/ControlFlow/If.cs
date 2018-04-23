using BiolyCompiler.Exceptions.ParserExceptions;
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

        public If(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            List<Conditional> conditionals = new List<Conditional>();

            int ifCounter = 0;
            XmlNode ifNode = node.TryGetNodeWithAttributeValue($"IF{ifCounter}");
            if (ifNode == null)
            {
                throw new MissingBlockException(id, "Missing blocks to decide if the if statement will run.");
            }
            while (ifNode != null)
            {
                XmlNode decidingNode = ifNode.FirstChild;
                VariableBlock decidingBlock = null;
                try
                {
                    decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(decidingNode, dfg, mostRecentRef, parseExceptions);
                }
                catch (ParseException e)
                {
                    parseExceptions.Add(e);
                }

                XmlNode guardedDFGNode = node.GetInnerBlockNode($"DO{ifCounter}", new MissingBlockException(id, $"{(ifCounter == 0 ? "If" : "Else if")} statement {(ifCounter == 0 ? String.Empty : $"Number {ifCounter}")} is missing blocks to execute."));
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg, parseExceptions);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg, parseExceptions);

                conditionals.Add(new Conditional(decidingBlock, guardedDFG, nextDFG));

                ifCounter++;
                ifNode = node.TryGetNodeWithAttributeValue($"IF{ifCounter}");
            }

            XmlNode edgeNode = node.TryGetNodeWithAttributeValue("ELSE");
            if (edgeNode != null)
            {
                XmlNode guardedDFGNode = node.GetInnerBlockNode($"DO{ifCounter}", new MissingBlockException(id, "Else statement is missing blocks to execute."));
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg, parseExceptions);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg, parseExceptions);

                conditionals.Add(new Conditional(null, guardedDFG, nextDFG));
            }
            this.IfStatements = conditionals;
        }
    }
}
