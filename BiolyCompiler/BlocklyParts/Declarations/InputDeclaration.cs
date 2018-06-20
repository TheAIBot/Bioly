using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.TypeSystem;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class InputDeclaration : StaticDeclarationBlock, DeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputName";
        public const string INPUT_AMOUNT_FIELD_NAME = "inputAmount";
        public const string FLUID_UNIT_FIELD_NAME = "inputUnit";
        public const string XML_TYPE_NAME = "inputDeclaration";
        public readonly float Amount;

        public InputDeclaration(string output, XmlNode node, string id) : base("moduleName-" + id, true, output, id)
        {
            this.Amount = node.GetNodeWithAttributeValue(INPUT_AMOUNT_FIELD_NAME).TextToFloat(id);
            Validator.ValueWithinRange(id, this.Amount, 0, int.MaxValue);
        }

        public InputDeclaration(string moduleName, string output, int amount, string id) : base(moduleName, true, output, id)
        {
            this.Amount = amount;
        }

        public static InputDeclaration Parse(XmlNode node, ParserInfo parserInfo)
        {
            string id = node.GetAttributeValue(Block.ID_FIELD_NAME);
            string output = node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);
            parserInfo.AddVariable(id, VariableType.FLUID, output);

            return new InputDeclaration(output, node, id);
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
