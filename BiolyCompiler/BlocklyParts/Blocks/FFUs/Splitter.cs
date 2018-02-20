using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.FFUs
{
    internal class Splitter : Block
    {
        public const string XmlTypeName = "splitter";

        public Splitter() : base(true)
        {

        }

        public override Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
