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
using System.Xml;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyTests.TestObjects
{

    public class TestBlock : BiolyCompiler.BlocklyParts.FluidBlock
    {
        public readonly Module associatedModule;

        public TestBlock(List<FluidInput> inputs, string output, Module associatedModule) : base(true, inputs, output, String.Empty)
        {
            this.associatedModule = associatedModule;
        }

        public TestBlock(List<string> inputs, string output, Module associatedModule) : this(inputs.Select(input => new FluidInput(input, 1, true)).ToList(), output, associatedModule)
        {
        }

        public override Module getAssociatedModule()
        {
            return associatedModule;
        }
    }
}