using BiolyCompiler;
using BiolyCompiler.Commands;
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
            Thread.Sleep(2000);
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += UpdateGraph;
            timer.Start();

            //CommandExecutor<string> executor = new SimulatorConnector(Browser, 10, 10);
            //ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
            //programExecutor.Run(10, 10, "<xml xmlns=\"http://www.w3.org/1999/xhtml\"><variables><variable type=\"\" id=\"lAcD0b.~0C~UNK3H^T0{\">input_fluid_name</variable><variable type=\"\" id=\"2_mj/236;ixh})],K-mv\">fluid_name</variable></variables><block type=\"start\" id=\"65V|.~MwPSnQW!y.nW7(\" x=\"96\" y=\"58\"><statement name=\"program\"><block type=\"input\" id=\"A(9ZEBMC0qs|5)WIZ#Rw\"><field name=\"inputName\" id=\"lAcD0b.~0C~UNK3H^T0{\" variabletype=\"\">input_fluid_name</field><field name=\"inputAmount\">10</field><field name=\"inputUnit\">1</field><next><block type=\"output\" id=\"-ZgZ,OVUcF2ezDoRx2w6\"><value name=\"inputFluid\"><block type=\"getFluid\" id=\"|mK#s!*6%y6mpL,cwA_e\"><field name=\"fluidName\" id=\"lAcD0b.~0C~UNK3H^T0{\" variabletype=\"\">input_fluid_name</field><field name=\"fluidAmount\">5</field><field name=\"useAllFluid\">FALSE</field></block></value></block></next></block></statement></block></xml>");
        }

        private void UpdateGraph(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool didWorkspaceChange = ExecuteJs<bool>("getIfWorkspaceChanged();");
            if (didWorkspaceChange)
            {
                string xml = ExecuteJs<string>("getWorkspaceAsXml();");
                try
                {
                    CDFG cdfg = XmlParser.Parse(xml);
                    (string nodes, string edges) = SimpleGraph.CDFGToSimpleGraph(cdfg);
                    string js = "setGraph(" + nodes + ", " + edges + ");";
                    Browser.ExecuteScriptAsync(js);

                    runSimulator(xml);
                }
                catch (Exception ee)
                {
                    Debug.WriteLine(ee.Message + Environment.NewLine + ee.StackTrace);
                }
            }
        }

        Thread simulatorThread = null;
        object simulatorLocker = new object();

        private void runSimulator(string xml)
        {
            lock (simulatorLocker)
            {
                simulatorThread?.Interrupt();
                simulatorThread?.Join();
                simulatorThread = new Thread(() =>
                {
                    try
                    {
                        CommandExecutor<string> executor = new SimulatorConnector(Browser, 10, 10);
                        ProgramExecutor<string> programExecutor = new ProgramExecutor<string>(executor);
                        programExecutor.Run(10, 10, xml);
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
