using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.Blocks.FFUs;

namespace BiolyCompiler.BlocklyParts.Blocks.Misc
{
    internal class Fluid : Block
    {
        private const string InputFluidName = "inputFluid";
        private const string OutputFluidName = "fluidName";
        public const string XmlTypeName = "fluid";

        public Fluid(List<string> input, string output, XmlNode node) : base(true, input, output)
        {
        }

        public static Block CreateFluid(string output, XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.GetNodeWithName(InputFluidName).InnerText);

            return new Fluid(inputs, output, node);
        }

        public static Block TryParseBlock(XmlNode node)
        {
            string output = node.GetNodeWithName(OutputFluidName).Value;
            XmlNode innerNode = node.GetNodeWithName(InputFluidName).FirstChild;
            switch (innerNode.Name)
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
