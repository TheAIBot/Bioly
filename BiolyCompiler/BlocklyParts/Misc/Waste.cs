using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Waste : FluidBlock
    {
        public const string InputFluidFieldName = "inputFluid";
        public const string XmlTypeName = "waste";

        public Waste(List<FluidInput> input, string output, XmlNode node, string id) : base(false, input, output, id)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            XmlNode inputFluidNode = node.GetInnerBlockNode(InputFluidFieldName, new MissingBlockException(id, "Waste is missing input fluid block."));
            FluidInput fluidInput = XmlParser.GetVariablesCorrectedName(inputFluidNode, mostRecentRef);

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new Waste(inputs, null, node, id);
        }

        public override string ToString()
        {
            return "Waste" + Environment.NewLine +
                   "Fluid: " + InputVariables[0];
        }
    }
}
