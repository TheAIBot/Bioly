using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.BoolLogic
{
    public class BoolOP : Block
    {
        private const string OPTypeName = "OP";
        private const string LeftBoolName = "A";
        private const string RightBoolName = "B";
        public const string XmlTypeName = "logic_compare";
        public readonly BoolOPTypes OPType;

        public BoolOP(List<string> input, string output, XmlNode node) : base(false, input, output)
        {
            switch (node.GetNodeWithAttributeValue(OPTypeName).InnerText)
            {
                case "EQ":
                    this.OPType = BoolOPTypes.EQ;
                    break;
                case "NEQ":
                    this.OPType = BoolOPTypes.NEQ;
                    break;
                case "LT":
                    this.OPType = BoolOPTypes.LT;
                    break;
                case "LTE":
                    this.OPType = BoolOPTypes.LTE;
                    break;
                case "GT":
                    this.OPType = BoolOPTypes.GT;
                    break;
                case "GTE":
                    this.OPType = BoolOPTypes.GTE;
                    break;
                default:
                    throw new Exception("Failed to parse the operator type.");
            }
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            XmlNode leftNode = node.GetNodeWithAttributeValue(LeftBoolName).FirstChild;
            XmlNode rightNode = node.GetNodeWithAttributeValue(RightBoolName).FirstChild;

            Block leftBoolBlock = XmlParser.ParseBlock(leftNode, dfg, mostRecentRef);
            Block rightBoolBlock = XmlParser.ParseBlock(rightNode, dfg, mostRecentRef);

            Node<Block> leftBoolNode = new Node<Block>();
            Node<Block> rightBoolNode = new Node<Block>();
            leftBoolNode.value = leftBoolBlock;
            rightBoolNode.value = rightBoolBlock;

            dfg.AddNode(leftBoolNode);
            dfg.AddNode(rightBoolNode);

            List<string> inputs = new List<string>();
            inputs.Add(leftBoolBlock.OutputVariable);
            inputs.Add(rightBoolBlock.OutputVariable);
            
            return new BoolOP(inputs, null, node);
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
