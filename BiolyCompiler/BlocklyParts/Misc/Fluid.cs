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
        public const string InputFluidFieldName = "inputFluid";
        public const string OutputFluidFieldName = "fluidName";
        public const string XML_TYPE_NAME = "fluid";

        public Fluid(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {
        }

        private static Block CreateFluid(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node, mostRecentRef));

            string id = node.GetAttributeValue(Block.IDFieldName);
            return new Fluid(inputs, output, node, id);
        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string output = node.GetNodeWithAttributeValue(OutputFluidFieldName).InnerText;
            XmlNode innerNode = node.GetInnerBlockNode(InputFluidFieldName, new MissingBlockException(id, "Fluid is missing fluid definition blocks."));
            switch (innerNode.GetAttributeValue(Block.TypeFieldName))
            {
                case Heater.XmlTypeName:
                    return Heater.CreateHeater(output, innerNode, mostRecentRef);
                case Mixer.XmlTypeName:
                    return Mixer.CreateMixer(output, innerNode, mostRecentRef);
                default:
                    return CreateFluid(output, innerNode, mostRecentRef);
            }
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
    }
}
