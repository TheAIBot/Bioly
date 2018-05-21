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
                    string[] files = Directory.GetFiles(@"../../../../BiolyPrograms");
                    List<string> loadedPrograms = new List<string>();
                    foreach (string file in files)
                    {
                        if (System.IO.Path.GetExtension(file) == Saver.DEFAULT_FILE_EXTENSION)
                        {
                            string fileContent = File.ReadAllText(file);
                            (CDFG cdfg, List<ParseException> exceptions) = XmlParser.Parse(fileContent);
                            if (exceptions.Count == 0)
                            {
                                string programName = System.IO.Path.GetFileNameWithoutExtension(file);
                                string[] inputs = cdfg.StartDFG.Input.Where(x => x.value is InputDeclaration)
                                                                     .Select(x => "\"" + x.value.OriginalOutputVariable + "\"")
                                                                     .ToArray();
                                string[] outputs = cdfg.StartDFG.Input.Where(x => x.value is OutputDeclaration)
                                                                      .Select(x => "\"" + (x.value as OutputDeclaration).ModuleName + "\"")
                                                                      .ToArray();
                                loadedPrograms.Add($"{{name: \"{programName}\", inputs: [{String.Join(",", inputs)}], outputs: [{String.Join(",", outputs)}], programXml: \"{fileContent.Replace("\"", "'")}\"}}");
                            }
                        }
                    }

                    string allPrograms = $"[{String.Join(",", loadedPrograms)}]";
                    Browser.ExecuteScriptAsync($"startBlockly({allPrograms})");
                }
            };
        }
    }
}
