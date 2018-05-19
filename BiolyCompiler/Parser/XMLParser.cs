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

            ParserInfo parserInfo = new ParserInfo();
            DFG<Block> startDFG = ParseDFG(node, parserInfo, true);
            parserInfo.cdfg.StartDFG = startDFG;

            return (parserInfo.cdfg, parserInfo.parseExceptions);
        }

        internal static DFG<Block> ParseDFG(XmlNode node, ParserInfo parserInfo, bool allowDeclarationBlocks = false)
        {
            parserInfo.EnterDFG();
            try
            {
                IControlBlock controlBlock = null;
                var dfg = new DFG<Block>();
                var mostRecentRef = new Dictionary<string, string>();
                while (true)
                {
                    if (IsConditional(node))
                    {
                        controlBlock = ParseConditionalBlocks(node, dfg, parserInfo);
                        break;
                    }

                    Block block = null;
                    try
                    {
                        block = ParseAndAddNodeToDFG(node, dfg, parserInfo, allowDeclarationBlocks);
                    }
                    catch (ParseException e)
                    {
                        parserInfo.parseExceptions.Add(e);
                    }
                    allowDeclarationBlocks = block is StaticDeclarationBlock && allowDeclarationBlocks;

                    //move on to the next node or exit if none
                    node = node.TryGetNodeWithName("next");
                    if (node == null)
                    {
                        break;
                    }
                    node = node.FirstChild;
                }

                if (parserInfo.parseExceptions.Count == 0)
                {
                    dfg.FinishDFG();
                }
                parserInfo.cdfg.AddNode(controlBlock, dfg);

                parserInfo.LeftDFG();
                return dfg;
            }
            catch (ParseException e)
            {
                parserInfo.parseExceptions.Add(e);
                parserInfo.LeftDFG();
                return null;
            }
        }

        internal static Block ParseAndAddNodeToDFG(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool allowDeclarationBlocks = false)
        {
            Block block = ParseBlock(node, dfg, parserInfo, allowDeclarationBlocks);
            
            dfg.AddNode(block);

            //update map of most recent nodes that outputs the variable
            //so other nodes that get their value from the node that
            //just updated the value
            if (parserInfo.mostRecentVariableRef.ContainsKey(block.OriginalOutputVariable))
            {
                parserInfo.mostRecentVariableRef[block.OriginalOutputVariable] = block.OutputVariable;
            }
            else
            {
                parserInfo.mostRecentVariableRef.Add(block.OriginalOutputVariable, block.OutputVariable);
                parserInfo.AddFluidVariable(block.OriginalOutputVariable);
            }

            return block;
        }

        private static IControlBlock ParseConditionalBlocks(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string blockType = node.Attributes["type"].Value;
            switch (blockType)
            {
                case If.XML_TYPE_NAME:
                    return new If(node, dfg, parserInfo);
                case Repeat.XML_TYPE_NAME:
                    return new Repeat(node, dfg, parserInfo);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }

        internal static DFG<Block> ParseNextDFG(XmlNode node, ParserInfo parserInfo)
        {
            node = Extensions.TryGetNodeWithName(node, "next");
            if (node == null)
            {
                return null;
            }

            node = node.FirstChild;
            return ParseDFG(node, parserInfo);
        }

        private static bool IsConditional(XmlNode node)
        {
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            return blockType == If.XML_TYPE_NAME || blockType == Repeat.XML_TYPE_NAME;
        }

        public static Block ParseBlock(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool allowDeclarationBlocks = false)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            switch (blockType)
            {
                case ArithOP.XML_TYPE_NAME:
                    return ArithOP.Parse(node, dfg, parserInfo);
                case Constant.XML_TYPE_NAME:
                    return Constant.Parse(node);
                //case FluidArray.XmlTypeName:
                //    return FluidArray.Parse(node);
                //case SetFluidArray.XmlTypeName:
                //    return SetFluidArray.Parse(node);
                case Fluid.XML_TYPE_NAME:
                    return Fluid.Parse(node, parserInfo);
                case InputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has be at the top of the program.");
                    }
                    return InputDeclaration.Parse(node, parserInfo);
                case OutputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has be at the top of the program.");
                    }
                    return OutputDeclaration.Parse(node, parserInfo);
                case OutputUseage.XML_TYPE_NAME:
                    return OutputUseage.Parse(node, parserInfo);
                case HeaterDeclaration.XML_TYPE_NAME:
                    return HeaterDeclaration.Parse(node, parserInfo);
                case Waste.XML_TYPE_NAME:
                    return Waste.Parse(node, parserInfo);
                case BoolOP.XML_TYPE_NAME:
                    return BoolOP.Parse(node, dfg, parserInfo);
                //case Sensor.XmlTypeName:
                //    return Sensor.Parse(node);
                default:
                    throw new UnknownBlockException(id);
            }
        }

        internal static FluidInput GetVariablesCorrectedName(XmlNode node, ParserInfo parserInfo)
        {
            return new FluidInput(node, parserInfo);
        }
    }
}
