using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class ImportVariable : VariableBlock
    {
        public const string VARIABLE_FIELD_NAME = "variableName";
        public const string XML_TYPE_NAME = "importNumberVariable";
        public readonly string VariableName;

        public ImportVariable(string variableName, string id, bool canBeScheduled) : base(true, null, null, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string variableName = node.GetNodeWithAttributeValue(VARIABLE_FIELD_NAME).InnerText;

            return new ImportVariable(variableName, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            throw new NotImplementedException();
        }

        public override string ToXml()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Import " + ;
        }
    }
}
