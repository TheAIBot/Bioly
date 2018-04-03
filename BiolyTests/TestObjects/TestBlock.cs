﻿using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
using BiolyCompiler.Routing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyCompiler.Modules.OperationTypes;
using System.Xml;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.VariableBlock
    {
        public readonly Module associatedModule;

        public TestBlock(List<string> input, string output, XmlNode node, Module associatedModule) : base(true, input, output)
        {
            this.associatedModule = associatedModule;
        }
        public override OperationType getOperationType()
        {
            return OperationType.Test;
        }
        public override Module getAssociatedModule()
        {
            return associatedModule;
        }
    }
}