using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Waste : Block
    {
        public const string XmlTypeName = "waste";

        public Waste(List<string> input, string output, XmlNode node) : base(false, input, output)
        {

        }

        public static Block Parse(XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.InnerText);

            return new Waste(inputs, XmlParser.CreateName(), node);
        }

        public override string ToString()
        {
            return "Waste" + Environment.NewLine +
                   "Fluid: " + InputVariables[0];
        }
    }
}
