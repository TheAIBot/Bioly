using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using System.Management;

namespace BiolyTests
{
    [TestClass]
    public static class TestTools
    {

        internal static IWebDriver Browser { get; private set; } = null;

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Process[] processes = Process.GetProcessesByName("chromedriver"); 
            processes.ToList().ForEach(x => KillProcessTree(x.Id));

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");

            IWebDriver browser = new ChromeDriver(options);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] parts = baseDirectory.Split('\\');
            string[] requiredParts = parts.Take(parts.Length - 4).ToArray();
            string path = "file:///" + String.Join("/", requiredParts) + "/webpage/index.html";
            browser.Navigate().GoToUrl(path);

            Browser = browser;
        }

        private static void KillProcessTree(int processID)
        {
            //can't close system idle process
            if (processID == 0)
            {
                return;
            }

            var searchQuery = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={processID}");
            foreach (ManagementObject mo in searchQuery.Get())
            {
                KillProcessTree(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                Process.GetProcessById(processID).Kill();
            }
            catch (Exception) { }
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
