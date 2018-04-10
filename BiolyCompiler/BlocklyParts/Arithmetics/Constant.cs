using BiolyCompiler.Commands;
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
        public const string XmlTypeName = "math_number";
        public readonly float Value;

        public Constant(string output, XmlNode node) : base(false, output)
        {
            Value = node.TextToInt();
        }

        public static Block Parse(XmlNode node)
        {
            return new Constant(null, node);
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
