using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.Arrays;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.FluidicInputs
{
    public class GetArrayFluid : FluidInput
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string FLUID_AMOUNT_FIELD_NAME = "fluidAmount";
        public const string USE_ALL_FLUID_FIELD_NAME = "useAllFluid";
        public const string XML_TYPE_NAME = "getFluidArrayIndex";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;

        public GetArrayFluid(VariableBlock indexBlock, string arrayName, string id, string fluidName, int inputAmountInDroplets, bool useAllFluid) : base(id, fluidName, null, inputAmountInDroplets, useAllFluid)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static FluidInput Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            if (doVariableCheck)
            {
                parserInfo.CheckVariable(id, VariableType.FLUID_ARRAY, arrayName);
            }
            parserInfo.MostRecentVariableRef.TryGetValue(arrayName, out string correctedName);

            string fluidName = correctedName ?? NO_FLUID_NAME;
            int amountInML = (int)node.GetNodeWithAttributeValue(FLUID_AMOUNT_FIELD_NAME).TextToFloat(id);
            bool useAllFluid = FluidInput.StringToBool(node.GetNodeWithAttributeValue(USE_ALL_FLUID_FIELD_NAME).InnerText);

            VariableBlock indexBlock = null;
            XmlNode indexNode = node.GetInnerBlockNode(INDEX_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block which define the index into the array."));
            if (indexNode != null)
            {
                indexBlock = (VariableBlock)XmlParser.ParseBlock(indexNode, dfg, parserInfo, false, false);
            }

            dfg.AddNode(indexBlock);

            return new GetArrayFluid(indexBlock, arrayName, id, fluidName, amountInML, useAllFluid);
        }

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            float floatIndex = IndexBlock.Run(variables, executor, dropPositions);
            if (float.IsInfinity(floatIndex) || float.IsNaN(floatIndex))
            {
                throw new InvalidNumberException(ID, floatIndex);
            }

            int index = (int)floatIndex;
            if (index < 0 || index >= arrayLength)
            {
                throw new ArrayIndexOutOfRange(ID, ArrayName, arrayLength, index);
            }


            OriginalFluidName = FluidArray.GetArrayIndexName(ArrayName, index);
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{ID}\">" +
                $"<field name=\"{ARRAY_NAME_FIELD_NAME}\">{ArrayName}</field>" +
                $"<field name=\"{FLUID_AMOUNT_FIELD_NAME}\">{AmountInML}</field>" +
                $"<field name=\"{USE_ALL_FLUID_FIELD_NAME}\">{FluidInput.BoolToString(UseAllFluid)}</field>" +
                $"<value name=\"{INDEX_FIELD_NAME}\">" +
                    IndexBlock.ToXml() + 
                "</value>" +
            "</block>";
        }
    }
}
