using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class GetArrayLength : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getArrayLength";
        public readonly string ArrayName;

        public GetArrayLength(string arrayName, string id) : base(false, null, id, false)
        {
            this.ArrayName = arrayName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckFluidArrayVariable(id, arrayName);

            return new GetArrayLength(arrayName, id);
        }


        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            return variables[FluidArray.GetArrayLengthVariable(ArrayName)];
        }
    }
}
