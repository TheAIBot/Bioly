﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public GetNumberVariable(string variableName, string output, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { variableName }, output, id, canBeScheduled)
        {
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string variableName = ParseTools.ParseString(node, VARIABLE_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.NUMBER, variableName);

            return new GetNumberVariable(variableName, parserInfo.GetUniqueAnonymousName(), id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new GetNumberVariable(InputNumbers.First(), OutputVariable, BlockID, CanBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            if (!variables.ContainsKey(InputNumbers.First()))
            {

            }
            return variables[InputNumbers.First()];
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{VARIABLE_FIELD_NAME}\">{InputNumbers.First()}</field>" +
            "</block>";
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public override string ToString()
        {
            return "Variable: " + InputNumbers.First();
        }
    }
}
