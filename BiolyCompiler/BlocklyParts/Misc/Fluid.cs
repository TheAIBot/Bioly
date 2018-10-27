using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Commands;
using BiolyCompiler.Routing;
using System.Linq;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Graphs;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Fluid : FluidBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string OUTPUT_FLUID_FIELD_NAME = "fluidName";
        public const string XML_TYPE_NAME = "fluid";

        public Fluid(List<FluidInput> input, string output, string id) : base(true, input, null, output, id)
        {
        }

        private static Block CreateFluid(string output, XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.ParseFluidInput(node, dfg, parserInfo));

            string id = ParseTools.ParseID(node);
            return new Fluid(inputs, output, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new Fluid(InputFluids.Copy(dfg), OutputVariable, BlockID);
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
            return new Fluid(inputFluids, OutputVariable + namePostfix, BlockID);
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string output = ParseTools.ParseString(node, OUTPUT_FLUID_FIELD_NAME);
            Validator.CheckVariableName(id, output);
            parserInfo.AddVariable(id, VariableType.FLUID, output);
            XmlNode innerNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Fluid is missing fluid definition blocks."));
            if (innerNode != null)
            {
                switch (innerNode.GetAttributeValue(Block.TYPE_FIELD_NAME))
                {
                    case HeaterUsage.XML_TYPE_NAME:
                        return HeaterUsage.CreateHeater(output, innerNode, dfg, parserInfo);
                    case Mixer.XmlTypeName:
                        return Mixer.CreateMixer(output, innerNode, dfg, parserInfo);
                    case Union.XML_TYPE_NAME:
                        return Union.CreateUnion(output, innerNode, dfg, parserInfo);
                    default:
                        return CreateFluid(output, innerNode, dfg, parserInfo);
                }
            }
            return null;
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

        public override List<Block> GetBlockTreeList(List<Block> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public static string ToXml(string id, string outputFluidName, string attachedBlock, string nextBlocks)
        {
            string xml =
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{id}\">" +
                $"<field name=\"{OUTPUT_FLUID_FIELD_NAME}\">{outputFluidName}</field>" +
                $"<value name=\"{INPUT_FLUID_FIELD_NAME}\">" +
                    attachedBlock + 
                "</value>";
            if (nextBlocks != null)
            {
                xml +=
                "<next>" +
                    nextBlocks + 
                "</next>";
            }
            xml += 
            "</block>";

            return xml;
        }

        public override string ToString()
        {
            return $"{InputFluids[0].OriginalFluidName} -> {OutputVariable}";
        }
    }
}
