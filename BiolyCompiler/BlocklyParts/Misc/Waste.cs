using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Waste : FluidBlock
    {
        public const string XmlTypeName = "waste";

        public Waste(List<FluidAsInput> input, string output, XmlNode node) : base(false, input, output)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidAsInput> inputs = new List<FluidAsInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node, mostRecentRef));

            return new Waste(inputs, null, node);
        }

        public override string ToString()
        {
            return "Waste" + Environment.NewLine +
                   "Fluid: " + InputVariables[0];
        }
    }
}
