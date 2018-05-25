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
    public class GetFluidArray : VariableBlock
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getArrayLength";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;

        public GetFluidArray(VariableBlock indexBlock, string arrayName, string id) : base(false, null, id, false)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckFluidArrayVariable(id, arrayName);

            VariableBlock indexBlock = null;
            XmlNode indexNode = node.GetInnerBlockNode(INDEX_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block which define the index into the array."));
            if (indexNode != null)
            {
                indexBlock = (VariableBlock)XmlParser.ParseBlock(indexNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(indexBlock);

            return new GetFluidArray(indexBlock, arrayName, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            int index = (int)IndexBlock.Run(variables, executor);
            if (index < 0 || index >= arrayLength)
            {
                throw new ArrayIndexOutOfRange(IDFieldName, ArrayName, arrayLength, index);
            }

            return variables[FluidArray.GetArrayIndexName(ArrayName, index)];
        }
    }
}
