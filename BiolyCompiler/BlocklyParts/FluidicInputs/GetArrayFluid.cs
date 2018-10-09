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

        public GetArrayFluid(VariableBlock indexBlock, string arrayName, string id, float inputAmountInDroplets, bool useAllFluid, List<string> inputNumbers) : 
            base(id, arrayName, inputAmountInDroplets, useAllFluid, inputNumbers)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static FluidInput Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            if (doVariableCheck)
            {
                parserInfo.CheckVariable(id, VariableType.FLUID_ARRAY, arrayName);
            }

            float amountInML = ParseTools.ParseFloat(node, parserInfo, id, FLUID_AMOUNT_FIELD_NAME);
            bool useAllFluid = FluidInput.StringToBool(ParseTools.ParseString(node, USE_ALL_FLUID_FIELD_NAME));

            VariableBlock indexBlock = ParseTools.ParseBlock<VariableBlock>(node, dfg, parserInfo, id, INDEX_FIELD_NAME,
                                       new MissingBlockException(id, "Missing block which define the index into the array."));

            dfg.AddNode(indexBlock);

            List<string> inputNumbers = new List<string>();
            //inputNumbers.Add(indexBlock?.OutputVariable);

            return new GetArrayFluid(indexBlock, arrayName, id, amountInML, useAllFluid, inputNumbers);
        }

        public override FluidInput CopyInput(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            renamer.TryGetValue(OriginalFluidName, out string correctedName);
            return new BasicInput(ID, correctedName, AmountInML, UseAllFluid);
        }

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            if (!variables.ContainsKey(FluidArray.GetArrayLengthVariable(ArrayName)))
            {
                throw new RuntimeException(ID, "Can't get a fluid before inserting one into the array. Array name: " + ArrayName);
            }
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
