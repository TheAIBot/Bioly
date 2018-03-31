using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Input : Block
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
            this.Unit = (FluidUnit)node.GetNodeWithAttributeValue(FluidUnitFieldName).TextToInt();
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
