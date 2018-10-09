using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.Routing;
using BiolyCompiler.TypeSystem;

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

        public SetArrayFluid(VariableBlock indexBlock, string arrayName, List<FluidInput> input, string indexBlockName, string id) : 
            base(true, input, null, arrayName, id)
        {
            this.ArrayName = arrayName;
            this.IndexBlock = indexBlock;
        }

        public static Block Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.FLUID_ARRAY, arrayName);

            VariableBlock indexBlock = ParseTools.ParseBlock<VariableBlock>(node ,dfg, parserInfo, id, INDEX_FIELD_NAME,
                                       new MissingBlockException(id, "Missing block to define the index."));

            FluidInput fluidInput = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, INPUT_FLUID_FIELD_NAME,
                                    new MissingBlockException(id, "Missing input fluid block."));


            dfg.AddNode(indexBlock);

            List<FluidInput> inputFluids = new List<FluidInput>();
            inputFluids.Add(fluidInput);

            return new SetArrayFluid(indexBlock, arrayName, inputFluids, indexBlock?.OriginalOutputVariable, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, renamer, namePostfix)));
            if (renamer.ContainsKey(OriginalOutputVariable))
            {
                renamer[OriginalOutputVariable] = OriginalOutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OriginalOutputVariable, OriginalOutputVariable + namePostfix);
            }
            return new Fluid(inputFluids, OriginalOutputVariable + namePostfix, BlockID);
        }

        public override void Update<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            base.Update(variables, executor, dropPositions);

            int arrayLength = (int)variables[FluidArray.GetArrayLengthVariable(ArrayName)];
            float floatIndex = IndexBlock.Run(variables, executor, dropPositions);
            if (float.IsInfinity(floatIndex) || float.IsNaN(floatIndex))
            {
                throw new InvalidNumberException(BlockID, floatIndex);
            }

            int index = (int)floatIndex;
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
            return "put fluid into " + ArrayName;
        }
    }
}
