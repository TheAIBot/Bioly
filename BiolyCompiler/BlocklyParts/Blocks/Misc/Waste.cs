using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.Misc
{
    internal class Waste : Block
    {
        public const string XmlTypeName = "waste";

        public Waste() : base(true)
        {

        }

        public override Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
