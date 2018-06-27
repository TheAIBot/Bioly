﻿using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.Arithmetics
{
    public class Constant : VariableBlock
    {
        public const string NumberFieldName = "NUM";
        public const string XML_TYPE_NAME = "math_number";
        public readonly float Value;

        public Constant(float value, string id, bool canBeScheduled) : base(false, null, null, null, id, canBeScheduled)
        {
            this.Value = value;
        }

        public static Block Parse(XmlNode node, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            float value = node.TextToFloat(id);
            return new Constant(value, id, canBeScheduled);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef, Dictionary<string, string> renamer, string namePostfix)
        {
            return new Constant(Value, BlockID, CanBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            return Value;
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{NumberFieldName}\">{Value.ToString(CultureInfo.InvariantCulture)}</field>" +
            "</block>";
        }

        public override string ToString()
        {
            return Value.ToString("N2");
        }
    }
}
