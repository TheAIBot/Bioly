using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FFUs;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Fluid : Block
    {
        private const string InputFluidName = "inputFluid";
        private const string OutputFluidName = "fluidName";
        public const string XmlTypeName = "fluid";

        public Fluid(List<string> input, string output, XmlNode node) : base(true, input, output)
        {
        }

        private static Block CreateFluid(string output, XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.InnerText);

            return new Fluid(inputs, output, node);
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(OutputFluidName).InnerText;
            XmlNode innerNode = node.GetNodeWithAttributeValue(InputFluidName).FirstChild;
            switch (innerNode.Attributes["type"].Value)
            {
                case Heater.XmlTypeName:
                    return Heater.CreateHeater(output, innerNode);
                case Mixer.XmlTypeName:
                    return Mixer.CreateMixer(output, innerNode);
                case Splitter.XmlTypeName:
                    return Splitter.CreateSplitter(output, innerNode);
                default:
                    return CreateFluid(output, innerNode);
            }
        }
    }
}
