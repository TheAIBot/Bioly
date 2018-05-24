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

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Union : FluidBlock
    {
        public const string FIRST_INPUT_FIELD_NAME = "inputFluidA";
        public const string SECOND_INPUT_FIELD_NAME = "inputFluidB";
        public const string XML_TYPE_NAME = "union";

        public Union(List<FluidInput> input, string output, XmlNode node, string id) : base(true, input, output, id)
        {

        }

        public static Block CreateUnion(string output, XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);

            XmlNode inputFluidNode1 = node.GetInnerBlockNode(FIRST_INPUT_FIELD_NAME , parserInfo, new MissingBlockException(id, "Union is missing input fluid block."));
            XmlNode inputFluidNode2 = node.GetInnerBlockNode(SECOND_INPUT_FIELD_NAME, parserInfo, new MissingBlockException(id, "Union is missing input fluid block."));

            FluidInput fluidInput1 = null;
            FluidInput fluidInput2 = null;

            if (inputFluidNode1 != null) {
                fluidInput1 = new FluidInput(inputFluidNode1, parserInfo, false);
            }
            if (inputFluidNode2 != null) {
                fluidInput2 = new FluidInput(inputFluidNode2, parserInfo, false);
            }

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput1);
            inputs.Add(fluidInput2);

            return new Union(inputs, output, node, id);
        }


        public override List<Command> ToCommands()
        {
            int time = 0;
            List<Command> routeCommands = new List<Command>();
            foreach (List<Route> routes in InputRoutes.Values.OrderBy(routes => routes.First().startTime))
            {
                routes.ForEach(route => routeCommands.AddRange(route.ToCommands(ref time)));
            }
            return routeCommands;
        }


        public string ToXml()
        {
            return ToXml(this.BlockID, InputVariables[0].ToXml(), InputVariables[0].ToXml());
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
            return "Union";
        }
    }
}
