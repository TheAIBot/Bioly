using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class FluidArray : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string ARRAY_LENGTH_FIELD_NAME = "arrayLength";
        public const string XML_TYPE_NAME = "fluidArray";
        public readonly string ArrayName;
        public readonly VariableBlock ArrayLengthBlock;

        public FluidArray(string arrayName, VariableBlock arrayLengthBlock, List<string> input, string id) : base(true, input, null, id, true)
        {
            this.ArrayName = arrayName;
            this.ArrayLengthBlock = arrayLengthBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.AddFluidArrayVariable(arrayName);

            VariableBlock arrayLengthBlock = null;
            XmlNode arrayLengthNode = node.GetInnerBlockNode(ARRAY_LENGTH_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block which define the length of the array."));
            if (arrayLengthNode != null)
            {
                arrayLengthBlock = (VariableBlock)XmlParser.ParseBlock(arrayLengthNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(arrayLengthBlock);

            List<string> inputs = new List<string>();
            inputs.Add(arrayLengthBlock?.OutputVariable);

            return new FluidArray(arrayName, arrayLengthBlock, inputs, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            throw new NotImplementedException();
        }

        public override (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            string variableName = GetArrayLengthVariable(ArrayName);
            //The value returned from the block can be a fraction and
            //the array length can't be. So convert to int.
            int arrayLength = (int)ArrayLengthBlock.Run(variables, executor);
            if (arrayLength < 0)
            {
                throw new RuntimeException(IDFieldName, $"Array length can't be set to {arrayLength}. The length has to be positive.");
            }
            
            return (variableName, arrayLength);
        }

        public static string GetArrayLengthVariable(string arrayName)
        {
            return $"{arrayName}{Validator.FLUID_ARRAY_SPECIAL_SEPARATOR}Length";
        }

        public static string GetArrayIndexName(string arrayName, int index)
        {
            return $"{arrayName}{Validator.FLUID_ARRAY_SPECIAL_SEPARATOR}Index{index}";
        }
    }
}
