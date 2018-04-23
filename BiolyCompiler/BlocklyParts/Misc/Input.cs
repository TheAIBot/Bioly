using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Exceptions.ParserExceptions;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Input : FluidBlock
    {
        public const string InputFluidFieldName = "inputName";
        public const string InputAmountFieldName = "inputAmount";
        public const string FluidUnitFieldName = "inputUnit";
        public const string XmlTypeName = "input";
        public readonly float Amount;
        public readonly FluidUnit Unit;

        public Input(string output, XmlNode node, string id) : base(true, output, id)
        {
            this.Amount = node.GetNodeWithAttributeValue(InputAmountFieldName).TextToFloat(id);
            Validator.ValueWithinRange(id, this.Amount, 0, int.MaxValue);

            this.Unit = StringToFluidUnit(id, node.GetNodeWithAttributeValue(FluidUnitFieldName).InnerText);
        }

        public Input(string output, int amount,string id) : base(true, output, id)
        {
            this.Amount = amount;
            this.Unit = FluidUnit.drops;
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(InputFluidFieldName).InnerText;
            string id = node.GetAttributeValue(Block.IDFieldName);
            return new Input(output, node, id);
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
            if (boundModule == null)
            {
                boundModule = new InputModule(new BoardFluid(OutputVariable), (int)Amount);
            }
            return boundModule;
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount.ToString("N2") + Unit.ToString().ToLower();
        }
    }
}
