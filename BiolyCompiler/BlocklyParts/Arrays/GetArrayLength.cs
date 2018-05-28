﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class GetArrayLength : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getArrayLength";
        public readonly string ArrayName;

        public GetArrayLength(string arrayName, List<string> input, string id, bool canBeScheduled) : base(false, input, null, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckFluidArrayVariable(id, arrayName);

            List<string> inputs = new List<string>();
            inputs.Add(arrayName);

            return new GetArrayLength(arrayName, inputs, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return variables[FluidArray.GetArrayLengthVariable(ArrayName)];
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{IDFieldName}\">" +
                $"<field name=\"{ARRAY_NAME_FIELD_NAME}\">{ArrayName}</field>" +
            "</block>";
        }

        public override string ToString()
        {
            return "Length of " + ArrayName;
        }
    }
}