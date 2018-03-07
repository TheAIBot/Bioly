using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Splitter : Block
    {
        private const string FluidAmountName = "fluidAmount";
        private const string InputFluidName = "inputFluid";
        public const string XmlTypeName = "splitter";
        public readonly int FluidAmount;

        public Splitter(List<string> input, string output, XmlNode node) : base(true, input, output)
        {
            this.FluidAmount = node.GetNodeWithAttributeValue(FluidAmountName).TextToInt();
        }

        public static Block CreateSplitter(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<string> inputs = new List<string>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(InputFluidName), mostRecentRef));

            return new Splitter(inputs, output, node);
        }

        public override string ToString()
        {
            return "Splitter" + Environment.NewLine + 
                   "Amount: " + FluidAmount;
        }
    }
}
