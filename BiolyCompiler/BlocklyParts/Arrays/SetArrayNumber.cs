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

        public SetArrayNumber(VariableBlock indexBlock, VariableBlock numberBlock, string arrayName, List<string> input, string id, bool canBeScheduled) : 
            base(true, null, input, arrayName, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
            this.NumberBlock = numberBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.NUMBER_ARRAY, arrayName);

            VariableBlock indexBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, INDEX_FIELD_NAME,
                                       new MissingBlockException(id, "Missing block to define the variables value."));

            VariableBlock numberInput = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, INPUT_NUMBER_FIELD_NAME,
                                        new MissingBlockException(id, "Mixer is missing input fluid block."));

            dfg.AddNode(indexBlock);
            dfg.AddNode(numberInput);

            List<string> inputs = new List<string>();
            inputs.Add(indexBlock?.OutputVariable);
            inputs.Add(numberInput?.OutputVariable);

            return new SetArrayNumber(indexBlock, numberInput, arrayName, inputs, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return NumberBlock.Run(variables, executor, dropPositions);
        }

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            base.Update(variables, executor, dropPositions);

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

            OriginalOutputVariable = FluidArray.GetArrayIndexName(ArrayName, index);
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
