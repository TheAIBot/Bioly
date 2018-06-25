﻿using BiolyCompiler.Graphs;
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

        public BasicInput(string id, string fluidName, string originalFluidName, float inputAmountInDroplets, bool useAllFluid) : 
            base(id, fluidName, originalFluidName, inputAmountInDroplets, useAllFluid, null)
        {

        }

        public static FluidInput Parse(XmlNode node, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string originalFluidName = node.GetNodeWithAttributeValue(FLUID_NAME_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, originalFluidName);
            if (doVariableCheck)
            {
                parserInfo.CheckVariable(id, VariableType.FLUID, originalFluidName);
            }
            parserInfo.MostRecentVariableRef.TryGetValue(originalFluidName, out string correctedName);

            string fluidName = correctedName ?? NO_FLUID_NAME;
            float amountInML = node.GetNodeWithAttributeValue(FLUID_AMOUNT_FIELD_NAME).TextToFloat(id);
            bool useAllFluid = FluidInput.StringToBool(node.GetNodeWithAttributeValue(USE_ALL_FLUID_FIELD_NAME).InnerText);

            return new BasicInput(id, fluidName, originalFluidName, amountInML, useAllFluid);
        }

        public override FluidInput CopyInput(DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            mostRecentRef.TryGetValue(OriginalFluidName, out string fluidName);
            return new BasicInput(ID, fluidName, OriginalFluidName, AmountInML, UseAllFluid);
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
