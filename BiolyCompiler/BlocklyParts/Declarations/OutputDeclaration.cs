﻿using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class OutputDeclaration : StaticDeclarationBlock
    {
        public const string XML_TYPE_NAME = "outputDeclaration";

        public OutputDeclaration(string moduleName, XmlNode node, string id) : base(moduleName, false, null, id)
        {

        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, moduleName);
            parserInfo.AddModuleName(moduleName);

            return new OutputDeclaration(moduleName, node, id);
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