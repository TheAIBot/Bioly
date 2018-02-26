using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.Misc;
using CefSharp;
using CefSharp.OffScreen;
using CefSharp.SchemeHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [TestMethod]
        public void ParseInputBlock()
        {
            string js = "workspace.newBlock(\"input\");" +
                        "const xml = Blockly.Xml.workspaceToDom(workspace);" +
                        "return Blockly.Xml.domToText(xml);";
            string xml = ExecuteJS(js);
            XmlNode node = StringToXmlNode(xml);
            Block input = Input.Parse(node);
        }

        private string ExecuteJS(string js)
        {
            ChromiumWebBrowser browser = TestTools.CreateBrowser();
            return (string)browser.EvaluateScriptAsync(js).Result.Result;
        }

        private XmlNode StringToXmlNode(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            //first child is the root xml node,
            //second is the first block which is what we want
            return xmlDocument.FirstChild.FirstChild;
        }
    }
}
