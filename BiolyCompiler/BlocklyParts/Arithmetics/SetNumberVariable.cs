﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class SetNumberVariable : VariableBlock
    {
        public const string VARIABLE_FIELD_NAME = "variableName";
        public const string INPUT_VARIABLE_FIELD_NAME = "inputVariable";
        public const string XML_TYPE_NAME = "setNumberVariable";
        private readonly VariableBlock OperandBlock;

        public SetNumberVariable(VariableBlock operandBlock, string output, string id) : 
            base(true, null, new List<string>() { operandBlock?.OutputVariable }, output, id, true)
        {
            this.OperandBlock = operandBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string output = ParseTools.ParseString(node, VARIABLE_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.NUMBER, output);

            VariableBlock operandBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, INPUT_VARIABLE_FIELD_NAME,
                                         new MissingBlockException(id, "Missing block to define the variables value."));

            dfg.AddNode(operandBlock);

            return new SetNumberVariable(operandBlock, output, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock operandCopy = (VariableBlock)OperandBlock.TrueCopy(dfg);

            dfg.AddNode(operandCopy);

            return new SetNumberVariable(operandCopy, OutputVariable, BlockID);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return OperandBlock.Run(variables, executor, dropPositions);
        }

        public override string ToXml()
        {
            return SetNumberVariable.ToXml(BlockID, OutputVariable, OperandBlock.ToXml(), null);
        }

        public static string ToXml(string id, string output, string attachedBlocks, string nextBlocks)
        {
            string xml =
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{id}\">" +
                $"<field name=\"{VARIABLE_FIELD_NAME}\">{output}</field>" +
                $"<value name=\"{INPUT_VARIABLE_FIELD_NAME}\">" +
                    attachedBlocks +
                "</value>";
            if (nextBlocks != null)
            {
                xml +=
                "<next>" +
                    nextBlocks +
                "</next>";
            }
            xml +=
            "</block>";

            return xml;
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            OperandBlock.GetVariableTreeList(blocks);
            return blocks;
        }

        public override string ToString()
        {
            return "Set " + OutputVariable;
        }
    }
}
