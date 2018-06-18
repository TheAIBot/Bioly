﻿using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class WasteDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string XML_TYPE_NAME = "wasteDeclaration";

        public WasteDeclaration(string moduleName, XmlNode node, string id) : base(moduleName, false, null, id)
        {

        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, moduleName);
            parserInfo.AddVariable(id, VariableType.WASTE, moduleName);

            return new WasteDeclaration(moduleName, node, id);
        }

        public override Module getAssociatedModule()
        {
            return new OutputModule();
        }

        public override string ToString()
        {
            return "Waste" + Environment.NewLine +
                   "Module name: " + ModuleName;
        }
    }
}