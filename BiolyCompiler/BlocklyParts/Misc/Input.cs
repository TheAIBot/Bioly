using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Input : Block
    {
        private const string InputFluidName = "inputName";
        private const string InputAmount = "inputAmount";
        private const string FluidUnitName = "inputUnit";
        public const string XmlTypeName = "input";
        public readonly int Amount;
        public readonly FluidUnit Unit;

        public Input(string output, XmlNode node) : base(true, output)
        {
            this.Amount = node.GetNodeWithAttributeValue(InputAmount).TextToInt();
            this.Unit = (FluidUnit)node.GetNodeWithAttributeValue(FluidUnitName).TextToInt();
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(InputFluidName).InnerText;
            return new Input(output, node);
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount + Unit.ToString().ToLower();
        }
    }
}
