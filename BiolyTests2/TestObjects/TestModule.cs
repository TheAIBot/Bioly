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

namespace BiolyTests2.TestObjects
{
    class TestModule : BiolyCompiler.Modules.Module
    {

        public TestModule() : base(4, 4, 3000)
        {
        }


        public override OperationType getOperationType()
        {
            return OperationType.Test;
        }

        public override Module GetCopyOf()
        {
            TestModule module = new TestModule();
            module.shape = new Rectangle(shape);
            return module;
        }

    }
}
