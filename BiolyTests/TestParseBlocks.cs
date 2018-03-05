using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Arithmetics;
using BiolyCompiler.BlocklyParts.BoolLogic;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
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
            string js = @"workspace.newBlock(""input"");";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null);
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
            Block input = XmlParser.ParseBlock(node, null);

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
            Block input = XmlParser.ParseBlock(node, null);

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
            Block input = XmlParser.ParseBlock(node, null);

            Assert.IsTrue(input is Splitter);
        }

        [TestMethod]
        public void ParseConstantBlock()
        {
            string js = @"workspace.newBlock(""math_number"");";
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null);

            Assert.IsTrue(input is Constant);
        }

        [TestMethod]
        public void ParseArithOPBlock()
        {
            string js = @"
                        const const1 = workspace.newBlock(""math_number"");
                        const const2 = workspace.newBlock(""math_number"");
                        const arithOP = workspace.newBlock(""math_arithmetic"");

                        const arithOPAIn = arithOP.getInput(""A"").connection;
                        const arithOPBIn = arithOP.getInput(""B"").connection;
                        const const1Out = const1.outputConnection;
                        const const2Out = const2.outputConnection;

                        arithOPAIn.connect(const1Out);
                        arithOPBIn.connect(const2Out);";
                        
            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>());

            Assert.IsTrue(input is ArithOP);
        }

        [TestMethod]
        public void ParseBoolOPBlock()
        {
            string js = @"
                        const const1 = workspace.newBlock(""math_number"");
                        const const2 = workspace.newBlock(""math_number"");
                        const boolOP = workspace.newBlock(""logic_compare"");

                        const boolOPAIn = boolOP.getInput(""A"").connection;
                        const boolOPBIn = boolOP.getInput(""B"").connection;
                        const const1Out = const1.outputConnection;
                        const const2Out = const2.outputConnection;

                        boolOPAIn.connect(const1Out);
                        boolOPBIn.connect(const2Out);";

            TestTools.ExecuteJS(js);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>());

            Assert.IsTrue(input is BoolOP);
        }
    }
}
