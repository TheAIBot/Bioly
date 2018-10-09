using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.TypeSystem;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class DropletDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "dropletName";
        public const string XML_TYPE_NAME = "dropletDeclaration";

        public DropletDeclaration(string output, string id) : base("moduleName-" + id, true, output, id)
        {
        }

        public DropletDeclaration(string moduleName, string output, string id) : base(moduleName, true, output, id)
        {
        }

        public static DropletDeclaration Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = ParseTools.ParseID(node);
            string output = ParseTools.ParseString(node, INPUT_FLUID_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.FLUID, output);

            return new DropletDeclaration(output, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> renamer, string namePostfix)
        {
            if (renamer.ContainsKey(OriginalOutputVariable))
            {
                renamer[OriginalOutputVariable] = OriginalOutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OriginalOutputVariable, OriginalOutputVariable + namePostfix);
            }
            return new DropletDeclaration(OriginalOutputVariable + namePostfix, BlockID);
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
