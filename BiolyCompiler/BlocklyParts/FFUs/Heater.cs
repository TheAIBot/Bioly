using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Heater : Block
    {
        public const string TemperatureFieldName = "temperature";
        public const string TimeFieldName = "time";
        public const string InputFluidFieldName = "inputFluid";
        public const string XmlTypeName = "heater";
        public readonly int Temperature;
        public readonly int Time;

        public Heater(List<string> input, string output, XmlNode node) : base(true, input, output)
        {
            this.Temperature = node.GetNodeWithAttributeValue(TemperatureFieldName).TextToInt();
            this.Time = node.GetNodeWithAttributeValue(TimeFieldName).TextToInt();
        }

        public static Block CreateHeater(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<string> inputs = new List<string>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(InputFluidFieldName), mostRecentRef));

            return new Heater(inputs, output, node);
        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
