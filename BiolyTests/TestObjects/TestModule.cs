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
using BiolyCompiler.Commands;
//using MoreLinq;

namespace BiolyTests.TestObjects
{
    class TestModule : BiolyCompiler.Modules.Module
    {

        public TestModule() : base(4, 4, 3000, true)
        {
        }

        public TestModule(int width, int height, int operationTime) : base(width, height, operationTime, true)
        {

        }
        
        public TestModule(int numberOfInputs, int numberOfOutputs) : base(Math.Max(numberOfInputs, numberOfOutputs)*Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, 3000, false)
        {
            InputLayout  = getDefaultLayout(numberOfInputs, Math.Max(numberOfInputs, numberOfOutputs));
            OutputLayout = getDefaultLayout(numberOfOutputs, Math.Max(numberOfInputs, numberOfOutputs));
        }

        private ModuleLayout getDefaultLayout(int dropletCount, int dropletsContained)
        {
            //It will place the droplets horizontaly in a row.
            List<Rectangle> EmptyRectangles = new List<Rectangle>();
            List<Droplet> OutputLocations = new List<Droplet>();
            for (int i = 0; i < dropletCount; i++)
            {
                Droplet droplet = new Droplet();
                droplet.Shape = new Rectangle(Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, i * Droplet.DROPLET_WIDTH, 0);
                OutputLocations.Add(droplet);
            }
            if (dropletCount < dropletsContained)
            {
                EmptyRectangles.Add(new Rectangle((dropletsContained - dropletCount) * Droplet.DROPLET_WIDTH, Droplet.DROPLET_HEIGHT, dropletCount * Droplet.DROPLET_WIDTH, 0));
            }
            return new ModuleLayout(Shape, EmptyRectangles, OutputLocations);
        }

        public void SetLayout(ModuleLayout Layout)
        {
            this.OutputLayout = Layout;
        }

        public override OperationType getOperationType()
        {
            return OperationType.Test;
        }

        public override Module GetCopyOf()
        {
            TestModule module = new TestModule(Shape.width, Shape.height, OperationTime);
            module.Shape = new Rectangle(Shape);
            module.InputLayout = this.InputLayout.GetCopy();
            module.OutputLayout = this.OutputLayout.GetCopy();
            return module;
        }

        protected override List<Command> GetModuleCommands()
        {
            throw new NotImplementedException();
        }
    }
}