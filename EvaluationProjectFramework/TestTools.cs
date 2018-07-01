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
    public class TestTools
    {
        private readonly IWebDriver Browser;

        public TestTools()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");

            IWebDriver browser = new ChromeDriver(Directory.GetCurrentDirectory(), options);
            string path = "file:///C:/Users/Andreas/Documents/GitHub/Bioly/webpage/index.html";
            browser.Navigate().GoToUrl(path);

            Browser = browser;
        }

        public void AssemblyCleanup()
        {
            Browser?.Dispose();
        }

        public string ExecuteJS(JSProgram program)
        {
            return ExecuteJS(program.ToString());
        }

        public string ExecuteJS(string js)
        {
            IJavaScriptExecutor jsExe = (IJavaScriptExecutor)Browser;
            return (string)jsExe.ExecuteScript(js);
        }

        public void ClearWorkspace()
        {
            ExecuteJS("Blockly.mainWorkspace.clear();");
        }

        public string GetWorkspaceString()
        {
            string js = @"return getWorkspaceAsXml();";
            return ExecuteJS(js);
        }
    }
}
