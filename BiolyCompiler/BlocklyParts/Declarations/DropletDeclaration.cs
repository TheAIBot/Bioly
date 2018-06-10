﻿using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class DropletDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "dropletName";
        public const string XML_TYPE_NAME = "dropletDeclaration";

        public DropletDeclaration(string output, XmlNode node, string id) : base("moduleName-" + id, true, output, id)
        {
        }

        public DropletDeclaration(string moduleName, string output, string id) : base(moduleName, true, output, id)
        {
        }

        public static DropletDeclaration Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string output = node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);
            parserInfo.AddVariable(id, VariableType.FLUID, output);

            return new DropletDeclaration(output, node, id);
        }
        
        public override Module getAssociatedModule()
        {
            return new Droplet(new BoardFluid(OriginalOutputVariable));
        }

        public override string ToString()
        {
            return "Droplet of type:" + OriginalOutputVariable + Environment.NewLine;
        }
    }
}