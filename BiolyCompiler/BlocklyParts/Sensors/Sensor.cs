using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Sensors
{
    public class Sensor : Block
    {
        public const string XmlTypeName = "sensor";

        public Sensor(List<string> input, string output, XmlNode node) : base(true, input, output)
        {

        }
    }
}
