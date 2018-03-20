﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.BlocklyParts.Sensors
{
    public class Sensor : Block
    {
        public const string XmlTypeName = "sensor";

        public Sensor(List<string> input, string output, XmlNode node) : base(true, input, output)
        {

        }
        public Block TryParseBlock(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override OperationType getOperationType(){
            return OperationType.Sensor;
        }

        public override Module getAssociatedModule()
        {
            return new SensorModule();
        }
    }
}