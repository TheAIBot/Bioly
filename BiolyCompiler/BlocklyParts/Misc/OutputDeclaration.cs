using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class OutputDeclaration : StaticDeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputFluid";
        public const string XML_TYPE_NAME = "outputDeclaration";

        public OutputDeclaration(string moduleName, string output, XmlNode node) : base(moduleName, false, output)
        {

        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            return new OutputDeclaration(moduleName, null, node);
        }

        public override Module getAssociatedModule()
        {
            return new OutputModule();
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Module name: " + ModuleName;
        }
    }
}
