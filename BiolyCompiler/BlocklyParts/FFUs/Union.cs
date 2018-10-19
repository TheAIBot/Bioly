using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Commands;
using BiolyCompiler.Routing;
using System.Linq;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Union : FluidBlock
    {
        public const string FIRST_INPUT_FIELD_NAME = "inputFluidA";
        public const string SECOND_INPUT_FIELD_NAME = "inputFluidB";
        public const string XML_TYPE_NAME = "union";

        public Union(List<FluidInput> input, string output, string id) : base(true, input, null, output, id)
        {

        }

        public static Block CreateUnion(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);

            FluidInput fluidInput1 = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, FIRST_INPUT_FIELD_NAME,
                                     new MissingBlockException(id, "Union is missing input fluid block."));
            FluidInput fluidInput2 = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, SECOND_INPUT_FIELD_NAME,
                                     new MissingBlockException(id, "Union is missing input fluid block."));

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput1);
            inputs.Add(fluidInput2);

            return new Union(inputs, output, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new Union(InputFluids.Copy(dfg), OutputVariable, BlockID);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            if (!renamer.ContainsKey(OutputVariable))
            {
                renamer.Add(OutputVariable, OutputVariable + namePostfix);
            }

            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, renamer, namePostfix)));

            renamer[OutputVariable] = OutputVariable + namePostfix;
            return new Union(inputFluids, OutputVariable + namePostfix, BlockID);
        }


        public override List<Command> ToCommands()
        {
            int time = 0;
            List<Command> routeCommands = new List<Command>();

            //add commands for waste routes. They must be before the other routes
            foreach (List<Route> wasteRouteList in WasteRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                wasteRouteList.ForEach(route => routeCommands.AddRange(route.ToCommands(ref time)));
            }

            foreach (List<Route> routes in InputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routes.ForEach(route => routeCommands.AddRange(route.ToCommands(ref time)));
            }
            return routeCommands;
        }


        public string ToXml()
        {
            return ToXml(this.BlockID, InputFluids[0].ToXml(), InputFluids[0].ToXml());
        }

        public static string ToXml(string id, string inputAXml, string inputBXml)
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{id}\">" +
                $"<value name=\"{FIRST_INPUT_FIELD_NAME}\">" +
                    inputAXml +
                "</value>" +
                $"<value name=\"{SECOND_INPUT_FIELD_NAME}\">" +
                    inputBXml +
                "</value>" + 
            "</block>";
        }

        public override string ToString()
        {
            return "Union: " + OutputVariable;
        }
    }
}
