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

namespace BiolyTests.ParseBlockTests
{
    [TestClass]
    public class TestParseBlocks
    {
        [TestInitialize()]
        public void ClearWorkspace() => TestTools.ClearWorkspace();

        [TestMethod]
        public void ParseInputBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "input");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null, TestTools.GetDefaultRefDictionary());
        }

        [TestMethod]
        public void ParseHeaterBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "fluid");
            program.AddBlock("b", "heater");
            program.AddBlock("c", "getInput");
            program.AddConnection("a", "inputFluid", "b");
            program.AddConnection("b", "inputFluid", "c");

            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null, TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is Heater);
        }

        [TestMethod]
        public void ParseMixerBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "fluid");
            program.AddBlock("b", "mixer");
            program.AddBlock("c", "getInput");
            program.AddBlock("d", "getInput");
            program.AddConnection("a", "inputFluid", "b");
            program.AddConnection("b", "inputFluidA", "c");
            program.AddConnection("b", "inputFluidB", "d");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null, TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is Mixer);
        }

        //[TestMethod]
        //public void ParseSplitterBlock()
        //{
        //    string js = @"
        //                const newFluid   = workspace.newBlock(""fluid"");
        //                const splitter   = workspace.newBlock(""splitter"");
        //                const fluidInput = workspace.newBlock(""getInput"");

        //                const newFluidIn    = newFluid.getInput(""inputFluid"").connection;
        //                const heaterIn      = splitter.getInput(""inputFluid"").connection;
        //                const heaterOut     = splitter.outputConnection;
        //                const fluidInputOut = fluidInput.outputConnection;

        //                newFluidIn.connect(heaterOut);
        //                heaterIn.connect(fluidInputOut);";
        //    TestTools.ExecuteJS(js);

        //    XmlNode node = TestTools.GetWorkspace();
        //    Block input = XmlParser.ParseBlock(node, null, TestTools.GetDefaultRefDictionary());

        //    Assert.IsTrue(input is Splitter);
        //}

        [TestMethod]
        public void ParseConstantBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "math_number");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, null, TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is Constant);
        }

        [TestMethod]
        public void ParseArithOPBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "math_number");
            program.AddBlock("b", "math_number");
            program.AddBlock("c", "math_arithmetic");
            program.AddConnection("c", "A", "a");
            program.AddConnection("c", "B", "b");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is ArithOP);
        }

        [TestMethod]
        public void ParseBoolOPBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "math_number");
            program.AddBlock("b", "math_number");
            program.AddBlock("c", "logic_compare");
            program.AddConnection("c", "A", "a");
            program.AddConnection("c", "B", "b");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is BoolOP);
        }

        [TestMethod]
        public void ParseWasteBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "waste");
            program.AddBlock("b", "getInput");
            program.AddConnection("a", "inputFluid", "b");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is Waste);
        }

        [TestMethod]
        public void ParseOutputBlock()
        {
            JSProgram program = new JSProgram();
            program.AddBlock("a", "output");
            program.AddBlock("b", "getInput");
            program.AddConnection("a", "inputFluid", "b");
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), TestTools.GetDefaultRefDictionary());

            Assert.IsTrue(input is Output);
        }
    }
}
