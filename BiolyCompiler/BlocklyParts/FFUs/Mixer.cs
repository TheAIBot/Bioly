using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Mixer : FluidBlock
    {
        public const string FirstInputFieldName = "inputFluidA";
        public const string SecondInputFieldName = "inputFluidB";
        public const string XmlTypeName = "mixer";

        public Mixer(List<FluidInput> input, string output, XmlNode node) : base(true, input, output)
        {

        }

        public static Block CreateMixer(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(FirstInputFieldName).FirstChild, mostRecentRef));
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(SecondInputFieldName).FirstChild, mostRecentRef));

            return new Mixer(inputs, output, node);
        }

        public override string ToString()
        {
            return "Mixer";
        }

        public override Module getAssociatedModule()
        {
            return new MixerModule(45);
        }
    }
}
