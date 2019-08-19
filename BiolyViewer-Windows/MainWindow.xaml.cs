using BiolyCompiler;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using BiolyCompiler.Parser;
using CefSharp;
using CefSharp.SchemeHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MoreLinq;
using BiolyCompiler.BlocklyParts.Misc;

namespace BiolyViewer_Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string SETTINGS_FILE_PATH = "settings.stx";
        private const string PROGRAMS_FOLDER_PATH = @"../../../../BiolyPrograms";
        private const string WEBPAGE_FOLDER_PATH = @"../../../../webpage";
        private WebUpdater Updater;

        public MainWindow()
        {
            var settings = new CefSettings();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "costum",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(WEBPAGE_FOLDER_PATH),
                IsSecure = true
            });
            Cef.Initialize(settings);

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CompilerOptions.PROGRAM_FOLDER_PATH = PROGRAMS_FOLDER_PATH;
            var programData = InlineProgram.LoadProgram("Basic protocol for E. coli Quick");
            //var programData = InlineProgram.LoadProgram("showcasing using inline program block");
            //var programData = InlineProgram.LoadProgram("UsingDiluter3");

            var sassdad = XmlParser.Parse("<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='gZ,c_HQgu:,yfA:J+Lxt'>input_fluid_name</variable></variables><block type='start' id='7skRs`d%@XtBz=rV_7{/' x='528' y='219'><statement name='program'><block type='inputDeclaration' id='2lbQ[al;v|T3G{$fy9Tn'><field name='inputName' id='gZ,c_HQgu:,yfA:J+Lxt' variabletype=''>asd</field><field name='inputAmount'>1</field></block></statement></block></xml>");
            for (int i = 0; i < 1; i++)
            {
                BenchmarkExecutor executor = new BenchmarkExecutor();
                ProgramExecutor<string> CurrentlyExecutionProgram = new ProgramExecutor<string>(executor);
                CurrentlyExecutionProgram.TimeBetweenCommands = 0;
                CurrentlyExecutionProgram.ShowEmptyRectangles = false;
                CurrentlyExecutionProgram.EnableOptimizations = true;
                CurrentlyExecutionProgram.EnableGarbageCollection = true;
                CurrentlyExecutionProgram.EnableSparseElectrodes = false;

                CurrentlyExecutionProgram.Run(45, 45, programData.cdfg, false);
            }

            this.Close();

            //Run in another thread to not block the UI
            await Task.Run(() =>
            {
                SettingsInfo settings = new SettingsInfo();
                settings.LoadSettings(SETTINGS_FILE_PATH);

                this.Updater = new WebUpdater(Browser, settings);

                Browser.Load("costum://index.html");
                Browser.JavascriptObjectRepository.Register("saver", new Saver(Browser), true);
                Browser.JavascriptObjectRepository.Register("webUpdater", Updater, true);
                //Wait for the MainFrame to finish loading
                Browser.FrameLoadEnd += async (s, args) =>
                {
                    //Wait for the MainFrame to finish loading
                    if (args.Frame.IsMain)
                    {
                        //Run in another thread to not block the UI
                        await Task.Run(() =>
                        {
                            GiveSettingsToJS(settings);
                            GiveProgramsToJS();
                        });
                    }
                };
            });
        }

        private void GiveSettingsToJS(SettingsInfo settings)
        {
            var settingStrings = settings.Settings.Select(x => $"{{id: \"{x.Key}\", value: {x.Value.ToString().Replace(',','.').ToLower()}}}");
            string settingsString = $"[{String.Join(", ", settingStrings)}]";

            Browser.ExecuteScriptAsync($"setSettings({settingsString});");
        }

        private void GiveProgramsToJS()
        {
            CompilerOptions.PROGRAM_FOLDER_PATH = PROGRAMS_FOLDER_PATH;
            string[] files = Directory.GetFiles(PROGRAMS_FOLDER_PATH);
            List<string> loadedPrograms = new List<string>();
            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file) == Saver.DEFAULT_FILE_EXTENSION)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(file);
                        (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(fileContent);
                        if (exceptions.Count == 0)
                        {
                            string programName = System.IO.Path.GetFileNameWithoutExtension(file);
                            (string[] inputStrings, string[] outputStrings, string[] variableStrings, string programXml, _) = InlineProgram.LoadProgram(programName);

                            string inputs = String.Join(",", inputStrings.Select(x => "\"" + x + "\""));
                            string outputs = String.Join(",", outputStrings.Select(x => "\"" + x + "\""));
                            string variables = String.Join(", ", variableStrings.Select(x => "\"" + x + "\""));
                            programXml = programXml.Replace("\"", "'");
                            loadedPrograms.Add($"{{name: \"{programName}\", inputs: [{inputs}], outputs: [{outputs}], variables: [{variables}], programXml: \"{programXml}\"}}");
                        }
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show(ee.Message + Environment.NewLine + ee.StackTrace);
                    }
                }
            }

            string allPrograms = $"[{String.Join(",", loadedPrograms)}]";
            string startBlockly = $"startBlockly({allPrograms});";
            Browser.ExecuteScriptAsync(startBlockly);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Updater?.Dispose();
        }
    }
}
