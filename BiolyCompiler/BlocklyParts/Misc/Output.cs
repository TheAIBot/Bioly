using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Output : Block
    {
        public const string XmlTypeName = "output";

        public Output(List<string> input, string output, XmlNode node) : base(false, input, output)
        {

        }

        public static Block Parse(XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.InnerText);

            return new Output(inputs, XmlParser.CreateName(), node);
        }
    }
}
