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
//using MoreLinq;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.Block
    {
        public TestBlock(List<string> input, string output, XmlNode node) : base(true, input, output)
        {

        }
        public override OperationType getOperationType()
        {
            return OperationType.Test;
        }
    }
}
