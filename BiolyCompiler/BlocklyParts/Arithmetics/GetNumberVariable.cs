using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class GetNumberVariable : VariableBlock
    {
        public const string VARIABLE_FIELD_NAME = "variableName";
        public const string XML_TYPE_NAME = "getNumberVariable";
        public readonly string VariableName;

        public GetNumberVariable(string variableName, string id) : base(false, null, null, id, false)
        {
            this.VariableName = variableName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string variableName = node.GetNodeWithAttributeValue(VARIABLE_FIELD_NAME).InnerText;
            parserInfo.CheckFluidVariable(id, variableName);
            return new GetNumberVariable(variableName, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            return variables[VariableName];
        }
    }
}
