﻿using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class OutputDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string XML_TYPE_NAME = "outputDeclaration";

        public OutputDeclaration(string moduleName, string output, string id) : base(moduleName, false, output, id)
        {

        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string moduleName = ParseTools.ParseString(node, MODULE_NAME_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.OUTPUT, moduleName);

            return new OutputDeclaration(moduleName, parserInfo.GetUniqueAnonymousName(), id);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new OutputDeclaration(ModuleName, OutputVariable, BlockID);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            return new OutputDeclaration(ModuleName, OutputVariable, BlockID);
        }

        public override Module getAssociatedModule()
        {
            return new OutputModule();
        }

        public override List<Block> GetBlockTreeList(List<Block> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public override string ToString()
        {
            return "Output" + Environment.NewLine +
                   "Module name: " + ModuleName;
        }
    }
}
