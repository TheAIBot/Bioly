﻿using BiolyCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BiolyCompiler.BlocklyParts.FFUs
{
    public class Heater : Block
    {
        private const string TemperatureName = "temperature";
        private const string TimeName = "time";
        private const string InputFluidName = "inputFluid";
        public const string XmlTypeName = "heater";
        public readonly int Temperature;
        public readonly int Time;

        public Heater(List<string> input, string output, XmlNode node) : base(true, input, output)
        {
            this.Temperature = node.GetNodeWithAttributeValue(TemperatureName).TextToInt();
            this.Time = node.GetNodeWithAttributeValue(TimeName).TextToInt();
        }

        public static Block CreateHeater(string output, XmlNode node)
        {
            List<string> inputs = new List<string>();
            inputs.Add(node.GetNodeWithAttributeValue(InputFluidName).InnerText);

            return new Heater(inputs, output, node);
        }
    }
}
