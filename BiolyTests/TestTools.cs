using Microsoft.VisualStudio.TestTools.UnitTesting;
using BiolyCompiler.Parser;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
    public static class TestTools
    {

        internal static IWebDriver Browser { get; private set; } = null;

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            /*
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");

            IWebDriver browser = new ChromeDriver(options);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] parts = baseDirectory.Split('\\');
            string[] requiredParts = parts.Take(parts.Length - 4).ToArray();
            string path = "file:///" + String.Join("/", requiredParts) + "/webpage/index.html";
            browser.Navigate().GoToUrl(path);

            Browser = browser;
            */
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            Browser?.Dispose();
        }

        public static string ExecuteJS(JSProgram program)
        {
            return ExecuteJS(program.ToString());
        }

        public static string ExecuteJS(string js)
        {
            IJavaScriptExecutor jsExe = (IJavaScriptExecutor)TestTools.Browser;
            return (string)jsExe.ExecuteScript(js);
        }

        public static XmlNode StringToXmlBlock(string xmlText)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlText);

            //first child is the root xml node,
            //second is the first block which is what we want
            return xmlDocument.FirstChild.GetNodeWithName("block");
        }

        public static void ClearWorkspace()
        {
            ExecuteJS("Blockly.mainWorkspace.clear();");
        }

        public static XmlNode GetWorkspace()
        {
            string js = @"return getWorkspaceAsXml();";
            string xml = ExecuteJS(js);
            return StringToXmlBlock(xml);
        }

        public static Dictionary<string, string> GetDefaultRefDictionary()
        {
            Dictionary<string, string> mostRecentRef = new Dictionary<string, string>();
            mostRecentRef.Add("input_fluid_name", "input_fluid_name");
            mostRecentRef.Add("fluid_name", "fluid_name");

            return mostRecentRef;
        }
    }
}
