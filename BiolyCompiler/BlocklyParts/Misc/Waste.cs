using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
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
        public const string XML_TYPE_NAME = "waste";

        public Waste(List<FluidInput> input, string output, XmlNode node, string id) : base(false, input, output, id)
        {

        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            //use when it's converted to a static usage block
            //string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            //parserInfo.CheckModuleVariable(id, moduleName);

            FluidInput fluidInput = null;
            XmlNode inputFluidNode = node.GetInnerBlockNode(InputFluidFieldName, parserInfo, new MissingBlockException(id, "Waste is missing input fluid block."));
            if (inputFluidNode != null)
            {
                fluidInput = XmlParser.ParseFluidInput(inputFluidNode, dfg, parserInfo);
            }

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
