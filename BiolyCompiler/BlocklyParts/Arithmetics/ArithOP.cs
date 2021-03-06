﻿using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class ArithOP : VariableBlock
    {
        public const string OPTypeFieldName = "OP";
        public const string LeftArithFieldName = "A";
        public const string RightArithFieldName = "B";
        public const string XML_TYPE_NAME = "math_arithmetic";
        public readonly ArithOPTypes OPType;
        private readonly VariableBlock LeftBlock;
        private readonly VariableBlock RightBlock;

        public ArithOP(VariableBlock leftBlock, VariableBlock rightBlock, string output, ArithOPTypes opType, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { leftBlock?.OutputVariable, rightBlock?.OutputVariable }, output, id, canBeScheduled)
        {
            this.OPType = opType;
            this.LeftBlock = leftBlock;
            this.RightBlock = rightBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            ArithOPTypes opType = ArithOP.StringToArithOPType(id, ParseTools.ParseString(node, OPTypeFieldName));

            VariableBlock leftArithBlock  = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, LeftArithFieldName, 
                                            new MissingBlockException(id, "Left side of arithmetic operator is missing a block."));
            VariableBlock rightArithBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, RightArithFieldName, 
                                            new MissingBlockException(id, "Right side of Arithmetic operator is missing a block."));

            dfg.AddNode(leftArithBlock);
            dfg.AddNode(rightArithBlock);

            return new ArithOP(leftArithBlock, rightArithBlock, parserInfo.GetUniqueAnonymousName(), opType, id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock leftCopy = (VariableBlock)LeftBlock.TrueCopy(dfg);
            VariableBlock rightCopy = (VariableBlock)RightBlock.TrueCopy(dfg);

            dfg.AddNode(leftCopy);
            dfg.AddNode(rightCopy);

            return new ArithOP(leftCopy, rightCopy, OutputVariable, OPType, BlockID, CanBeScheduled);
        }

        public static ArithOPTypes StringToArithOPType(string id, string arithOPTypeAsString)
        {
            switch (arithOPTypeAsString)
            {
                case "ADD":
                    return ArithOPTypes.ADD;
                case "MINUS":
                    return ArithOPTypes.SUB;
                case "MULTIPLY":
                    return ArithOPTypes.MUL;
                case "DIVIDE":
                    return ArithOPTypes.DIV;
                case "POWER":
                    return ArithOPTypes.POW;
                default:
                    throw new InternalParseException(id, $"Unknown arithmetic operator.{Environment.NewLine}Expected  either ADD, MINUS, MULTIPLY, DIVIDE or POWER but operator was {arithOPTypeAsString}.");
            }
        }

        public static string ArithOpTypeToString(ArithOPTypes type)
        {
            switch (type)
            {
                case ArithOPTypes.ADD:
                    return "ADD";
                case ArithOPTypes.SUB:
                    return "MINUS";
                case ArithOPTypes.MUL:
                    return "MULTIPLY";
                case ArithOPTypes.DIV:
                    return "DIVIDE";
                case ArithOPTypes.POW:
                    return "POWER";
                default:
                    throw new InternalParseException("Failed to parse the arithmetic operator type. Type: " + type.ToString());
            }
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            float leftResult = LeftBlock.Run(variables, executor, dropPositions);
            float rightResult = RightBlock.Run(variables, executor, dropPositions);

            switch (OPType)
            {
                case ArithOPTypes.ADD:
                    return leftResult + rightResult;
                case ArithOPTypes.SUB:
                    return leftResult - rightResult;
                case ArithOPTypes.MUL:
                    return leftResult * rightResult;
                case ArithOPTypes.DIV:
                    return leftResult / rightResult;
                case ArithOPTypes.POW:
                    return (float)Math.Pow(leftResult, rightResult);
                default:
                    throw new InternalRuntimeException("Failed to parse the arithmetic operator type.");
            }
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{OPTypeFieldName}\">{ArithOpTypeToString(OPType)}</field>" +
                $"<value name=\"{LeftArithFieldName}\">" +
                    LeftBlock.ToXml() + 
                "</value>" +
                $"<value name=\"{RightArithFieldName}\">" +
                    RightBlock.ToXml() + 
                "</value>" +
            "</block>";
        }

        public override string ToString()
        {
            switch (OPType)
            {
                case ArithOPTypes.ADD:
                    return "+";
                case ArithOPTypes.SUB:
                    return "-";
                case ArithOPTypes.MUL:
                    return "*";
                case ArithOPTypes.DIV:
                    return "/";
                default:
                    throw new InternalParseException("Failed to parse the operator type.");
            }
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            LeftBlock.GetVariableTreeList(blocks);
            RightBlock.GetVariableTreeList(blocks);

            return blocks;
        }
    }
}
