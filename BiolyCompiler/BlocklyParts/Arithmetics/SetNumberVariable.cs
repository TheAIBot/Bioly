using System;
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

        public SetNumberVariable(VariableBlock operandBlock, List<string> input, string output, string id) : base(true, input, output, id, true)
        {
            this.OperandBlock = operandBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string output = node.GetNodeWithAttributeValue(VARIABLE_FIELD_NAME).InnerText;
            parserInfo.AddVariable(id, VariableType.NUMBER, output);

            VariableBlock operandBlock = null;
            XmlNode operandNode = node.GetInnerBlockNode(INPUT_VARIABLE_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block to define the variables value."));
            if (operandNode != null)
            {
                operandBlock = (VariableBlock)XmlParser.ParseBlock(operandNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(operandBlock);

            List<string> inputs = new List<string>();
            inputs.Add(operandBlock?.OutputVariable);

            return new SetNumberVariable(operandBlock, inputs, output, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return OperandBlock.Run(variables, executor, dropPositions);
        }

        public override string ToXml()
        {
            return SetNumberVariable.ToXml(BlockID, OriginalOutputVariable, OperandBlock.ToXml(), null);
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

        public override string ToString()
        {
            return "Set " + OriginalOutputVariable;
        }
    }
}
