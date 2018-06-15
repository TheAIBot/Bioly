using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class SetArrayNumber : VariableBlock
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string INPUT_NUMBER_FIELD_NAME = "inputNumber";
        public const string XML_TYPE_NAME = "setNumberArrayIndex";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;
        public readonly VariableBlock NumberBlock;

        public SetArrayNumber(VariableBlock indexBlock, VariableBlock numberBlock, string arrayName, List<string> input, string id, bool canBeScheduled) : base(true, input, arrayName, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
            this.NumberBlock = numberBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.NUMBER_ARRAY, arrayName);

            VariableBlock indexBlock = null;
            XmlNode indexNode = node.GetInnerBlockNode(INDEX_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block to define the variables value."));
            if (indexNode != null)
            {
                indexBlock = (VariableBlock)XmlParser.ParseBlock(indexNode, dfg, parserInfo, false, false);
            }

            VariableBlock numberInput = null;
            XmlNode inputNumberNode = node.GetInnerBlockNode(INPUT_NUMBER_FIELD_NAME, parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));
            if (inputNumberNode != null)
            {
                numberInput = (VariableBlock)XmlParser.ParseBlock(inputNumberNode, dfg, parserInfo, false, false);
            }


            dfg.AddNode(indexBlock);

            List<string> inputs = new List<string>();
            inputs.Add(indexBlock?.OutputVariable);
            inputs.Add(numberInput?.OutputVariable);

            return new SetArrayNumber(indexBlock, numberInput, arrayName, inputs, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            throw new InternalRuntimeException("This method is not supported for.");
        }

        public override (string variableName, float value) ExecuteBlock<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            float floatIndex = IndexBlock.Run(variables, executor, dropPositions);
            if (float.IsInfinity(floatIndex) || float.IsNaN(floatIndex))
            {
                throw new InvalidNumberException(BlockID, floatIndex);
            }

            int index = (int)floatIndex;
            if (index < 0 || index >= arrayLength)
            {
                throw new ArrayIndexOutOfRange(BlockID, ArrayName, arrayLength, index);
            }

            string arrayIndexName = FluidArray.GetArrayIndexName(ArrayName, index);
            return (arrayIndexName, NumberBlock.Run(variables, executor, dropPositions));
        }



        public override string ToXml()
        {
            throw new InternalParseException(BlockID, "Can't create xml of this block.");
        }

        public override string ToString()
        {
            return "put number into " + ArrayName;
        }
    }
}
