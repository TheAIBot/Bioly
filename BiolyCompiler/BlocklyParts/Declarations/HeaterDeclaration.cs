using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Exceptions.ParserExceptions;
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
    public class HeaterDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string XML_TYPE_NAME = "heaterDeclaration";

        public HeaterDeclaration(string moduleName, string id) : base(moduleName, true, null, id)
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

            return new HeaterDeclaration(moduleName, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef)
        {
            return new HeaterDeclaration(ModuleName, BlockID);
        }

        public override string ToString()
        {
            return "Heater";
        }
    }
}
