﻿using System;
using BiolyCompiler.Parser;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.BlocklyParts.FFUs;

namespace BiolyCompiler.BlocklyParts.Misc
{
    public class Fluid : FluidBlock
    {
        public const string InputFluidFieldName = "inputFluid";
        public const string OutputFluidFieldName = "fluidName";
        public const string XmlTypeName = "fluid";

        public Fluid(List<FluidInput> input, string output, XmlNode node) : base(true, input, output)
        {
        }

        private static Block CreateFluid(string output, XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            List<FluidInput> inputs = new List<FluidInput>();
            inputs.Add(XmlParser.GetVariablesCorrectedName(node, mostRecentRef));

            return new Fluid(inputs, output, node);
        }

        public static Block Parse(XmlNode node, Dictionary<string, string> mostRecentRef)
        {
            string output = node.GetNodeWithAttributeValue(OutputFluidFieldName).InnerText;
            XmlNode innerNode = node.GetNodeWithAttributeValue(InputFluidFieldName).FirstChild;
            switch (innerNode.Attributes["type"].Value)
            {
                case Heater.XmlTypeName:
                    return Heater.CreateHeater(output, innerNode, mostRecentRef);
                case Mixer.XmlTypeName:
                    return Mixer.CreateMixer(output, innerNode, mostRecentRef);
                default:
                    return CreateFluid(output, innerNode, mostRecentRef);
            }
        }
    }
}
