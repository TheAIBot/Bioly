using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class GetNumberVariable : VariableBlock
    {
        public const string VARIABLE_FIELD_NAME = "variableName";
        public const string XML_TYPE_NAME = "getNumberVariable";
        public readonly string VariableName;

        public GetNumberVariable(string variableName, string id, List<string> input, bool canBeScheduled) : base(false, null, input, null, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string variableName = ParseTools.ParseString(node, VARIABLE_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.NUMBER, variableName);

            List<string> inputs = new List<string>();
            inputs.Add(variableName);

            return new GetNumberVariable(variableName, id, inputs, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return variables[VariableName];
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{VARIABLE_FIELD_NAME}\">{VariableName}</field>" +
            "</block>";
        }

        public override string ToString()
        {
            return "Variable: " + VariableName;
        }
    }
}
