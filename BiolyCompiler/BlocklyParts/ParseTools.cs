using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts
{
    public static class ParseTools
    {
        public static T ParseBlock<T>(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, string id, string nodeName, ParseException exception) where T : Block
        {
            XmlNode leftNode = node.GetInnerBlockNode(nodeName, parserInfo, exception);
            if (leftNode != null)
            {
                return (T)XmlParser.ParseBlock(leftNode, dfg, parserInfo, false, false);
            }

            return null;
        }

        public static FluidInput ParseFluidInput(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, string id, string nodeName, ParseException exception)
        {
            XmlNode inputFluidNode = node.GetInnerBlockNode(nodeName, parserInfo, exception);
            if (inputFluidNode != null)
            {
                return XmlParser.ParseFluidInput(inputFluidNode, dfg, parserInfo);
            }

            return null;
        }

        public static string ParseString(XmlNode node, string nodeName)
        {
            return node.GetNodeWithAttributeValue(nodeName).InnerText;
        }

        public static string ParseID(XmlNode node)
        {
            return node.GetAttributeValue(Block.ID_FIELD_NAME);
        }

        public static float ParseFloat(XmlNode node, ParserInfo parserInfo, string id, string nodeName)
        {
            XmlNode numberNode = node.GetNodeWithAttributeValue(nodeName);
            return ParseFloat(numberNode, parserInfo, id);
        }

        public static float ParseFloat(XmlNode node, ParserInfo parserInfo, string id)
        {
            return node.TextToFloat(id, parserInfo);
        }
    }
}
