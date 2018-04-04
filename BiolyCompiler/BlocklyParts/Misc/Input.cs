using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;

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
            switch (node.GetNodeWithAttributeValue(FluidUnitFieldName).InnerText)
            {
                case "0":
                    this.Unit = FluidUnit.drops;
                    break;
                case "1":
                    this.Unit = FluidUnit.ml;
                    break;
                default:
                    throw new Exception("Unknown fluid unit");
            }
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(InputFluidFieldName).InnerText;
            return new Input(output, node);
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount + Unit.ToString().ToLower();
        }
    }
}
