﻿using BiolyCompiler;
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
            program.AddInputBlock("a", 20, FluidUnit.ml);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            parserInfo.EnterDFG();
            parserInfo.AddFluidVariable("a");
            InputDeclaration input = (InputDeclaration)XmlParser.ParseBlock(node, null, parserInfo, true);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual("a", input.OriginalOutputVariable);
            Assert.AreEqual(20, input.Amount);
            Assert.AreEqual(FluidUnit.ml, input.Unit);
        }

        [TestMethod]
        public void ParseHeaterUseageAndHeaterDeclarationBlock()
        {
            JSProgram program = new JSProgram();
            program.AddHeaterSegment("a", "bestModule", 173, 39, "b", 29, false);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            parserInfo.EnterDFG();
            parserInfo.AddFluidVariable("a");
            parserInfo.AddFluidVariable("b");
            HeaterUseage heater = (HeaterUseage)XmlParser.ParseBlock(node, null, parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual("a", heater.OriginalOutputVariable);
            Assert.AreEqual(173, heater.Temperature);
            Assert.AreEqual(39, heater.Time);
        }

        [TestMethod]
        public void ParseMixerBlock()
        {
            JSProgram program = new JSProgram();
            program.AddMixerSegment("a", "b", 10, false, "c", 0, true);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            parserInfo.EnterDFG();
            parserInfo.AddFluidVariable("a");
            parserInfo.AddFluidVariable("b");
            parserInfo.AddFluidVariable("c");
            Mixer mixer = (Mixer)XmlParser.ParseBlock(node, null, parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual("a", mixer.OriginalOutputVariable);
        }

        [TestMethod]
        public void ParseConstantBlock()
        {
            JSProgram program = new JSProgram();
            program.AddConstantBlock(210);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            Constant constant = (Constant)XmlParser.ParseBlock(node, null, parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual(210, constant.Value);
        }

        [TestMethod]
        public void ParseArithOPBlock()
        {
            JSProgram program = new JSProgram();
            string a = program.AddConstantBlock(20);
            string b = program.AddConstantBlock(12);
            program.AddArithOPBlock(ArithOPTypes.ADD, a, b);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            ArithOP arithOP = (ArithOP)XmlParser.ParseBlock(node, new DFG<Block>(), parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual(ArithOPTypes.ADD, arithOP.OPType);
        }

        [TestMethod]
        public void ParseBoolOPBlock()
        {
            JSProgram program = new JSProgram();
            string a = program.AddConstantBlock(20);
            string b = program.AddConstantBlock(12);
            program.AddBoolOPBlock(BoolOPTypes.EQ, a, b);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            BoolOP boolOP = (BoolOP)XmlParser.ParseBlock(node, new DFG<Block>(), parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.AreEqual(BoolOPTypes.EQ, boolOP.OPType);
        }

        [TestMethod]
        public void ParseWasteBlock()
        {
            JSProgram program = new JSProgram();
            program.AddWasteSegment("a", 329, false);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            parserInfo.EnterDFG();
            parserInfo.AddFluidVariable("a");
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.IsTrue(input is Waste);
        }

        [TestMethod]
        public void ParseOutputBlock()
        {
            JSProgram program = new JSProgram();
            program.AddOutputSegment("a", 1249, false);
            TestTools.ExecuteJS(program);

            XmlNode node = TestTools.GetWorkspace();
            ParserInfo parserInfo = new ParserInfo();
            parserInfo.EnterDFG();
            parserInfo.AddFluidVariable("a");
            parserInfo.AddModuleName(StaticBlock.DEFAULT_MODULE_NAME);
            Block input = XmlParser.ParseBlock(node, new DFG<Block>(), parserInfo);

            Assert.AreEqual(0, parserInfo.parseExceptions.Count, parserInfo.parseExceptions.FirstOrDefault()?.Message);
            Assert.IsTrue(input is OutputUseage);
        }

        [TestMethod]
        public void ParseRandomDFG()
        {
            Random random = new Random(1280);
            for (int i = 0; i < 30; i++)
            {
                JSProgram program = new JSProgram();
                program.Render = false;
                program.CreateRandomDFG(30, random);
                TestTools.ExecuteJS(program);

                string xml = TestTools.GetWorkspaceString();
                XmlParser.Parse(xml);

                TestTools.ClearWorkspace();
            }
        }
    }
}
