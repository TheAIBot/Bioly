using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class HeaterDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string XML_TYPE_NAME = "heaterDeclaration";
        public readonly int Temperature;
        public readonly int Time;

        public HeaterDeclaration(string moduleName, XmlNode node, string id) : base(moduleName, true, null, id)
        {
        }

        public override Module getAssociatedModule()
        {
            return new HeaterModule();
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, moduleName);
            parserInfo.AddVariable(id, VariableType.HEATER, moduleName);

            return new HeaterDeclaration(moduleName, node, id);
        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
