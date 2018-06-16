using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Sensors
{
    public class Sensor : FluidBlock
    {
        public const string XmlTypeName = "sensor";
        public static string TypeName => "sensor";

        public Sensor(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, null, output, id)
        {

        }
        public Block TryParseBlock(XmlNode node)
        {
            throw new InternalParseException("Sensor block is not supported yet.");
        }

        public override Module getAssociatedModule()
        {
            return new SensorModule();
        }

        public override string ToString()
        {
            return "AKSLDJALSKJDASLKDJSALKDJASLKDJASLKDJASLKDJASDLKASJDLAKSJDASLKDJASLKDJASLKDJASDLKASJDLK";
        }
    }
}
