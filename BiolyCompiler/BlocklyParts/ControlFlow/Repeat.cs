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
        public const string XmlTypeName = "controls_repeat_ext";
        private const string TimesBlockFieldName = "TIMES";
        private const string DoBlockFieldName = "DO";
        public readonly Conditional Cond;

        public Repeat(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            XmlNode conditionalNode = node.GetInnerBlockNode(TimesBlockFieldName, new MissingBlockException(id, "Repeat block is missing its conditional block."));
            VariableBlock decidingBlock = null;
            try
            {
                decidingBlock = (VariableBlock)XmlParser.ParseAndAddNodeToDFG(conditionalNode, dfg, mostRecentRef, parseExceptions);
            }
            catch (ParseException e)
            {
                parseExceptions.Add(e);
            }
            XmlNode guardedNode = node.GetInnerBlockNode(DoBlockFieldName, new MissingBlockException(id, "Repeat block is missing blocks to execute."));
            DFG<Block> guardedDFG = XmlParser.ParseDFG(guardedNode, cdfg, parseExceptions);
            DFG<Block> nextDFG = XmlParser.ParseNextDFG(node, cdfg, parseExceptions);

            this.Cond = new Conditional(decidingBlock, guardedDFG, nextDFG);
        }
    }
}
