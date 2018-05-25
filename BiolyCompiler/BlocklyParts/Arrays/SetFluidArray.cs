﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class SetFluidArray : FluidBlock
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "setFluidArray";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;

        public SetFluidArray(VariableBlock indexBlock, string arrayName, List<FluidInput> input, string id) : base(true, input, arrayName, id)
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
            XmlNode indexNode = node.GetInnerBlockNode(INDEX_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block to define the variables value."));
            if (indexNode != null)
            {
                indexBlock = (VariableBlock)XmlParser.ParseBlock(indexNode, dfg, parserInfo, false, false);
            }

            FluidInput fluidInput = null;
            XmlNode inputFluidNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));
            if (inputFluidNode != null)
            {
                fluidInput = new FluidInput(inputFluidNode, parserInfo);
            }


            dfg.AddNode(indexBlock);

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new SetFluidArray(indexBlock, arrayName, inputs, id);
        }

        public override void UpdateOriginalOutputVariable<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            int index = (int)IndexBlock.Run(variables, executor);
            if (index < 0 || index >= arrayLength)
            {
                throw new ArrayIndexOutOfRange(IDFieldName, ArrayName, arrayLength, index);
            }

            OriginalOutputVariable = FluidArray.GetArrayIndexName(ArrayName, index);
        }
    }
}
