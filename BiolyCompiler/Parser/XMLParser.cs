using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.BlocklyParts.Declarations;
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
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.BlocklyParts.FluidicInputs;

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

            return (parserInfo.cdfg, parserInfo.ParseExceptions);
        }

        internal static DFG<Block> ParseDFG(XmlNode node, ParserInfo parserInfo, bool allowDeclarationBlocks = false, bool canFirstBlockBeControlFlow = true)
        {
            parserInfo.EnterDFG();
            try
            {
                IControlBlock controlBlock = null;
                var dfg = new DFG<Block>();
                var mostRecentRef = new Dictionary<string, string>();
                while (true)
                {
                    if (IsDFGBreaker(node, dfg) && canFirstBlockBeControlFlow)
                    {
                        controlBlock = ParseDFGBreaker(node, dfg, parserInfo);
                        break;
                    }

                    Block block = null;
                    try
                    {
                        block = ParseAndAddNodeToDFG(ref node, dfg, parserInfo, allowDeclarationBlocks);
                    }
                    catch (ParseException e)
                    {
                        parserInfo.ParseExceptions.Add(e);
                    }
                    allowDeclarationBlocks = block is DeclarationBlock && allowDeclarationBlocks;

                    //move on to the next node or exit if none
                    node = node.TryGetNodeWithName("next");
                    if (node == null)
                    {
                        break;
                    }
                    node = node.FirstChild;
                }

                if (parserInfo.ParseExceptions.Count == 0)
                {
                    dfg.FinishDFG();
                }
                parserInfo.cdfg.AddNode(controlBlock, dfg);

                parserInfo.LeftDFG();
                return dfg;
            }
            catch (ParseException e)
            {
                parserInfo.ParseExceptions.Add(e);
                parserInfo.LeftDFG();
                return null;
            }
        }

        internal static Block ParseAndAddNodeToDFG(ref XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool allowDeclarationBlocks = false)
        {
            Block block = ParseBlock(ref node, dfg, parserInfo, allowDeclarationBlocks);
            
            dfg.AddNode(block);

            //update map of most recent nodes that outputs the variable
            //so other nodes that get their value from the node that
            //just updated the value
            if (parserInfo.MostRecentVariableRef.ContainsKey(block.OriginalOutputVariable))
            {
                parserInfo.MostRecentVariableRef[block.OriginalOutputVariable] = block.OutputVariable;
            }
            else
            {
                parserInfo.MostRecentVariableRef.Add(block.OriginalOutputVariable, block.OutputVariable);
            }

            return block;
        }

        private static bool IsDFGBreaker(XmlNode node, DFG<Block> dfg)
        {
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            switch (blockType)
            {
                case If.XML_TYPE_NAME:
                case Repeat.XML_TYPE_NAME:
                    return true;
                case SetArrayFluid.XML_TYPE_NAME:
                case SetArrayNumber.XML_TYPE_NAME:
                    //case SensorUsage.XML_TYPE_NAME:
                    //these blocks needs to be at the start of a dfg
                    //so only break if this isn't an empty dfg
                    return dfg.Nodes.Count > 0;
                default:
                    return false;
            }
        }

        private static IControlBlock ParseDFGBreaker(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string blockType = node.Attributes[Block.TypeFieldName].Value;
            switch (blockType)
            {
                case If.XML_TYPE_NAME:
                    return new If(node, dfg, parserInfo);
                case Repeat.XML_TYPE_NAME:
                    return new Repeat(node, dfg, parserInfo);
                case SetArrayFluid.XML_TYPE_NAME:
                    return new Direct(node, dfg, parserInfo);
                case SetArrayNumber.XML_TYPE_NAME:
                    return new Direct(node, dfg, parserInfo);
                default:
                    throw new Exception("Invalid type: " + blockType);
            }
        }

        internal static DFG<Block> ParseNextDFG(XmlNode node, ParserInfo parserInfo)
        {
            node = node.TryGetNodeWithName("next");
            if (node == null)
            {
                return null;
            }

            node = node.FirstChild;
            return ParseDFG(node, parserInfo);
        }

        public static Block ParseBlock(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool allowDeclarationBlocks = false, bool canBeScheduled = true)
        {
            return ParseBlock(ref node, dfg, parserInfo, allowDeclarationBlocks, canBeScheduled);
        }

        public static Block ParseBlock(ref XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool allowDeclarationBlocks = false, bool canBeScheduled = true)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            switch (blockType)
            {
                case ArithOP.XML_TYPE_NAME:
                    return ArithOP.Parse(node, dfg, parserInfo, canBeScheduled);
                case Constant.XML_TYPE_NAME:
                    return Constant.Parse(node, canBeScheduled);
                case FluidArray.XML_TYPE_NAME:
                    return FluidArray.Parse(node, dfg, parserInfo);
                case SetArrayFluid.XML_TYPE_NAME:
                    return SetArrayFluid.Parse(node, dfg, parserInfo);
                case Fluid.XML_TYPE_NAME:
                    return Fluid.Parse(node, dfg, parserInfo);
                case InputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has to be at the top of the program.");
                    }
                    return InputDeclaration.Parse(node, parserInfo);
                case OutputDeclaration.XML_TYPE_NAME:
                    if (!allowDeclarationBlocks)
                    {
                        throw new ParseException(id, "Declaration blocks has to be at the top of the program.");
                    }
                    return OutputDeclaration.Parse(node, parserInfo);
                case OutputUseage.XML_TYPE_NAME:
                    return OutputUseage.Parse(node, dfg, parserInfo);
                case HeaterDeclaration.XML_TYPE_NAME:
                    return HeaterDeclaration.Parse(node, parserInfo);
                case Waste.XML_TYPE_NAME:
                    return Waste.Parse(node, dfg, parserInfo);
                case BoolOP.XML_TYPE_NAME:
                    return BoolOP.Parse(node, dfg, parserInfo, canBeScheduled);
                //case Sensor.XmlTypeName:
                //    return Sensor.Parse(node);
                case InlineProgram.XML_TYPE_NAME:
                    {
                        InlineProgram program = new InlineProgram(node, dfg, parserInfo);
                        program.AppendProgramXml(ref node, parserInfo);
                        return ParseBlock(ref node, dfg, parserInfo, allowDeclarationBlocks, canBeScheduled);
                    }
                case GetNumberVariable.XML_TYPE_NAME:
                    return GetNumberVariable.Parse(node, parserInfo, canBeScheduled);
                case SetNumberVariable.XML_TYPE_NAME:
                    return SetNumberVariable.Parse(node, dfg, parserInfo);
                case GetDropletCount.XML_TYPE_NAME:
                    return GetDropletCount.Parser(node, parserInfo, canBeScheduled);
                case GetFluidArrayLength.XML_TYPE_NAME:
                    return GetFluidArrayLength.Parse(node, parserInfo, canBeScheduled);
                case ImportVariable.XML_TYPE_NAME:
                    return ImportVariable.Parse(node, parserInfo, canBeScheduled);
                case NumberArray.XML_TYPE_NAME:
                    return NumberArray.Parse(node, dfg, parserInfo);
                case GetArrayNumber.XML_TYPE_NAME:
                    return GetArrayNumber.Parse(node, dfg, parserInfo, canBeScheduled);
                case SetArrayNumber.XML_TYPE_NAME:
                    return SetArrayNumber.Parse(node, dfg, parserInfo, canBeScheduled);
                case RoundOP.XML_TYPE_NAME:
                    return RoundOP.Parse(node, dfg, parserInfo, canBeScheduled);
                default:
                    throw new UnknownBlockException(id);
            }
        }

        public static FluidInput ParseFluidInput(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string blockType = node.GetAttributeValue(Block.TypeFieldName);
            switch (blockType)
            {
                case BasicInput.XML_TYPE_NAME:
                    return BasicInput.Parse(node, parserInfo, doVariableCheck);
                case GetArrayFluid.XML_TYPE_NAME:
                    return GetArrayFluid.Parse(node, dfg, parserInfo, doVariableCheck);
                default:
                    throw new UnknownBlockException(id);
            }
        }
    }
}
