using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.Misc
{
    internal class Input : Block
    {
        private const string InputFluidName = "inputName";
        private const string InputAmount = "inputAmount";
        private const string FluidUnitName = "inputUnit";
        public const string XmlTypeName = "input";
        public readonly int Amount;
        public readonly FluidUnit Unit;

        public Input(string output, XmlNode node) : base(true, output)
        {

        }

        public static Block TryParseBlock(XmlNode node)
        {

        }
    }
}
