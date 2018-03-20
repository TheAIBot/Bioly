using System;
using BiolyCompiler.BlocklyParts.Blocks;
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
using BiolyTests2.TestObjects;
//using MoreLinq;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.Block
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
