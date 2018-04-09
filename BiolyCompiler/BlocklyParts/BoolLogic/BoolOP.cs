using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
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
        public const string XmlTypeName = "logic_compare";
        public readonly BoolOPTypes OPType;
        private readonly VariableBlock LeftBlock;
        private readonly VariableBlock RightBlock;

        public BoolOP(VariableBlock leftBlock, VariableBlock rightBlock, List<string> input, string output, XmlNode node) : base(false, input, output)
        {
            this.OPType = BoolOP.StringToBoolOPType(node.GetNodeWithAttributeValue(OPTypeFieldName).InnerText);
            this.LeftBlock = leftBlock;
            this.RightBlock = rightBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            XmlNode leftNode = node.GetNodeWithAttributeValue(LeftBoolFieldName).FirstChild;
            XmlNode rightNode = node.GetNodeWithAttributeValue(RightBoolFieldName).FirstChild;

            VariableBlock leftBoolBlock = (VariableBlock)XmlParser.ParseBlock(leftNode, dfg, mostRecentRef);
            VariableBlock rightBoolBlock = (VariableBlock)XmlParser.ParseBlock(rightNode, dfg, mostRecentRef);

            Node<Block> leftBoolNode = new Node<Block>(leftBoolBlock);
            Node<Block> rightBoolNode = new Node<Block>(rightBoolBlock);

            dfg.AddNode(leftBoolNode);
            dfg.AddNode(rightBoolNode);

            List<string> inputs = new List<string>();
            inputs.Add(leftBoolBlock.OutputVariable);
            inputs.Add(rightBoolBlock.OutputVariable);
            
            return new BoolOP(leftBoolBlock, rightBoolBlock, inputs, null, node);
        }

        public static BoolOPTypes StringToBoolOPType(string boolOPAsString)
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
                    throw new Exception("Failed to parse the boolean operator type.");
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
                    throw new Exception("Failed to parse the boolean operator type.");
            }
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            float leftResult = LeftBlock.Run(variables, executor);
            float rightResult = RightBlock.Run(variables, executor);

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
