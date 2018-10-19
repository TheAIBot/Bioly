using BiolyCompiler.BlocklyParts.FluidicInputs;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class OutputUsage : StaticUseageBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "outputUsage";

        public OutputUsage(string moduleName, List<FluidInput> input, string output, string id) : base(moduleName, input, null, true, output, id)
        {

        }

        public static OutputUsage Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string moduleName = ParseTools.ParseString(node, MODULE_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.OUTPUT, moduleName);

            FluidInput fluidInput = ParseTools.ParseFluidInput(node, dfg, parserInfo, id, INPUT_FLUID_FIELD_NAME,
                                    new MissingBlockException(id, "Output is missing input fluid block."));

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new OutputUsage(moduleName, inputs, null, id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new OutputUsage(ModuleName, InputFluids.Copy(dfg), OutputVariable, BlockID);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, renamer, namePostfix)));

            return new OutputUsage(ModuleName, inputFluids, null, BlockID);
        }

        public override void Bind(Module module, Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            //The amount of droplets that the output module will take,
            //needs to be changed to the amount required by this block/operation.
            //This is neccessary for the routing to work:
            InfiniteModuleLayout layout = (InfiniteModuleLayout) module.GetInputLayout();
            layout.SetGivenAmountOfDroplets(InputFluids[0].GetAmountInDroplets(FluidVariableLocations), module);




            base.Bind(module, FluidVariableLocations);  
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Fluid: " + InputFluids[0].OriginalFluidName + Environment.NewLine +
                   "To target module: " + ModuleName;
        }
    }
}
