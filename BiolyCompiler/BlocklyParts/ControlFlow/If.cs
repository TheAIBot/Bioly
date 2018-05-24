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

        public If(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
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

                VariableBlock decidingBlock = null;
                XmlNode ifNode = node.GetInnerBlockNode(GetIfFieldName(ifCounter), parserInfo, new MissingBlockException(id, $"{exceptionStart} is missing its conditional block."));
                if (ifNode != null)
                {
                    decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(ref ifNode, dfg, parserInfo);
                }

                XmlNode guardedDFGNode = node.GetInnerBlockNode(GetDoFieldName(ifCounter), parserInfo, new MissingBlockException(id, $"{exceptionStart} is missing blocks to execute."));
                if (guardedDFGNode != null)
                {
                    DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, parserInfo);
                    DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, parserInfo);

                    conditionals.Add(new Conditional(decidingBlock, guardedDFG, nextDFG));
                }
            }

            if (hasElse)
            {
                XmlNode guardedDFGNode = node.GetInnerBlockNode("ELSE", parserInfo, new MissingBlockException(id, "Else statement is missing blocks to execute"));
                if (guardedDFGNode != null)
                {
                    DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedDFGNode, parserInfo);
                    DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, parserInfo);

                    conditionals.Add(new Conditional(null, guardedDFG, nextDFG));
                }
            }

            this.IfStatements = conditionals;
        }

        public static string GetIfFieldName(int ifCounter = 0)
        {
            return $"IF{ifCounter}";
        }

        public static string GetDoFieldName(int ifCounter = 0)
        {
            return $"DO{ifCounter}";
        }
    }
}
