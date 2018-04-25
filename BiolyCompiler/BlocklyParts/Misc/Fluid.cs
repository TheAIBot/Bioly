using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Fluid : FluidBlock
    {
        public const string InputFluidFieldName = "inputFluid";
        public const string OutputFluidFieldName = "fluidName";
        public const string XML_TYPE_NAME = "fluid";

        public Fluid(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {
        }

        private static Block CreateFluid(string output, XmlNode node, ParserInfo parserInfo)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node, parserInfo));

            string id = node.GetAttributeValue(Block.IDFieldName);
            return new Fluid(inputs, output, node, id);
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string output = node.GetNodeWithAttributeValue(OutputFluidFieldName).InnerText;
            Validator.CheckVariableName(id, output);
            XmlNode innerNode = node.GetInnerBlockNode(InputFluidFieldName, new MissingBlockException(id, "Fluid is missing fluid definition blocks."));
            switch (innerNode.GetAttributeValue(Block.TypeFieldName))
            {
                case Heater.XmlTypeName:
                    return Heater.CreateHeater(output, innerNode, parserInfo);
                case Mixer.XmlTypeName:
                    return Mixer.CreateMixer(output, innerNode, parserInfo);
                default:
                    return CreateFluid(output, innerNode, parserInfo);
            }
        }
    }
}
