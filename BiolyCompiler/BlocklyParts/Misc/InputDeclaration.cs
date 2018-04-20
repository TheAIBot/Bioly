using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class InputDeclaration : StaticDeclarationBlock
    {
        public const string INPUT_FLUID_FIELD_NAME = "inputName";
        public const string INPUT_AMOUNT_FIELD_NAME = "inputAmount";
        public const string FLUID_UNIT_FIELD_NAME = "inputUnit";
        public const string XML_TYPE_NAME = "inputDeclaration";
        public readonly int Amount;
        public readonly FluidUnit Unit;

        public InputDeclaration(string moduleName, string output, XmlNode node) : base(moduleName, true, output)
        {
            this.Amount = node.GetNodeWithAttributeValue(INPUT_AMOUNT_FIELD_NAME).TextToInt();
            this.Unit = StringToFluidUnit(node.GetNodeWithAttributeValue(FLUID_UNIT_FIELD_NAME).InnerText);
        }

        public InputDeclaration(string moduleName, string output, int amount) : base(moduleName, true, output)
        {
            this.Amount = amount;
            this.Unit = FluidUnit.drops;
        }

        public static Block Parse(XmlNode node)
        {
            string output = node.GetNodeWithAttributeValue(INPUT_FLUID_FIELD_NAME).InnerText;
            string moduleName = node.GetNodeWithAttributeValue(MODULE_NAME_FIELD_NAME).InnerText;
            return new InputDeclaration(moduleName, output, node);
        }

        public static FluidUnit StringToFluidUnit(string value)
        {
            switch (value)
            {
                case "0":
                    return FluidUnit.drops;
                case "1":
                    return FluidUnit.ml;
                default:
                    throw new Exception("Unknown fluid unit");
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
                boundModule = new InputModule(new BoardFluid(OutputVariable), Amount); ;
            }
            return boundModule;
        }

        public override string ToString()
        {
            return OriginalOutputVariable + Environment.NewLine +
                   "Amount: " + Amount + Unit.ToString().ToLower();
        }
    }
}
