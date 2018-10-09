using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
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

        public WasteDeclaration(string moduleName, string id) : base(moduleName, false, null, id)
        {

        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string moduleName = ParseTools.ParseString(node, MODULE_NAME_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.WASTE, moduleName);

            return new WasteDeclaration(moduleName, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            return new WasteDeclaration(ModuleName, BlockID);
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
