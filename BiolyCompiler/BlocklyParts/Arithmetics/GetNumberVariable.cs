using System;
using System.Collections.Generic;
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
        public readonly string VariableName;

        public GetNumberVariable(string variableName, string id, List<string> input, bool canBeScheduled) : base(false, null, input, null, id, canBeScheduled)
        {
            this.VariableName = variableName;
        }

        public static Block Parse(XmlNode node, ParserInfo parserInfo, bool canBeScheduled)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string variableName = node.GetNodeWithAttributeValue(VARIABLE_FIELD_NAME).InnerText;
            parserInfo.CheckVariable(id, VariableType.NUMBER, variableName);

            parserInfo.MostRecentVariableRef.TryGetValue(variableName, out string correctedName);
            List<string> inputs = new List<string>();
            if (correctedName != null)
            {
                inputs.Add(correctedName);
            }
            else
            {
                inputs = null;
            }

            return new GetNumberVariable(variableName, id, inputs, canBeScheduled);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef, Dictionary<string, string> renamer, string namePostfix)
        {
            renamer.TryGetValue(VariableName, out string correctedVariableName);
            mostRecentRef.TryGetValue(correctedVariableName, out string correctedName);
            List<string> inputs = new List<string>();
            inputs.Add(correctedName);

            return new GetNumberVariable(VariableName, BlockID, inputs, CanBeScheduled);
        }

        public override float Run<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions)
        {
            if (!variables.ContainsKey(VariableName))
            {
                Console.Write("");
            }
            return variables[VariableName];
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
            return "Variable: " + VariableName;
        }
    }
}
