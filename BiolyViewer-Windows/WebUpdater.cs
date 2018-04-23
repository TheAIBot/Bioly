using BiolyCompiler;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using CefSharp;
using CefSharp.WinForms;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiolyViewer_Windows
{
    public class WebUpdater
    {
        int BOARD_WIDTH = 20;
        int BOARD_HEIGHT = 50;
        private readonly ChromiumWebBrowser Browser;

        public WebUpdater(ChromiumWebBrowser browser)
        {
            this.Browser = browser;
        }

        public void Update(string xml)
        {
            try
            {
                (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(xml);
                if (exceptions.Count == 0)
                {
                    (string nodes, string edges) = SimpleGraph.CDFGToSimpleGraph(cdfg);
                    string js = $"setGraph({nodes}, {edges});ClearErrors();";
                    Browser.ExecuteScriptAsync(js);

                    RunSimulator(xml);
                }
                else
                {
                    string[] errorInfos = exceptions.DistinctBy(e => e.ID)
                                                    .Select(e => $"{{id: \"{e.ID}\", message: \"{String.Join(@"\n", exceptions.Where(ee => e.ID == ee.ID).Select(ee => ee.Message))}\"}}")
                                                    .ToArray();
                    string ids = string.Join(", ", errorInfos);
                    string js = $"ShowBlocklyErrors([{ids}]);";
                    Browser.ExecuteScriptAsync(js);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        Thread simulatorThread = null;
        object simulatorLocker = new object();

        private void RunSimulator(string xml)
        {
            lock (simulatorLocker)
            {
                //CommandExecutor<string> executor = new SimulatorConnector(Browser, BOARD_WIDTH, BOARD_HEIGHT);
                //ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
                //programExecutor.Run(BOARD_WIDTH, BOARD_HEIGHT, xml);
                simulatorThread?.Interrupt();
                simulatorThread?.Join();
                simulatorThread = new Thread(() =>
                {
                    try
                    {
                        CommandExecutor<string> executor = new SimulatorConnector(Browser, BOARD_WIDTH, BOARD_HEIGHT);
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
                        programExecutor.Run(BOARD_WIDTH, BOARD_HEIGHT, xml);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                });
                simulatorThread.Start();
            }
        }

        private T ExecuteJs<T>(string js)
        {
            return (T)Browser.GetMainFrame().EvaluateScriptAsync(js, null).Result.Result;
        }
    }
}
