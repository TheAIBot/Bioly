using BiolyCompiler;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Exceptions.RuntimeExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiolyOnTheWeb
{
    public class WebUpdater : IDisposable
    {
        private readonly SettingsInfo Settings = new SettingsInfo();
        private readonly IJSRuntime JSExecutor;

        public WebUpdater(IJSRuntime jsExe)
        {
            this.JSExecutor = jsExe;
        }

        CancellationTokenSource cancelSource = null;

        public async void Update(string xml)
        {
            try
            {
                //xml = xml.Replace("&lt", "<");
                //await JSExecutor.InvokeAsync<string>("alert", xml);
                //throw new Exception(xml);
                (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(xml);
                if (exceptions.Count == 0)
                {
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
                        await JSExecutor.InvokeAsync<string>("setGraph", nodes, edges);
                    }
                    await JSExecutor.InvokeAsync<string>("ClearErrors");

                    RunSimulator(cdfg, optimizedCDFG);
                }
                else
                {
                    var errorInfos = exceptions.GroupBy(e => e.ID)
                                               .Select(e => $"{{id: \"{e.Key}\", message: \"{String.Join(@"\n", e.Select(ee => ee.Message))}\"}}");
                    await JSExecutor.InvokeAsync<string>("ShowBlocklyErrors", errorInfos.ToArray());
                }
            }
            catch (ParseException e)
            {
                await JSExecutor.InvokeAsync<string>("ShowUnexpectedError", e.Message);
                Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
            catch (Exception e)
            {
                await JSExecutor.InvokeAsync<string>("ShowUnexpectedError", e.Message);
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
                simulatorThread = new Thread(async () =>
                {
                    try
                    {
                        int boardWidth = Settings.BoardWidth;
                        int boardHeight = Settings.BoardHeight;
                        int timeBetweenCommands = (int)((1f / Settings.CommandFrequency) * 1000);
                        using (SimulatorConnector executor = new SimulatorConnector(boardWidth, boardHeight))
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
                        await JSExecutor.InvokeAsync<string>("ShowUnexpectedError", e.Message.Replace('\"', ' ').Replace('\'', ' '));
                    }
                    catch (RuntimeException e)
                    {
                        await JSExecutor.InvokeAsync<string>("ShowUnexpectedError", e.Message.Replace('\"', ' ').Replace('\'', ' '));
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

        public async void SettingsChanged()
        {
            string settingsString = await JSExecutor.InvokeAsync<string>("getSettings");

            Settings.UpdateSettingsFromString(settingsString);
            Settings.SaveSettings(settingsString, "settings.stx");
        }

        public async void GiveSettingsToJS()
        {
            var settingStrings = Settings.Settings.Select(x => $"{{id: \"{x.Key}\", value: {x.Value.ToString().Replace(',', '.').ToLower()}}}").ToArray();
            string settingsString = $"[{String.Join(", ", settingStrings)}]";

            await JSExecutor.InvokeAsync<string>("setSettings", settingsString);
        }

        private async void GiveProgramsToJS()
        {
            //CompilerOptions.PROGRAM_FOLDER_PATH = PROGRAMS_FOLDER_PATH;
            //string[] files = Directory.GetFiles(PROGRAMS_FOLDER_PATH);
            //List<string> loadedPrograms = new List<string>();
            //foreach (string file in files)
            //{
            //    if (System.IO.Path.GetExtension(file) == Saver.DEFAULT_FILE_EXTENSION)
            //    {
            //        try
            //        {
            //            string fileContent = File.ReadAllText(file);
            //            (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(fileContent);
            //            if (exceptions.Count == 0)
            //            {
            //                string programName = System.IO.Path.GetFileNameWithoutExtension(file);
            //                (string[] inputStrings, string[] outputStrings, string[] variableStrings, string programXml, _) = InlineProgram.LoadProgram(programName);

            //                string inputs = String.Join(",", inputStrings.Select(x => "\"" + x + "\""));
            //                string outputs = String.Join(",", outputStrings.Select(x => "\"" + x + "\""));
            //                string variables = String.Join(", ", variableStrings.Select(x => "\"" + x + "\""));
            //                programXml = programXml.Replace("\"", "'");
            //                loadedPrograms.Add($"{{name: \"{programName}\", inputs: [{inputs}], outputs: [{outputs}], variables: [{variables}], programXml: \"{programXml}\"}}");
            //            }
            //        }
            //        catch (Exception ee)
            //        {
            //            MessageBox.Show(ee.Message + Environment.NewLine + ee.StackTrace);
            //        }
            //    }
            //}

            //string allPrograms = $"[{String.Join(",", loadedPrograms)}]";
            //string startBlockly = $"startBlockly({allPrograms});";
            //Browser.ExecuteScriptAsync(startBlockly);
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
