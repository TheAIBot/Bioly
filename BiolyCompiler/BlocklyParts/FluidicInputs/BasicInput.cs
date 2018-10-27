using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FluidicInputs
{
    public class BasicInput : FluidInput
    {
        public const string FLUID_NAME_FIELD_NAME = "fluidName";
        public const string FLUID_AMOUNT_FIELD_NAME = "fluidAmount";
        public const string USE_ALL_FLUID_FIELD_NAME = "useAllFluid";
        public const string XML_TYPE_NAME = "getFluid";

        public BasicInput(string id, string originalFluidName, float inputAmountInDroplets, bool useAllFluid) : 
            base(id, originalFluidName, inputAmountInDroplets, useAllFluid)
        {

        }

        public static FluidInput Parse(XmlNode node, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            string id = ParseTools.ParseID(node);
            string originalFluidName = ParseTools.ParseString(node, FLUID_NAME_FIELD_NAME);
            if (doVariableCheck)
            {
                parserInfo.CheckVariable(id, VariableType.FLUID, originalFluidName);
            }

            float amountInML = ParseTools.ParseFloat(node, parserInfo, id, FLUID_AMOUNT_FIELD_NAME);
            bool useAllFluid = FluidInput.StringToBool(ParseTools.ParseString(node, USE_ALL_FLUID_FIELD_NAME));

            return new BasicInput(id, originalFluidName, amountInML, useAllFluid);
        }

        public override FluidInput TrueCopy(DFG<Block> dfg)
        {
            return new BasicInput(ID, OriginalFluidName, AmountInML, UseAllFluid);
        }

        public override FluidInput CopyInput(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            renamer.TryGetValue(OriginalFluidName, out string correctedName);
            return new BasicInput(ID, correctedName, AmountInML, UseAllFluid);
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{ID}\">" +
                $"<field name=\"{FLUID_NAME_FIELD_NAME}\">{OriginalFluidName}</field>" +
                $"<field name=\"{FLUID_AMOUNT_FIELD_NAME}\">{AmountInML}</field>" +
                $"<field name=\"{USE_ALL_FLUID_FIELD_NAME}\">{FluidInput.BoolToString(UseAllFluid)}</field>" +
            "</block>";
        }
    }
}
