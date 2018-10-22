using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
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

        public BoolOP(VariableBlock leftBlock, VariableBlock rightBlock, string output, BoolOPTypes opType, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { leftBlock?.OutputVariable, rightBlock?.OutputVariable }, output, id, canBeScheduled)
        {
            this.OPType = opType;
            this.LeftBlock = leftBlock;
            this.RightBlock = rightBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            BoolOPTypes opType = BoolOP.StringToBoolOPType(id, ParseTools.ParseString(node, OPTypeFieldName));

            VariableBlock leftBoolBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, LeftBoolFieldName,
                                          new MissingBlockException(id, "Left side of boolean operator is missing a block."));
            VariableBlock rightBoolBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, RightBoolFieldName,
                                           new MissingBlockException(id, "Right side of boolean operator is missing a block."));

            dfg.AddNode(leftBoolBlock);
            dfg.AddNode(rightBoolBlock);

            return new BoolOP(leftBoolBlock, rightBoolBlock, parserInfo.GetUniqueAnonymousName(), opType, id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock leftCopy = (VariableBlock)LeftBlock.TrueCopy(dfg);
            VariableBlock rightCopy = (VariableBlock)RightBlock.TrueCopy(dfg);

            dfg.AddNode(leftCopy);
            dfg.AddNode(rightCopy);

            return new BoolOP(leftCopy, rightCopy, OutputVariable, OPType, BlockID, CanBeScheduled);
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
                    throw new InternalParseException("Failed to parse the boolean operator type. Type: " + type.ToString());
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
                    throw new InternalRuntimeException("Failed to parse the operator type. Type: " + OPType.ToString());
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

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            LeftBlock.GetVariableTreeList(blocks);
            RightBlock.GetVariableTreeList(blocks);

            return blocks;
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
                    throw new InternalParseException("Failed to parse the operator type.");
            }
        }
    }
}
