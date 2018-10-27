using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Arrays
{
    public class GetArrayLength : VariableBlock
    {
        public const string ARRAY_NAME_FIELD_NAME = "arrayName";
        public const string XML_TYPE_NAME = "getArrayLength";
        public readonly string ArrayName;

        public GetArrayLength(string arrayName, string output, string id, bool canBeScheduled) : 
            base(false, null, new List<string>() { arrayName }, output, id, canBeScheduled)
        {
            this.ArrayName = arrayName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string arrayName = ParseTools.ParseString(node, ARRAY_NAME_FIELD_NAME);
            parserInfo.CheckVariable(id, new VariableType[] { VariableType.NUMBER_ARRAY, VariableType.FLUID_ARRAY }, arrayName);

            return new GetArrayLength(arrayName, parserInfo.GetUniqueAnonymousName(), id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new GetArrayLength(ArrayName, OutputVariable, BlockID, CanBeScheduled);
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

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public override string ToString()
        {
            return "Length of " + ArrayName;
        }
    }
}
