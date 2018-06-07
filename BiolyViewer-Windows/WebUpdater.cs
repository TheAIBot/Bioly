﻿using BiolyCompiler;
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
    public class WebUpdater : IDisposable
    {
        private readonly ChromiumWebBrowser Browser;
        private readonly SettingsInfo Settings;
        private const bool SHOW_GRAPH = false;

        public WebUpdater(ChromiumWebBrowser browser, SettingsInfo settings)
        {
            this.Browser = browser;
            this.Settings = settings;
        }

        public void Update(string xml)
        {
            try
            {
                (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(xml);
                if (exceptions.Count == 0)
                {
                    string js = String.Empty;
                    if (SHOW_GRAPH)
                    {
                        (string nodes, string edges) = SimpleGraph.CDFGToSimpleGraph(cdfg);
                        js = $"setGraph({nodes}, {edges});";
                    }
                    js += $"ClearErrors();";
                    Browser.ExecuteScriptAsync(js);

                    RunSimulator(xml);
                }
                else
                {
                    var errorInfos = exceptions.GroupBy(e => e.ID)
                                               .Select(e => $"{{id: \"{e.Key}\", message: \"{String.Join(@"\n", e.Select(ee => ee.Message))}\"}}");
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
                        int boardWidth = Settings.BoardWidth;
                        int boardHeight = Settings.BoardHeight;
                        int timeBetweenCommands = (int)((1f / Settings.CommandFrequency) * 1000);
                        bool showEmptyRectangles = Settings.ShowEmptyRectangles;
                        using (SimulatorConnector executor = new SimulatorConnector(Browser, boardWidth, boardHeight))
                        {
                            ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
                            programExecutor.TimeBetweenCommands = timeBetweenCommands;
                            programExecutor.ShowEmptyRectangles = showEmptyRectangles;
                            programExecutor.Run(boardWidth, boardHeight, xml);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                });
                simulatorThread.Start();
            }
        }

        public void SettingsChanged(string settingsString)
        {
            Settings.UpdateSettingsFromString(settingsString);
            Settings.SaveSettings(settingsString, MainWindow.SETTINGS_FILE_PATH);
        }

        public void Dispose()
        {
            lock (simulatorLocker)
            {
                simulatorThread?.Interrupt();
                simulatorThread?.Join();
            }
        }
    }
}
