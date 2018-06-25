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
    public class WasteUsage : StaticUseageBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "wasteUsage";

        public WasteUsage(string moduleName, List<FluidInput> input, string output, string id) : base(moduleName, input, null, true, output, id)
        {

        }

        public static WasteUsage Parse(XmlNode node, DFG<Block> dfg, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.WASTE, moduleName);

            FluidInput fluidInput = null;
            XmlNode inputFluidNode = node.GetInnerBlockNode(INPUT_FLUID_FIELD_NAME, parserInfo, new MissingBlockException(id, "Waste is missing input fluid block."));
            if (inputFluidNode != null)
            {
                fluidInput = XmlParser.ParseFluidInput(inputFluidNode, dfg, parserInfo);
            }

            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(fluidInput);

            return new WasteUsage(moduleName, inputs, null, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputFluids = new List<FluidInput>();
            InputFluids.ToList().ForEach(x => inputFluids.Add(x.CopyInput(dfg, mostRecentRef)));

            return new WasteUsage(ModuleName, inputFluids, OriginalOutputVariable, BlockID);
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
            return "Waste" + Environment.NewLine +
                   "Fluid: " + InputFluids[0].OriginalFluidName + Environment.NewLine +
                   "To target module: " + ModuleName;
        }
    }
}
