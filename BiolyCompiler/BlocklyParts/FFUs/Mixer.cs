using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Mixer : FluidBlock
    {
        public const string FirstInputFieldName = "inputFluidA";
        public const string SecondInputFieldName = "inputFluidB";
        public const string XmlTypeName = "mixer";

        public Mixer(List<FluidAsInput> input, string output, XmlNode node) : base(true, input, output)
        {

        }

        public static Block CreateMixer(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidAsInput> inputs = new List<FluidAsInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(FirstInputFieldName), mostRecentRef));
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(SecondInputFieldName), mostRecentRef));

            return new Mixer(inputs, output, node);
        }
        
        public override OperationType getOperationType() {
            return OperationType.Mixer;
        }

        public override string ToString()
        {
            return "Mixer";
        }

        public override Module getAssociatedModule()
        {
            return new MixerModule(4,4,2000);
        }
    }
}
