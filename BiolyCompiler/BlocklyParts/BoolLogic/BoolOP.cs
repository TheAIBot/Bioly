using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.BoolLogic
{
    public class BoolOP : VariableBlock
    {
        public const string OPTypeFieldName = "OP";
        public const string LeftBoolFieldName = "A";
        public const string RightBoolFieldName = "B";
        public const string XML_TYPE_NAME = "logic_compare";
        public readonly BoolOPTypes OPType;
        private readonly VariableBlock LeftBlock;
        private readonly VariableBlock RightBlock;

        public BoolOP(VariableBlock leftBlock, VariableBlock rightBlock, List<string> input, string output, XmlNode node, string id, bool canBeScheduled) : base(false, input, output, id, canBeScheduled)
        {
            this.OPType = BoolOP.StringToBoolOPType(id, node.GetNodeWithAttributeValue(OPTypeFieldName).InnerText);
            this.LeftBlock = leftBlock;
            this.RightBlock = rightBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            VariableBlock leftBoolBlock = null;
            VariableBlock rightBoolBlock = null;

            XmlNode leftNode = node.GetInnerBlockNode(LeftBoolFieldName, parserInfo, new MissingBlockException(id, "Left side of boolean operator is missing a block."));
            if (leftNode != null)
            {
                leftBoolBlock = (VariableBlock)XmlParser.ParseBlock(leftNode, dfg, parserInfo, false, false);
            }

            XmlNode rightNode = node.GetInnerBlockNode(RightBoolFieldName, parserInfo, new MissingBlockException(id, "Right side of boolean operator is missing a block."));
            if (rightNode != null)
            {
                rightBoolBlock = (VariableBlock)XmlParser.ParseBlock(rightNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(leftBoolBlock);
            dfg.AddNode(rightBoolBlock);

            List<string> inputs = new List<string>();
            inputs.Add(leftBoolBlock?.OutputVariable);
            inputs.Add(rightBoolBlock?.OutputVariable);

            return new BoolOP(leftBoolBlock, rightBoolBlock, inputs, null, node, id, canBeScheduled);
        }

        public static BoolOPTypes StringToBoolOPType(string id, string boolOPAsString)
        {
            switch (boolOPAsString)
            {
                case "EQ":
                    return BoolOPTypes.EQ;
                case "NEQ":
                    return BoolOPTypes.NEQ;
                case "LT":
                    return BoolOPTypes.LT;
                case "LTE":
                    return BoolOPTypes.LTE;
                case "GT":
                    return BoolOPTypes.GT;
                case "GTE":
                    return BoolOPTypes.GTE;
                default:
                    throw new InternalParseException(id, $"Unknown boolean operator.{Environment.NewLine}Expected  either EQ, NEQ, LT, LTE, GT or GTE but operator was {boolOPAsString}.");
            }
        }

        public static string BoolOpTypeToString(BoolOPTypes type)
        {
            switch (type)
            {
                case BoolOPTypes.EQ:
                    return "EQ";
                case BoolOPTypes.NEQ:
                    return "NEQ";
                case BoolOPTypes.LT:
                    return "LT";
                case BoolOPTypes.LTE:
                    return "LTE";
                case BoolOPTypes.GT:
                    return "GT";
                case BoolOPTypes.GTE:
                    return "GTE";
                default:
                    throw new InternalParseException("Failed to parse the boolean operator type.");
            }
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            float leftResult = LeftBlock.Run(variables, executor, dropPositions);
            float rightResult = RightBlock.Run(variables, executor, dropPositions);

            switch (OPType)
            {
                case BoolOPTypes.EQ:
                    return leftResult == rightResult ? 1 : 0;
                case BoolOPTypes.NEQ:
                    return leftResult != rightResult ? 1 : 0;
                case BoolOPTypes.LT:
                    return leftResult <  rightResult ? 1 : 0;
                case BoolOPTypes.LTE:
                    return leftResult <= rightResult ? 1 : 0;
                case BoolOPTypes.GT:
                    return leftResult >  rightResult ? 1 : 0;
                case BoolOPTypes.GTE:
                    return leftResult >= rightResult ? 1 : 0;
                default:
                    throw new Exception("Failed to parse the operator type.");
            }
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{OPTypeFieldName}\">{BoolOpTypeToString(OPType)}</field>" +
                $"<value name=\"{LeftBlock}\">" +
                    LeftBlock.ToXml() +
                "</value>" +
                $"<value name=\"{RightBlock}\">" +
                    RightBlock.ToXml() +
                "</value>" +
            "</block>";
        }

        public override string ToString()
        {
            switch (OPType)
            {
                case BoolOPTypes.EQ:
                    return "==";
                case BoolOPTypes.NEQ:
                    return "!=";
                case BoolOPTypes.LT:
                    return "<";
                case BoolOPTypes.LTE:
                    return "<=";
                case BoolOPTypes.GT:
                    return ">";
                case BoolOPTypes.GTE:
                    return ">=";
                default:
                    throw new Exception("Failed to parse the operator type.");
            }
        }
    }
}
