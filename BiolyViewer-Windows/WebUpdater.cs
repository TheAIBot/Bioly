using BiolyCompiler;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
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

        public WebUpdater(ChromiumWebBrowser browser, SettingsInfo settings)
        {
            this.Browser = browser;
            this.Settings = settings;
        }

        CancellationTokenSource cancelSource = null;

        public void Update(string xml)
        {
            try
            {
                (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(xml);
                if (exceptions.Count == 0)
                {
                    string js = String.Empty;
                    bool optimizedCDFG = false;
                    if (Settings.CreateGraph)
                    {
                        if (ProgramExecutor<string>.CanOptimizeCDFG(cdfg) && Settings.EnableOptimizations)
                        {
                            int boardWidth = Settings.BoardWidth;
                            int boardHeight = Settings.BoardHeight;

                            
                            cancelSource?.Cancel();

                            cancelSource = new CancellationTokenSource();
                            CDFG newCdfg = new CDFG();
                            newCdfg.StartDFG = ProgramExecutor<string>.OptimizeCDFG<string>(boardWidth, boardHeight, cdfg, cancelSource.Token, Settings.EnableGC);
                            newCdfg.AddNode(null, newCdfg.StartDFG);

                            if (cancelSource.IsCancellationRequested)
                            {
                                return;
                            }

                            cdfg = newCdfg;
                            optimizedCDFG = true;
                        }
                        (string nodes, string edges) = SimpleGraph.CDFGToSimpleGraph(cdfg);
                        js = $"setGraph({nodes}, {edges});";
                    }
                    js += $"ClearErrors();";
                    Browser.ExecuteScriptAsync(js);

                    RunSimulator(cdfg, optimizedCDFG);
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
            catch (ParseException e)
            {
                string message = $"ShowUnexpectedError(\"{e.Message.Replace('\"', ' ').Replace('\'', ' ')}\");";
                Browser.ExecuteScriptAsync(message);
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
            catch (Exception e)
            {
                string message = $"ShowUnexpectedError(\"{e.Message.Replace('\"', ' ').Replace('\'', ' ')}\");";
                Browser.ExecuteScriptAsync(message);
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        Thread simulatorThread = null;
        object simulatorLocker = new object();
        ProgramExecutor<string> CurrentlyExecutionProgram = null;

        private void RunSimulator(CDFG cdfg, bool alreadyOptimized)
        {
            lock (simulatorLocker)
            {
                if (CurrentlyExecutionProgram != null)
                {
                    CurrentlyExecutionProgram.KeepRunning.Cancel();
                }
                simulatorThread?.Join();
                simulatorThread = new Thread(() =>
                {
                    try
                    {
                        int boardWidth = Settings.BoardWidth;
                        int boardHeight = Settings.BoardHeight;
                        int timeBetweenCommands = (int)((1f / Settings.CommandFrequency) * 1000);
                        using (SimulatorConnector executor = new SimulatorConnector(Browser, boardWidth, boardHeight))
                        {
                            CurrentlyExecutionProgram = new ProgramExecutor<string>(executor);
                            CurrentlyExecutionProgram.TimeBetweenCommands = timeBetweenCommands;
                            CurrentlyExecutionProgram.ShowEmptyRectangles = Settings.ShowEmptyRectangles;
                            CurrentlyExecutionProgram.EnableOptimizations = Settings.EnableOptimizations;
                            CurrentlyExecutionProgram.EnableGarbageCollection = Settings.EnableGC;
                            CurrentlyExecutionProgram.EnableSparseElectrodes = Settings.EnableSparseBoard;

                            CurrentlyExecutionProgram.Run(boardWidth, boardHeight, cdfg, alreadyOptimized);
                        }
                    }
                    catch (InternalRuntimeException e)
                    {
                        Browser.ExecuteScriptAsync($"ShowUnexpectedError(\"{e.Message.Replace('\"', '\'')}\");");
                    }
                    catch (RuntimeException e)
                    {
                        Browser.ExecuteScriptAsync($"ShowUnexpectedError(\"{e.Message.Replace('\"', '\'')}\");");
                    }
                    catch (ThreadInterruptedException)
                    {

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
