using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Blocks
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;

        public Block(bool canBeOutput)
        {
            this.CanBeOutput = canBeOutput;
        }

        public abstract Block TryParseBlock(XmlNode node);
    }
}
