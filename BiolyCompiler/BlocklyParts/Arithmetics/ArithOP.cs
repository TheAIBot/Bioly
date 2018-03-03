﻿using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class ArithOP : Block
    {
        private const string OPTypeName = "OP";
        private const string LeftArithName = "A";
        private const string RightArithName = "B";
        public const string XmlTypeName = "math_arithmetic";
        public readonly ArithOPTypes OPType;

        public ArithOP(List<string> input, string output, XmlNode node) : base(false, input, output)
        {
            switch (node.GetNodeWithAttributeValue(OPTypeName).InnerText)
            {
                case "ADD":
                    this.OPType = ArithOPTypes.ADD;
                    break;
                case "SUB":
                    this.OPType = ArithOPTypes.SUB;
                    break;
                case "MUL":
                    this.OPType = ArithOPTypes.MUL;
                    break;
                case "DIV":
                    this.OPType = ArithOPTypes.DIV;
                    break;
                default:
                    throw new Exception("Failed to parse the operator type.");
            }
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg)
        {
            XmlNode leftNode = node.GetNodeWithAttributeValue(LeftArithName).FirstChild;
            XmlNode rightNode = node.GetNodeWithAttributeValue(RightArithName).FirstChild;

            Block leftArithBlock = XMLParser.ParseBlock(leftNode, dfg);
            Block rightArithBlock = XMLParser.ParseBlock(rightNode, dfg);

            Node<Block> leftArithNode = new Node<Block>();
            Node<Block> rightArithNode = new Node<Block>();
            leftArithNode.value = leftArithBlock;
            rightArithNode.value = rightArithBlock;

            dfg.AddNode(leftArithNode);
            dfg.AddNode(rightArithNode);

            List<string> inputs = new List<string>();
            inputs.Add(leftArithBlock.OutputVariable);
            inputs.Add(rightArithBlock.OutputVariable);

            string output = XMLParser.CreateName();
            return new ArithOP(inputs, output, node);
        }
    }
}
