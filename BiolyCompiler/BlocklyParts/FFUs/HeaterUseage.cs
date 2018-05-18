using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class HeaterUseage : StaticUseageBlock
    {
        public const string TEMPERATURE_FIELD_NAME = "temperature";
        public const string TIME_FIELD_NAME = "time";
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string OUTPUT_FLUID_FIELD_NAME = "outputFluid";
        public const string XML_TYPE_NAME = "heaterUseage";
        public readonly int Temperature;
        public readonly int Time;

        public HeaterUseage(string moduleName, List<FluidInput> inputs, string output, XmlNode node, string id) : base(moduleName, inputs, true, output, id)
        {
            this.Temperature = (int)node.GetNodeWithAttributeValue(TEMPERATURE_FIELD_NAME).TextToFloat(id);
            //Can't be colder than absolute zero and the board probably can't handle more than 1000C
            Validator.ValueWithinRange(id, this.Temperature, -273, 1000);

            this.Time = (int)node.GetNodeWithAttributeValue(TIME_FIELD_NAME).TextToFloat(id);
            //Time can't be negative and probably shouldn't be over a months time so throw an erro in those cases
            Validator.ValueWithinRange(id, this.Time, 0, 2592000);
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string moduleName = node.GetAttributeValue(MODULE_NAME_FIELD_NAME);
            Validator.CheckVariableName(id, moduleName);
            parserInfo.AddModuleName(moduleName);

            XmlNode inputFluidNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, new MissingBlockException(id, "Heater is missing input fluid block."));
            FluidInput fluidInput = XmlParser.GetVariablesCorrectedName(inputFluidNode, parserInfo);
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            string output = node.GetNodeWithAttributeValue(OUTPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);

            return new HeaterUseage(moduleName, inputs, output, node, id);
        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
