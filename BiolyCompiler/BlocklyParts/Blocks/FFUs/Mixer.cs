using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.FFUs
{
    internal class Mixer : Block
    {
        public const string XmlTypeName = "mixer";

        public Mixer() : base(true)
        {

        }

        public override Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
