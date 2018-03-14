using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;

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

        public static Block CreateMixer(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<string> inputs = new List<string>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(FirstInputName), mostRecentRef));
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(SecondInputName), mostRecentRef));

            return new Mixer(inputs, output, node);
        }
        
        public override OperationType getOperationType() {
            return OperationType.Mixer;
        }

        public override string ToString()
        {
            return "Mixer";
        }
    }
}
