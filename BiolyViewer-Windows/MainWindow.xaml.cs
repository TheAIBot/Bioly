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

namespace BiolyViewer_Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int BOARD_WIDTH = 20;
        int BOARD_HEIGHT = 50;
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
            Browser.FrameLoadEnd += (_, ee) =>
            {
                //Wait for the webpage to finish loading
                if (ee.Frame.IsMain)
                {
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = 500;
                    timer.Elapsed += (s, __) => UpdateGraph();
                    timer.Start();
                }
            };
        }

        private void UpdateGraph()
        {
            bool didWorkspaceChange = ExecuteJs<bool>("getIfWorkspaceChanged();");
            if (didWorkspaceChange)
            {
                string xml = ExecuteJs<string>("getWorkspaceAsXml();");
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
