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
        public MainWindow()
        {
            var settings = new CefSettings();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "costum",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(@"../../../../webpage"),
                IsSecure = true
            });
            Cef.Initialize(settings);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            Browser.Load("costum://index.html");
            Browser.JavascriptObjectRepository.Register("saver", new Saver(Browser), true);
            Browser.JavascriptObjectRepository.Register("webUpdater", new WebUpdater(Browser), true);
            //Wait for the MainFrame to finish loading
            Browser.FrameLoadEnd += (s, args) =>
            {
                //Wait for the MainFrame to finish loading
                if (args.Frame.IsMain)
                {
                    GiveSettingsToJS();
                    GiveProgramsToJS();
                }
            };
        }

        private void GiveSettingsToJS()
        {

        }

        private void GiveProgramsToJS()
        {
            CompilerOptions.PROGRAM_FOLDER_PATH = @"../../../../BiolyPrograms";
            string[] files = Directory.GetFiles(@"../../../../BiolyPrograms");
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
                            (string[] inputStrings, string[] outputStrings, string[] variableStrings, string programXml) = InlineProgram.LoadProgram(programName);

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
            Browser.ExecuteScriptAsync($"startBlockly({allPrograms});");
        }
    }
}
