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
using System.Linq;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class HeaterUsage : StaticUseageBlock
    {
        public const string TEMPERATURE_FIELD_NAME = "temperature";
        public const string TIME_FIELD_NAME = "time";
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "heaterUsage";
        public readonly int Temperature;
        public readonly int Time;


        public HeaterUsage(string moduleName, List<FluidInput> inputs, string output, int temperature, int time, string id) : 
            base(moduleName, inputs, null, true, output, id)
        {
            this.Temperature = temperature;
            //Can't be colder than absolute zero and the board probably can't handle more than 1000K
            Validator.ValueWithinRange(id, this.Temperature, -273, 1000);

            this.Time = time;
            //Time can't be negative and probably shouldn't be over a months time so throw an erro in those cases
            Validator.ValueWithinRange(id, this.Time, 0, 2592000);
        }

        public static Block CreateHeater(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string moduleName = ParseTools.ParseString(node, MODULE_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.HEATER, moduleName);

            int temperature = (int)ParseTools.ParseFloat(node, parserInfo, id, TEMPERATURE_FIELD_NAME);
            int time = (int)ParseTools.ParseFloat(node, parserInfo, id, TIME_FIELD_NAME);

            FluidInput fluidInput = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, INPUT_FLUID_FIELD_NAME,
                                    new MissingBlockException(id, "Heater is missing input fluid block."));

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new HeaterUsage(moduleName, inputs, output, temperature, time, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef, Dictionary<string, string> renamer, string namePostfix)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, mostRecentRef, renamer, namePostfix)));

            if (renamer.ContainsKey(OriginalOutputVariable))
            {
                renamer[OriginalOutputVariable] = OriginalOutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OriginalOutputVariable, OriginalOutputVariable + namePostfix);
            }
            return new HeaterUsage(ModuleName, inputFluids, OriginalOutputVariable + namePostfix, Temperature, Time, BlockID);
        }


        public override List<Command> ToCommands()
        {
            //This is neccessary to ensure that the the droplets spends the required time in the module:
            BoundModule.OperationTime = Time;
            return base.ToCommands();
        }

        private void SetTemperatureAndTime(string id, int temperature, int time)
        {


        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
