using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Mixer : Block
    {
        private const string FirstInputName = "inputFluidA";
        private const string SecondInputName = "inputFluidB";
        public const string XmlTypeName = "mixer";

        public Mixer(List<string> input, string output, XmlNode node) : base(true, input, output)
        {

        }

        public static Block CreateMixer(string output, XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.GetNodeWithAttributeValue(FirstInputName).InnerText);
            inputs.Add(node.GetNodeWithAttributeValue(SecondInputName).InnerText);

            return new Mixer(inputs, output, node);
        }
    }
}
