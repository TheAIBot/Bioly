using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Modules;
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

        public Output(List<FluidInput> input, string output, XmlNode node, string id) : base(false, input, output, id)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            XmlNode inputFluidNode = node.GetInnerBlockNode(InputFluidFieldName, new MissingBlockException(id, "Output is missing input fluid block."));
            FluidInput fluidInput = XmlParser.GetVariablesCorrectedName(inputFluidNode, mostRecentRef);

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new Output(inputs, null, node, id);
        }

        public override Module getAssociatedModule()
        {
            if (boundModule == null)
            {
                boundModule = new OutputModule(InputVariables[0].GetAmountInDroplets()); //The shouldn't be more than 1 input source
            }
            return boundModule;
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Fluid: " + InputVariables[0].FluidName;
        }
    }
}
