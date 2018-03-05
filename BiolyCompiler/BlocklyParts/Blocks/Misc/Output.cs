using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.Misc
{
    internal class Output : Block
    {
        public const string XmlTypeName = "output";

        public Output(List<string> input, string output, XmlNode node) : base(false, input, output)
        {

        }

        internal static Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
