using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.Routing;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class SetArrayFluid : FluidBlock
    {
        public const string INDEX_FIELD_NAME = "index";
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "setFluidArrayIndex";
        public readonly string ArrayName;
        public readonly VariableBlock IndexBlock;

        public SetArrayFluid(VariableBlock indexBlock, string arrayName, List<FluidInput> input, string id) : base(true, input, arrayName, id)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckFluidArrayVariable(id, arrayName);

            VariableBlock indexBlock = null;
            XmlNode indexNode = node.GetInnerBlockNode(INDEX_FIELD_NAME, parserInfo, new MissingBlockException(id, "Missing block to define the variables value."));
            if (indexNode != null)
            {
                indexBlock = (VariableBlock)XmlParser.ParseBlock(indexNode, dfg, parserInfo, false, false);
            }

            FluidInput fluidInput = null;
            XmlNode inputFluidNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Mixer is missing input fluid block."));
            if (inputFluidNode != null)
            {
                fluidInput = XmlParser.ParseFluidInput(inputFluidNode, dfg, parserInfo);
            }


            dfg.AddNode(indexBlock);

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new SetArrayFluid(indexBlock, arrayName, inputs, id);
        }

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            base.Update(variables, executor, dropPositions);

            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            int index = (int)IndexBlock.Run(variables, executor, dropPositions);
            if (index < 0 || index >= arrayLength)
            {
                throw new ArrayIndexOutOfRange(BlockID, ArrayName, arrayLength, index);
            }

            OriginalOutputVariable = FluidArray.GetArrayIndexName(ArrayName, index);
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

        public override string ToString()
        {
            return "Set value in the array " + ArrayName;
        }
    }
}
