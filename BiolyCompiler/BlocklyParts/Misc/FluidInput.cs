using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class FluidInput
    {
        public const string FluidNameFieldName = "fluidName";
        public const string FluidAmountFieldName = "fluidAmount";
        public const string UseAllFluidFieldName = "useAllFluid";
        public const string XmlTypeName = "getFluid";

        public  readonly string FluidName;
        private readonly float AmountInML;
        public  readonly bool UseAllFluid;

        public const int ML_PER_DROPLET = 1;

        public FluidInput(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string originalName = node.GetNodeWithAttributeValue(FluidNameFieldName).InnerText;
            Validator.CheckVariableName(id, originalName);
            parserInfo.CheckFluidVariable(id, originalName);
            parserInfo.mostRecentVariableRef.TryGetValue(originalName, out string correctedName);

            this.FluidName = correctedName ?? "ERROR_FINDING_NODE";
            this.AmountInML = node.GetNodeWithAttributeValue(FluidAmountFieldName).TextToFloat(id);
            this.UseAllFluid = FluidInput.StringToBool(node.GetNodeWithAttributeValue(UseAllFluidFieldName).InnerText);
        }

        public FluidInput(string fluidName, int inputAmountInDroplets, bool useAllFluid)
        {

            this.FluidName = fluidName;
            this.AmountInML = GetMLOfFluidFromNumberOfDroplets(inputAmountInDroplets);
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

        public int GetAmountInDroplets()
        {
            //tempoary until ratio is added
            return (int)Math.Floor((AmountInML / ML_PER_DROPLET) + 0.01);
        }

        public int GetMLOfFluidFromNumberOfDroplets(int numberOfDroplets)
        {
            //tempoary until ratio is added
            return numberOfDroplets * ML_PER_DROPLET;
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
