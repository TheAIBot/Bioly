using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Heater : FluidBlock
    {
        public const string TemperatureFieldName = "temperature";
        public const string TimeFieldName = "time";
        public const string InputFluidFieldName = "inputFluid";
        public const string XmlTypeName = "heater";
        public readonly int Temperature;
        public readonly int Time;

        public Heater(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {
            this.Temperature = (int)node.GetNodeWithAttributeValue(TemperatureFieldName).TextToFloat(id);
            //Can't be colder than absolute zero and the board probably can't handle more than 1000C
            Validator.ValueWithinRange(id, this.Temperature, -273, 1000);

            this.Time = (int)node.GetNodeWithAttributeValue(TimeFieldName).TextToFloat(id);
            //Time can't be negative and probably shouldn't be over a months time so throw an erro in those cases
            Validator.ValueWithinRange(id, this.Time, 0, 2592000);
        }

        public static Block CreateHeater(string output, XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            XmlNode inputFluidNode = node.GetInnerBlockNode(InputFluidFieldName, new MissingBlockException(id, "Heater is missing input fluid block."));
            FluidInput fluidInput = XmlParser.GetVariablesCorrectedName(inputFluidNode, parserInfo);

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);
            return new Heater(inputs, output, node, id);
        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
