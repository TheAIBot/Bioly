using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class HeaterDeclaration : StaticDeclarationBlock
    {
        public const string XML_TYPE_NAME = "heaterDeclaration";
        public readonly int Temperature;
        public readonly int Time;

        public HeaterDeclaration(string moduleName, string output, XmlNode node, string id) : base(moduleName, true, output, id)
        {
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string moduleName = node.GetAttributeValue(MODULE_NAME_FIELD_NAME);
            Validator.CheckVariableName(id, moduleName);
            parserInfo.AddModuleName(moduleName);
            
            return new HeaterDeclaration(moduleName, null, node, id);
        }

        public override string ToString()
        {
            return "Heater" + Environment.NewLine +
                   "Temp: " + Temperature + Environment.NewLine +
                   "Time: " + Time;
        }
    }
}
