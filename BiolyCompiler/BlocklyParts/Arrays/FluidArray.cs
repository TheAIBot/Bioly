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
    public class FluidArray : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string ARRAY_LENGTH_FIELD_NAME = "arrayLength";
        public const string XML_TYPE_NAME = "fluidArray";
        public readonly string ArrayName;
        public readonly VariableBlock ArrayLengthBlock;

        public FluidArray(string arrayName, VariableBlock arrayLengthBlock, List<string> input, string id) : base(true, null, input, arrayName, id, true)
        {
            this.ArrayName = arrayName;
            this.ArrayLengthBlock = arrayLengthBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.FLUID_ARRAY, arrayName);

            VariableBlock arrayLengthBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, ARRAY_LENGTH_FIELD_NAME,
                                             new MissingBlockException(id, "Missing block which define the length of the array."));

            dfg.AddNode(arrayLengthBlock);

            List<string> inputs = new List<string>();
            inputs.Add(arrayLengthBlock?.OutputVariable);

            return new FluidArray(arrayName, arrayLengthBlock, inputs, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock arrayCopy = (VariableBlock)ArrayLengthBlock.TrueCopy(dfg);

            dfg.AddNode(arrayCopy);

            return new FluidArray(ArrayName, arrayCopy, InputNumbers.Copy(), BlockID);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            throw new InternalRuntimeException("This method is not supported for this block.");
        }

        public override (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            string variableName = GetArrayLengthVariable(ArrayName);
            //The value returned from the block can be a fraction and
            //the array length can't be. So convert to int.
            int arrayLength = (int)ArrayLengthBlock.Run(variables, executor, dropPositions);
            if (arrayLength < 0)
            {
                throw new RuntimeException(BlockID, $"Array length can't be set to {arrayLength}. The length has to be positive.");
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

        public override string ToXml()
        {
            throw new InternalParseException(BlockID, "Can't create xml of this block.");
        }

        public override string ToString()
        {
            return "New fluid array with name " + ArrayName;
        }
    }
}
