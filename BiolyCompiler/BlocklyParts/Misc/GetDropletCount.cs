using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Parser;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class GetDropletCount : VariableBlock
    {
        public const string VARIABLE_FIELD_NAME = "fluidName";
        public const string XML_TYPE_NAME = "getDropletCount";
        public readonly string VariableName;

        public GetDropletCount(string variableName, string output, string id, bool canBeScheduled) : base(false, null, null, output, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parser(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = ParseTools.ParseID(node);
            string variableName = ParseTools.ParseString(node, VARIABLE_FIELD_NAME);
            parserInfo.CheckVariable(id, VariableType.FLUID, variableName);

            return new GetDropletCount(variableName, parserInfo.GetUniqueAnonymousName(), id, canBeScheduled);
        }

        public override Block TrueCopy(DFG<Block> dfg)
        {
            return new GetDropletCount(VariableName, OutputVariable, BlockID, CanBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            dropPositions.TryGetValue(VariableName, out BoardFluid drops);
            return drops?.GetNumberOfDropletsAvailable() ?? 0;
        }

        public override string ToXml()
        {
            return
            $"<block type=\"{XML_TYPE_NAME}\" id=\"{BlockID}\">" +
                $"<field name=\"{VARIABLE_FIELD_NAME}\">{VariableName}</field>" +
            "</block>";
        }

        public override List<VariableBlock> GetVariableTreeList(List<VariableBlock> blocks)
        {
            blocks.Add(this);
            return blocks;
        }

        public override string ToString()
        {
            return "Droplet count of " + VariableName;
        }
    }
}