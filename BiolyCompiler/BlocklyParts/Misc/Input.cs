using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Input : FluidBlock
    {
        public const string InputFluidFieldName = "inputName";
        public const string InputAmountFieldName = "inputAmount";
        public const string FluidUnitFieldName = "inputUnit";
        public const string XmlTypeName = "input";
        public readonly int Amount;
        public readonly FluidUnit Unit;

        public Input(string output, XmlNode node) : base(true, output)
        {
            this.Amount = node.GetNodeWithAttributeValue(InputAmountFieldName).TextToInt();
            this.Unit = StringToFluidUnit(node.GetNodeWithAttributeValue(FluidUnitFieldName).InnerText);
        }

        public Input(string output, int amount) : base(true, output)
        {
            this.Amount = amount;
            this.Unit = FluidUnit.drops;
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(InputFluidFieldName).InnerText;
            return new Input(output, node);
        }

        public static FluidUnit StringToFluidUnit(string value)
        {
            switch (value)
            {
                case "0":
                    return FluidUnit.drops;
                case "1":
                    return FluidUnit.ml;
                default:
                    throw new Exception("Unknown fluid unit");
            }
        }

        public static string FluidUnitToString(FluidUnit value)
        {
            switch (value)
            {
                case FluidUnit.drops:
                    return "0";
                case FluidUnit.ml:
                    return "1";
                default:
                    throw new Exception("Unknown fluid unit");
            }
        }

        public override Module getAssociatedModule()
        {
            if (boundModule == null)
            {
                boundModule = new InputModule(new BoardFluid(OutputVariable), Amount); ;
            }
            return boundModule;
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount + Unit.ToString().ToLower();
        }
    }
}
