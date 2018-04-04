using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Output : FluidBlock
    {
        public const string InputFluidFieldName = "inputFluid";
        public const string XmlTypeName = "output";

        public Output(List<FluidAsInput> input, string output, XmlNode node) : base(false, input, output)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidAsInput> inputs = new List<FluidAsInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(InputFluidFieldName).FirstChild, mostRecentRef));

            return new Output(inputs, null, node);
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Fluid: " + InputVariables[0].FluidName;
        }
    }
}
