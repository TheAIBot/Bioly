using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class RoundOP : VariableBlock
    {
        public const string OPTypeFieldName = "OP";
        public const string NUMBER_FIELD_NAME = "NUM";
        public const string XML_TYPE_NAME = "math_round";
        public readonly VariableBlock NumberBlock;
        public readonly RoundOPTypes RoundType;

        public RoundOP(VariableBlock numberBlock, string output, RoundOPTypes roundType, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { numberBlock?.OutputVariable }, output, id, canBeScheduled)
        {
            this.NumberBlock = numberBlock;
            this.RoundType = roundType;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            RoundOPTypes roundType = StringToRoundOPType(id, ParseTools.ParseString(node, OPTypeFieldName));

            VariableBlock numberBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, NUMBER_FIELD_NAME,
                                        new MissingBlockException(id, "Number defining block is missing."));

            dfg.AddNode(numberBlock);

            return new RoundOP(numberBlock, parserInfo.GetUniqueAnonymousName(), roundType, id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock numberCopy = (VariableBlock)NumberBlock.TrueCopy(dfg);

            dfg.AddNode(numberCopy);

            return new RoundOP(numberCopy, OutputVariable, RoundType, BlockID, CanBeScheduled);
        }

        public static RoundOPTypes StringToRoundOPType(string id, string roundOPTypeAsString)
        {
            switch (roundOPTypeAsString)
            {
                case "ROUND":
                    return RoundOPTypes.ROUND;
                case "ROUNDDOWN":
                    return RoundOPTypes.ROUNDDOWN;
                case "ROUNDUP":
                    return RoundOPTypes.ROUNDUP;
                default:
                    throw new InternalParseException(id, $"Unknown round operator.{Environment.NewLine}Expected either ROUND, ROUNDUP or ROUNDDOWN but operator was {roundOPTypeAsString}.");
            }
        }

        public static string RoundOpTypeToString(RoundOPTypes type)
        {
            switch (type)
            {
                case RoundOPTypes.ROUND:
                    return "ROUND";
                case RoundOPTypes.ROUNDDOWN:
                    return "ROUNDDOWN";
                case RoundOPTypes.ROUNDUP:
                    return "ROUNDUP";
                default:
                    throw new InternalParseException("Failed to parse the round operator type. Type: " + type.ToString());
            }
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            float result = NumberBlock.Run(variables, executor, dropPositions);
            switch (RoundType)
            {
                case RoundOPTypes.ROUND:
                    return (float)Math.Round(result);
                case RoundOPTypes.ROUNDDOWN:
                    return (float)Math.Floor(result);
                case RoundOPTypes.ROUNDUP:
                    return (float)Math.Ceiling(result);
                default:
                    throw new InternalRuntimeException("Failed to parse the round operator type. Type: " + RoundType.ToString());
            }
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" + 
                $"<field name=\"OP\">ROUNDDOWN</field>" +
                $"<value name=\"NUM\">" +
                "</value>" +
            "</block>";
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            NumberBlock.GetVariableTreeList(blocks);

            return blocks;
        }

        public override string ToString()
        {
            return "Rounding " + RoundType.ToString();
        }
    }
}
