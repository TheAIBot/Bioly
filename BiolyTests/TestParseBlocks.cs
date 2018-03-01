using BiolyCompiler.BlocklyParts;
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
                        const heater     = workspace.newBlock(""heat"");
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
        }
    }
}
