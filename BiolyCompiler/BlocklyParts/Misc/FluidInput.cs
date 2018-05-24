using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class FluidInput
    {
        public const string FLUID_NAME_FIELD_NAME = "fluidName";
        public const string FLUID_AMOUNT_FIELD_NAME = "fluidAmount";
        public const string USE_ALL_FLUID_FIELD_NAME = "useAllFluid";
        public const string XML_TYPE_NAME = "getFluid";

        public readonly string ID;
        public  readonly string FluidName;
        public readonly string OriginalFluidName;
        private readonly float AmountInML;
        public  readonly bool UseAllFluid;

        public const int ML_PER_DROPLET = 1;

        public FluidInput(XmlNode node, ParserInfo parserInfo, bool doVariableCheck = true)
        {
            this.ID = node.GetAttributeValue(Block.IDFieldName);
            OriginalFluidName = node.GetNodeWithAttributeValue(FLUID_NAME_FIELD_NAME).InnerText;
            Validator.CheckVariableName(ID, OriginalFluidName);
            if (doVariableCheck)
            {
                parserInfo.CheckFluidVariable(ID, OriginalFluidName);
            }
            parserInfo.mostRecentVariableRef.TryGetValue(OriginalFluidName, out string correctedName);

            this.FluidName = correctedName ?? "ERROR_FINDING_NODE";
            this.AmountInML = node.GetNodeWithAttributeValue(FLUID_AMOUNT_FIELD_NAME).TextToFloat(ID);
            this.UseAllFluid = FluidInput.StringToBool(node.GetNodeWithAttributeValue(USE_ALL_FLUID_FIELD_NAME).InnerText);
        }

        public FluidInput(string fluidName, int inputAmountInML, bool useAllFluid)
        {
            this.FluidName = fluidName;
            this.OriginalFluidName = fluidName;
            this.AmountInML = inputAmountInML;
            this.UseAllFluid = useAllFluid;
        }

        public static bool StringToBool(string boolean)
        {
            switch (boolean)
            {
                case "TRUE":
                    return true;
                case "FALSE":
                    return false;
                default:
                    throw new Exception("Failed to parse the boolean type.");
            }
        }

        public static string BoolToString(bool value)
        {
            switch (value)
            {
                case true:
                    return "TRUE";
                case false:
                    return "FALSE";
                default:
                    throw new Exception("Failed to parse the boolean type.");
            }
        }

        public int GetAmountInDroplets(Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            if (UseAllFluid)
            {
                return FluidVariableLocations.ContainsKey(OriginalFluidName) ? FluidVariableLocations[OriginalFluidName].GetNumberOfDropletsAvailable() : 0;

            }
            //tempoary until ratio is added
            return (int)Math.Floor((AmountInML / ML_PER_DROPLET) + 0.01);
        }

        public string ToXml()
        {
            return FluidInput.ToXml(ID, OriginalFluidName, AmountInML, UseAllFluid);
        }

        public static string ToXml(string id, string inputFluidName, float amount, bool useAllFluid)
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{id}\">" +
                $"<field name=\"{FLUID_NAME_FIELD_NAME}\">{inputFluidName}</field>" +
                $"<field name=\"{FLUID_AMOUNT_FIELD_NAME}\">{amount}</field>" +
                $"<field name=\"{USE_ALL_FLUID_FIELD_NAME}\">{FluidInput.BoolToString(useAllFluid)}</field>" +
            "</block>";
        }

        public override string ToString()
        {
            if (UseAllFluid)
            {
                return "all of it";
            }
            else
            {
                return AmountInML.ToString("N2") + " ml";
            }
        }
    }
}
