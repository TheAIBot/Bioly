using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BiolyTests
{
    [TestClass]
    public class TestParseBlocks
    {
        [TestInitialize()]
        public void ClearWorkspace() => TestTools.ClearWorkspace();

        [TestMethod]
        public void ParseInputBlock()
        {
            string js = "workspace.newBlock(\"input\");";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = Input.Parse(node);
        }

        [TestMethod]
        public void ParseHeaterBlock()
        {
            string js = @"
                        const newFluid   = workspace.newBlock(""fluid"");
                        const heater     = workspace.newBlock(""heater"");
                        const fluidInput = workspace.newBlock(""getInput"");

                        const newFluidIn    = newFluid.getInput(""inputFluid"").connection;
                        const heaterIn      = heater.getInput(""inputFluid"").connection;
                        const heaterOut     = heater.outputConnection;
                        const fluidInputOut = fluidInput.outputConnection;

                        newFluidIn.connect(heaterOut);
                        heaterIn.connect(fluidInputOut);";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = Fluid.Parse(node);

            Assert.IsTrue(input is Heater);
        }

        [TestMethod]
        public void ParseMixerBlock()
        {
            string js = @"
                        const newFluid    = workspace.newBlock(""fluid"");
                        const mixer       = workspace.newBlock(""mixer"");
                        const fluidInputA = workspace.newBlock(""getInput"");
                        const fluidInputB = workspace.newBlock(""getInput"");

                        const newFluidIn    = newFluid.getInput(""inputFluid"").connection;
                        const mixerInA      = mixer.getInput(""inputFluidA"").connection;
                        const mixerInB      = mixer.getInput(""inputFluidB"").connection;
                        const heaterOut     = mixer.outputConnection;
                        const fluidInputAOut = fluidInputA.outputConnection;
                        const fluidInputBOut = fluidInputB.outputConnection;

                        newFluidIn.connect(heaterOut);
                        mixerInA.connect(fluidInputAOut);
                        mixerInB.connect(fluidInputBOut);";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = Fluid.Parse(node);

            Assert.IsTrue(input is Mixer);
        }

        [TestMethod]
        public void ParseSplitterBlock()
        {
            string js = @"
                        const newFluid   = workspace.newBlock(""fluid"");
                        const splitter   = workspace.newBlock(""splitter"");
                        const fluidInput = workspace.newBlock(""getInput"");

                        const newFluidIn    = newFluid.getInput(""inputFluid"").connection;
                        const heaterIn      = splitter.getInput(""inputFluid"").connection;
                        const heaterOut     = splitter.outputConnection;
                        const fluidInputOut = fluidInput.outputConnection;

                        newFluidIn.connect(heaterOut);
                        heaterIn.connect(fluidInputOut);";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = Fluid.Parse(node);

            Assert.IsTrue(input is Splitter);
        }
    }
}
