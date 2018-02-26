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
    }
}
