﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class GetFluidArrayLength : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getArrayLength";
        public readonly string ArrayName;

        public GetFluidArrayLength(string arrayName, List<string> input, string id, bool canBeScheduled) : base(false, input, null, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string arrayName = node.GetNodeWithAttributeValue(ARRAY_NAME_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.FLUID_ARRAY, arrayName);

            List<string> inputs = new List<string>();
            inputs.Add(arrayName);

            return new GetFluidArrayLength(arrayName, inputs, id, canBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return variables[FluidArray.GetArrayLengthVariable(ArrayName)];
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{ARRAY_NAME_FIELD_NAME}\">{ArrayName}</field>" +
            "</block>";
        }

        public override string ToString()
        {
            return "Length of " + ArrayName;
        }
    }
}