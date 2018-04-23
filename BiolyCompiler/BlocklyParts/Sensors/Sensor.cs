using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Sensors
{
    public class Sensor : FluidBlock
    {
        public const string XmlTypeName = "sensor";

        public Sensor(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {

        }
        public Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override Module getAssociatedModule()
        {
            return new SensorModule();
        }
    }
}
