﻿using BiolyCompiler.Commands;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class Constant : VariableBlock
    {
        public const string NumberFieldName = "NUM";
        public const string XML_TYPE_NAME = "math_number";
        public readonly float Value;

        public Constant(string output, XmlNode node, string id) : base(false, output, id, false)
        {
            Value = node.TextToFloat(id);
        }

        public static Block Parse(XmlNode node)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            return new Constant(null, node, id);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString("N2");
        }
    }
}
