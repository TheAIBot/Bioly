using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class HeaterUsage : StaticUseageBlock
    {
        public const string TEMPERATURE_FIELD_NAME = "temperature";
        public const string TIME_FIELD_NAME = "time";
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "heaterUsage";
        public int Temperature { get; private set; }
        public int Time { get; private set; }


        public HeaterUsage(string moduleName, List<FluidInput> inputs, string output, int temperature, int time, string id) : base(moduleName, inputs, true, output, id)
        {
            SetTemperatureAndTime(id, temperature, time);
        }


        public HeaterUsage(string moduleName, List<FluidInput> inputs, string output, XmlNode node, string id) : base(moduleName, inputs, true, output, id)
        {
            int temperature = (int)node.GetNodeWithAttributeValue(TEMPERATURE_FIELD_NAME).TextToFloat(id);
            int time = (int)node.GetNodeWithAttributeValue(TIME_FIELD_NAME).TextToFloat(id);
            SetTemperatureAndTime(id, temperature, time);
        }

        public static Block CreateHeater(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.HEATER, moduleName);

            FluidInput fluidInput = null;
            XmlNode inputFluidNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Heater is missing input fluid block."));
            if (inputFluidNode != null)
            {
                fluidInput = XmlParser.ParseFluidInput(inputFluidNode, dfg, parserInfo);
            }

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new HeaterUsage(moduleName, inputs, output, node, id);
        }


        public override List<Command> ToCommands()
        {
            //This is neccessary to ensure that the the droplets spends the required time in the module:
            BoundModule.OperationTime = Time;
            return base.ToCommands();
        }

        private void SetTemperatureAndTime(string id, int temperature, int time)
        {
            this.Temperature = temperature;
            //Can't be colder than absolute zero and the board probably can't handle more than 1000C
            Validator.ValueWithinRange(id, this.Temperature, -273, 1000);

            this.Time = time;
            //Time can't be negative and probably shouldn't be over a months time so throw an erro in those cases
            Validator.ValueWithinRange(id, this.Time, 0, 2592000);

        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
