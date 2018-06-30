using BiolyCompiler;
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
using System.Diagnostics;

namespace BiolyTests
{
    public static class TestTools
    {

        internal static IWebDriver Browser { get; private set; } = null;

        public static void AssemblyInit()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");

            IWebDriver browser = new ChromeDriver(@"C:\Users\andre\Documents\GitHub\Bioly\EvaluationProject\bin\Debug\netcoreapp2.0", options);
            string path = "file:///C:/Users/andre/Documents/GitHub/Bioly/webpage/index.html";
            browser.Navigate().GoToUrl(path);

            Browser = browser;
        }

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
            if (Browser == null)
            {
                AssemblyInit();
            }
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

        public static string GetWorkspaceString()
        {
            string js = @"return getWorkspaceAsXml();";
            return ExecuteJS(js);
        }

        public static XmlNode GetWorkspace()
        {
            string xml = GetWorkspaceString();
            return StringToXmlBlock(xml);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (action == null) throw new ArgumentNullException("action");
            foreach (var item in enumerable) action(item);
        }
    }
}
