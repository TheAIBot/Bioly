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
    public class GetArrayNumber : VariableBlock
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getNumberArrayIndex";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;

        public GetArrayNumber(VariableBlock indexBlock, string arrayName, string output, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { indexBlock?.OutputVariable, arrayName }, output, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.NUMBER_ARRAY, arrayName);

            VariableBlock indexBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, INDEX_FIELD_NAME,
                                       new MissingBlockException(id, "Missing block which define the index into the array."));

            dfg.AddNode(indexBlock);

            return new GetArrayNumber(indexBlock, arrayName, parserInfo.GetUniqueAnonymousName(), id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            VariableBlock indexCopy = (VariableBlock)IndexBlock.TrueCopy(dfg);

            dfg.AddNode(indexCopy);

            return new GetArrayNumber(indexCopy, ArrayName, OutputVariable, BlockID, CanBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
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

            return variables[FluidArray.GetArrayIndexName(ArrayName, index)];
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{ARRAY_NAME_FIELD_NAME}\">{ArrayName}</field>" +
                $"<value name=\"{INDEX_FIELD_NAME}\">" +
                    IndexBlock.ToXml() + 
                "</value>" +
            "</block>";
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            IndexBlock.GetVariableTreeList(blocks);

            return blocks;
        }

        public override string ToString()
        {
            return "get number from array " + ArrayName;
        }
    }
}
