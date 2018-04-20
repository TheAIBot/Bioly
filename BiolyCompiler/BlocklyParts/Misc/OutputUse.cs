﻿using BiolyCompiler.Modules;
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

        public OutputUseage(string moduleName, List<FluidInput> input, string output, XmlNode node) : base(moduleName, input, false, output)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).FirstChild, mostRecentRef));
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            return new OutputUseage(moduleName, inputs, null, node);
        }
        

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Fluid: " + InputVariables[0].FluidName + Environment.NewLine +
                   "To target module: " + ModuleName;
        }
    }
}