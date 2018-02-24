using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks.Misc
{
    internal class Input : Block
    {
        public const string XmlTypeName = "input";

        public Input() : base(true)
        {

        }

        public override Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }
        
    }
}
