using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class ImportVariable : VariableBlock, DeclarationBlock
    {
        public const string VARIABLE_FIELD_NAME = "variableName";
        public const string XML_TYPE_NAME = "importNumberVariable";
        public readonly string VariableName;

        public ImportVariable(string variableName, string id, bool canBeScheduled) : base(true, null, null, null, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string variableName = ParseTools.ParseString(node, VARIABLE_FIELD_NAME);
            parserInfo.AddVariable(id, VariableType.NUMBER, variableName);

            return new ImportVariable(variableName, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            throw new InternalRuntimeException("Not allowed to execute this block.");
        }

        public override string ToXml()
        {
            throw new InternalParseException(BlockID, "Can't create xml of this block.");
        }

        public override string ToString()
        {
            return "Import " + VariableName;
        }
    }
}
