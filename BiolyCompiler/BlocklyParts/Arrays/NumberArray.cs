using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class NumberArray : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string ARRAY_LENGTH_FIELD_NAME = "arrayLength";
        public const string XML_TYPE_NAME = "numberArray";
        public readonly string ArrayName;
        public readonly VariableBlock ArrayLengthBlock;

        public NumberArray(string arrayName, VariableBlock arrayLengthBlock, List<string> input, string id) : base(true, null, input, arrayName, id, true)
        {
            this.ArrayName = arrayName;
            this.ArrayLengthBlock = arrayLengthBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.AddVariable(id, VariableType.NUMBER_ARRAY, arrayName);

            VariableBlock arrayLengthBlock = null;
            XmlNode arrayLengthNode = node.GetInnerBlockNode(ARRAY_LENGTH_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block which define the length of the array."));
            if (arrayLengthNode != null)
            {
                arrayLengthBlock = (VariableBlock)XmlParser.ParseBlock(arrayLengthNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(arrayLengthBlock);

            List<string> inputs = new List<string>();
            inputs.Add(arrayLengthBlock?.OutputVariable);

            return new NumberArray(arrayName, arrayLengthBlock, inputs, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            throw new InternalRuntimeException("This method is not supported for this block.");
        }

        public override (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            string variableName = FluidArray.GetArrayLengthVariable(ArrayName);
            //The value returned from the block can be a fraction and
            //the array length can't be. So convert to int.
            int arrayLength = (int)ArrayLengthBlock.Run(variables, executor, dropPositions);
            if (arrayLength < 0)
            {
                throw new RuntimeException(BlockID, $"Array length can't be set to {arrayLength}. The length has to be positive.");
            }

            return (variableName, arrayLength);
        }

        public override string ToXml()
        {
            throw new InternalParseException(BlockID, "Can't create xml of this block.");
        }

        public override string ToString()
        {
            return "New number array with name " + ArrayName;
        }
    }
}
