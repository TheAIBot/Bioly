using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class OutputUseage : StaticUseageBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "outputUseage";

        public OutputUseage(string moduleName, List<FluidInput> input, string output, XmlNode node, string id) : base(moduleName, input, false, output, id)
        {

        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).FirstChild, parserInfo));
            string id = node.GetAttributeValue(Block.IDFieldName);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckModuleVariable(id, moduleName);
            return new OutputUseage(moduleName, inputs, null, node, id);
        }

        public override void Bind(Module module, Dictionary<string, BoardFluid> FluidVariableLocations)
        {
            //The amount of droplets that the output module will take,
            //needs to be changed to the amount required by this block/operation.
            //This is neccessary for the routing to work:
            InfiniteModuleLayout layout = (InfiniteModuleLayout) module.GetInputLayout();
            layout.SetGivenAmountOfDroplets(InputVariables[0].GetAmountInDroplets(FluidVariableLocations), module);




            base.Bind(module, FluidVariableLocations);  
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Fluid: " + InputVariables[0].FluidName + Environment.NewLine +
                   "To target module: " + ModuleName;
        }
    }
}
