using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.BlocklyParts.Declarations
{
    public class InputDeclaration : StaticDeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputName";
        public const string INPUT_AMOUNT_FIELD_NAME = "inputAmount";
        public const string FLUID_UNIT_FIELD_NAME = "inputUnit";
        public const string XML_TYPE_NAME = "inputDeclaration";
        public readonly float Amount;
        public readonly FluidUnit Unit;

        public InputDeclaration(string output, XmlNode node, string id) : base("moduleName-" + id, true, output, id)
        {
            this.Amount = node.GetNodeWithAttributeValue(INPUT_AMOUNT_FIELD_NAME).TextToFloat(id);
            Validator.ValueWithinRange(id, this.Amount, 0, int.MaxValue);

            this.Unit = StringToFluidUnit(id, node.GetNodeWithAttributeValue(FLUID_UNIT_FIELD_NAME).InnerText);
        }

        public InputDeclaration(string moduleName, string output, int amount, string id) : base(moduleName, true, output, id)
        {
            this.Amount = amount;
            this.Unit = FluidUnit.drops;
        }

        public static InputDeclaration Parse(XmlNode node)
        {
            string id = node.GetAttributeValue(Block.IDFieldName);
            string output = node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).InnerText;
            Validator.CheckVariableName(id, output);

            return new InputDeclaration(output, node, id);
        }

        public static FluidUnit StringToFluidUnit(string id, string value)
        {
            switch (value)
            {
                case "0":
                    return FluidUnit.drops;
                case "1":
                    return FluidUnit.ml;
                default:
                    throw new InternalParseException(id, $"Unknown fluid unit.{Environment.NewLine}Expected  either 0 or 1 but value was {value}.");
            }
        }

        public static string FluidUnitToString(FluidUnit value)
        {
            switch (value)
            {
                case FluidUnit.drops:
                    return "0";
                case FluidUnit.ml:
                    return "1";
                default:
                    throw new Exception("Unknown fluid unit");
            }
        }

        public override Module getAssociatedModule()
        {
            return new InputModule(new BoardFluid(OriginalOutputVariable), (int)Amount);
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount.ToString("N2") + Unit.ToString().ToLower();
        }
    }
}
