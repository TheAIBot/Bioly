using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Threading;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.ControlFlow;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.Parser
{
    public static class XmlParser
    {
        public static (CDFG, List<ParseException>) Parse(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            XmlNode node;
            try
            {
                node = xmlDocument.FirstChild.GetNodeWithName("block").FirstChild.FirstChild;
            }
            catch (Exception)
            {
                throw new MissingBlockException("", "Missing start block.");
            }

            CDFG cdfg = new CDFG();
            List<ParseException> parseExceptions = new List<ParseException>();

            DFG<Block> startDFG = ParseDFG(node, cdfg, parseExceptions, true);
            cdfg.StartDFG = startDFG;

            return (cdfg, parseExceptions);
        }

        internal static DFG<Block> ParseDFG(XmlNode node, CDFG cdfg, List<ParseException> parseExceptions, bool allowDeclarationBlocks = false)
        {
            try
            {
                IControlBlock controlBlock = null;
                var dfg = new DFG<Block>();
                var mostRecentRef = new Dictionary<string, string>();
                while (true)
                {
                    if (IsConditional(node))
                    {
                        controlBlock = ParseConditionalBlocks(node, cdfg, dfg, mostRecentRef, parseExceptions);
                        break;
                    }
                    try
                    {
                        Block block = ParseAndAddNodeToDFG(node, dfg, mostRecentRef, parseExceptions, allowDeclarationBlocks);
                        allowDeclarationBlocks = block is StaticDeclarationBlock && allowDeclarationBlocks;
                    }
                    catch (ParseException e)
                    {
                        parseExceptions.Add(e);
                    }

                    //move on to the next node or exit if none
                    node = node.TryGetNodeWithName("next");
                    if (node == null)
                    {
                        break;
                    }
                    node = node.FirstChild;
                }

                if (parseExceptions.Count == 0)
                {
                    dfg.FinishDFG();
                }
                cdfg.AddNode(controlBlock, dfg);

                return dfg;
            }
            catch (ParseException e)
            {
                parseExceptions.Add(e);
                return null;
            }
        }

        internal static Block ParseAndAddNodeToDFG(XmlNode node, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions, bool allowDeclarationBlocks = false)
        {
            Block block = ParseBlock(node, dfg, mostRecentRef, parseExceptions, allowDeclarationBlocks);
            
            dfg.AddNode(block);

            //update map of most recent nodes that outputs the variable
            //so other nodes that get their value from the node that
            //just updated the value
            if (mostRecentRef.ContainsKey(block.OriginalOutputVariable))
            {
                mostRecentRef[block.OriginalOutputVariable] = block.OutputVariable;
            }
            else
            {
                mostRecentRef.Add(block.OriginalOutputVariable, block.OutputVariable);
            }

            return block;
        }

        private static IControlBlock ParseConditionalBlocks(XmlNode node, CDFG cdfg, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case If.XML_TYPE_NAME:
                    return new If(node, cdfg, dfg, mostRecentRef, parseExceptions);
                case Repeat.XML_TYPE_NAME:
                    return new Repeat(node, cdfg, dfg, mostRecentRef, parseExceptions);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }

        internal static DFG<Block> ParseNextDFG(XmlNode node, CDFG cdfg, List<ParseException> parseExceptions)
        {
            node = Extensions.TryGetNodeWithName(node, "next");
            if (node == null)
            {
                return null;
            }

            node = node.FirstChild;
            return ParseDFG(node, cdfg, parseExceptions);
        }

        private static bool IsConditional(XmlNode node)
        {
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            return blockType == If.XML_TYPE_NAME || blockType == Repeat.XML_TYPE_NAME;
        }

        public static Block ParseBlock(XmlNode node, DFG<Block> dfg, Dictionary<string, string> mostRecentRef, List<ParseException> parseExceptions, bool allowDeclarationBlocks = false)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            switch (blockType)
            {
                case ArithOP.XML_TYPE_NAME:
                    return ArithOP.Parse(node, dfg, mostRecentRef, parseExceptions);
                case Constant.XML_TYPE_NAME:
                    return Constant.Parse(node);
                //case FluidArray.XmlTypeName:
                //    return FluidArray.Parse(node);
                //case SetFluidArray.XmlTypeName:
                //    return SetFluidArray.Parse(node);
                case Fluid.XML_TYPE_NAME:
                    return Fluid.Parse(node, mostRecentRef);
                case InputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has be at the top of the program.");
                    }
                    return InputDeclaration.Parse(node);
                case OutputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has be at the top of the program.");
                    }
                    return OutputDeclaration.Parse(node, mostRecentRef);
                case OutputUseage.XML_TYPE_NAME:
                    return OutputUseage.Parse(node, mostRecentRef);
                case Waste.XML_TYPE_NAME:
                    return Waste.Parse(node, mostRecentRef);
                case BoolOP.XML_TYPE_NAME:
                    return BoolOP.Parse(node, dfg, mostRecentRef, parseExceptions);
                //case Sensor.XmlTypeName:
                //    return Sensor.Parse(node);
                default:
                    throw new UnknownBlockException(id);
            }
        }

        internal static FluidInput GetVariablesCorrectedName(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            return new FluidInput(node, mostRecentRef);
        }
    }
}
