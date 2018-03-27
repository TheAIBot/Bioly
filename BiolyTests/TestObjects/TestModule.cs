using System;
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
    class TestModule : BiolyCompiler.Modules.Module
    {

        public TestModule() : base(4, 4, 3000, 1, 1)
        {
        }

        public TestModule(int width, int height, int operationTime) : base(width, height, operationTime, 1, 1)
        {

        }

        public TestModule(int width, int height, int operationTime, int numberOfInputs, int numberOfOutputs) : base(width, height, operationTime, numberOfInputs, numberOfOutputs)
        {

        }

        public TestModule(int numberOfInputs, int numberOfOutputs) : base(4, 4, 3000, numberOfInputs, numberOfOutputs)
        {

        }

        public override OperationType getOperationType()
        {
            return OperationType.Test;
        }

        public override Module GetCopyOf()
        {
            TestModule module = new TestModule(shape.width, shape.height, operationTime, numberOfInputs, numberOfOutputs);
            module.shape = new Rectangle(shape);
            return module;
        }

    }
}