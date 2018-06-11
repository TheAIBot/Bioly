using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Commands;
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

        public GetDropletCount(string variableName, string id, bool canBeScheduled) : base(false, null, null, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parser(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string variableName = node.GetNodeWithAttributeValue(VARIABLE_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.FLUID, variableName);
            return new GetDropletCount(variableName, id, canBeScheduled);
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

        public override string ToString()
        {
            return "Droplet count of " + VariableName;
        }
    }
}