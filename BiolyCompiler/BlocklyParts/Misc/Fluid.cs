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

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Fluid : FluidBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string OUTPUT_FLUID_FIELD_NAME = "fluidName";
        public const string XML_TYPE_NAME = "fluid";

        public Fluid(List<FluidInput> input, string output, string id) : base(true, input, output, id)
        {
        }

        private static Block CreateFluid(string output, XmlNode node, ParserInfo parserInfo)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(new FluidInput(node, parserInfo));

            string id = node.GetAttributeValue(Block.IDFieldName);
            return new Fluid(inputs, output, id);
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string output = node.GetNodeWithAttributeValue(OUTPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);
            XmlNode innerNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Fluid is missing fluid definition blocks."));
            if (innerNode != null)
            {
                switch (innerNode.GetAttributeValue(Block.TypeFieldName))
                {
                    case HeaterUseage.XML_TYPE_NAME:
                        return HeaterUseage.CreateHeater(output, innerNode, parserInfo);
                    case Mixer.XmlTypeName:
                        return Mixer.CreateMixer(output, innerNode, parserInfo);
                    case Union.XML_TYPE_NAME:
                        return Union.CreateUnion(output, innerNode, parserInfo);
                    default:
                        return CreateFluid(output, innerNode, parserInfo);
                }
            }
            return null;
        }

        public List<Command> GetFluidTransferOperations()
        {
            int time = 0;
            List<Command> routeCommands = new List<Command>();
            foreach (List<Route> routes in InputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routes.ForEach(route => routeCommands.AddRange(route.ToCommands(ref time)));
            }
            return routeCommands;
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
            return $"{InputVariables[0].OriginalFluidName} -> {OriginalOutputVariable}";
        }
    }
}
