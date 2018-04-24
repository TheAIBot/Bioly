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
        public const string XML_TYPE_NAME = "controls_if";
        public readonly IReadOnlyList<Conditional> IfStatements;

        public If(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            List<Conditional> conditionals = new List<Conditional>();

            int IfBlocksCount = 1;
            bool hasElse = false;

            XmlNode mutatorNode = node.TryGetNodeWithName("mutation");
            if (mutatorNode != null)
            {
                string elseifAttrib = mutatorNode.TryGetAttributeValue("elseif");
                if (elseifAttrib != null)
                {
                    IfBlocksCount += int.Parse(elseifAttrib);
                }

                hasElse = mutatorNode.TryGetAttributeValue("else") != null;
            }

            for (int ifCounter = 0; ifCounter < IfBlocksCount; ifCounter++)
            {
                string exceptionStart = $"{ (ifCounter == 0 ? "If" : "Else if") } statement { (ifCounter == 0 ? String.Empty : $"Number {ifCounter}")}";

                XmlNode ifNode = null;
                VariableBlock decidingBlock = null;
                try
                {
                    ifNode = node.GetInnerBlockNode($"IF{ifCounter}", new MissingBlockException(id, $"{exceptionStart} is missing its conditional block."));
                    decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(ifNode, dfg, mostRecentRef, parseExceptions);
                }
                catch (ParseException e)
                {
                    parseExceptions.Add(e);
                }

                XmlNode guardedDFGNode = null;
                try
                {
                    guardedDFGNode = node.GetInnerBlockNode($"DO{ifCounter}", new MissingBlockException(id, $"{exceptionStart} is missing blocks to execute."));
                    DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg, parseExceptions);
                    DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg, parseExceptions);

                    conditionals.Add(new Conditional(decidingBlock, guardedDFG, nextDFG));
                }
                catch (ParseException e)
                {
                    parseExceptions.Add(e);
                }
            }

            if (hasElse)
            {
                XmlNode guardedDFGNode = node.GetInnerBlockNode("ELSE", new MissingBlockException(id, "Else statement is missing blocks to execute"));
                DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, cdfg, parseExceptions);
                DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg, parseExceptions);

                conditionals.Add(new Conditional(null, guardedDFG, nextDFG));
            }

            this.IfStatements = conditionals;
        }
    }
}
