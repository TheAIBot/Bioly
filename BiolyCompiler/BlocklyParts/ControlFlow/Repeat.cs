using BiolyCompiler.Exceptions.ParserExceptions;
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
        public const string XML_TYPE_NAME = "controls_repeat_ext";
        public const string TimesBlockFieldName = "TIMES";
        public const string DoBlockFieldName = "DO";
        public readonly Conditional Cond;

        public Repeat(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            XmlNode conditionalNode = node.GetInnerBlockNode(TimesBlockFieldName, new MissingBlockException(id, "Repeat block is missing its conditional block."));
            VariableBlock decidingBlock = null;
            try
            {
                decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(conditionalNode, dfg, parserInfo);
            }
            catch (ParseException e)
            {
                parserInfo.parseExceptions.Add(e);
            }
            XmlNode guardedNode = node.GetInnerBlockNode(DoBlockFieldName, new MissingBlockException(id, "Repeat block is missing blocks to execute."));
            DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedNode, parserInfo);
            DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, parserInfo);

            this.Cond = new Conditional(decidingBlock, guardedDFG, nextDFG);
        }
    }
}
