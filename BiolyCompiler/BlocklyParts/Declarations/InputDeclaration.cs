using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.TypeSystem;
using BiolyCompiler.Graphs;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class InputDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputName";
        public const string INPUT_AMOUNT_FIELD_NAME = "inputAmount";
        public const string FLUID_UNIT_FIELD_NAME = "inputUnit";
        public const string XML_TYPE_NAME = "inputDeclaration";
        public readonly float Amount;

        public InputDeclaration(string output, float amount, string id) : base("moduleName-" + id, true, output, id)
        {
            this.Amount = amount;
            Validator.ValueWithinRange(id, this.Amount, 0, int.MaxValue);
        }

        public InputDeclaration(string moduleName, string output, int amount, string id) : base(moduleName, true, output, id)
        {
            this.Amount = amount;
        }

        public static InputDeclaration Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            float amount = node.GetNodeWithAttributeValue(INPUT_AMOUNT_FIELD_NAME).TextToFloat(id);
            string output = node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);
            parserInfo.AddVariable(id, VariableType.FLUID, output);

            return new InputDeclaration(output, amount, id);
        }

        public override Block CopyBlock(DFG<Block> dfg, Dictionary<string, string> mostRecentRef, Dictionary<string, string> renamer, string namePostfix)
        {
            if (renamer.ContainsKey(OriginalOutputVariable))
            {
                renamer[OriginalOutputVariable] = OriginalOutputVariable + namePostfix;
            }
            else
            {
                renamer.Add(OriginalOutputVariable, OriginalOutputVariable + namePostfix);
            }
            return new InputDeclaration(OriginalOutputVariable + namePostfix, Amount, BlockID);
        }

        public override Module getAssociatedModule()
        {
            return new InputModule(new BoardFluid(OriginalOutputVariable), (int)Amount);
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount.ToString("N2") + " drops";
        }
    }
}
