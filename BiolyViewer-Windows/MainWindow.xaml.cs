using BiolyCompiler.Graphs;
using CefSharp;
using CefSharp.SchemeHandler;
using System;
using System.Collections.Generic;
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
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(rootFolder: @"../../../../webpage"),
                IsSecure = true
            });

            Cef.Initialize(settings);

            InitializeComponent();
            BiolyCompiler.Compiler fisk = new BiolyCompiler.Compiler();
            fisk.DoStuff();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Load("costum://index.html");
            Thread.Sleep(1000);
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += UpdateGraph;
            timer.Start();
        }

        private void UpdateGraph(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool didWorkspaceChange = ExecuteJs<bool>("getIfWorkspaceChanged();");
            if (didWorkspaceChange)
            {
                string xml = ExecuteJs<string>("getWorkspaceAsXml();");
                try
                {
                    CDFG cdfg = BiolyCompiler.Parser.XmlParser.Parse(xml);
                    (string nodes, string edges) = DFGToSimpleNE(cdfg.StartDFG);
                    string js = "setGraph(" + nodes + ", " + edges + ");";
                    Browser.ExecuteScriptAsync(js);
                }
                catch (Exception ee) { }
            }
        }

        private T ExecuteJs<T>(string js)
        {
            return (T)Browser.GetMainFrame().EvaluateScriptAsync(js, null).Result.Result;
        }

        private (string nodes, string edges) DFGToSimpleNE(DFG<BiolyCompiler.BlocklyParts.Block> dfg)
        {
            string nodes = "";
            string edges = "";

            foreach (Node<BiolyCompiler.BlocklyParts.Block> node in dfg.Nodes)
            {
                nodes += "{ data: { id: '" + node.value.OutputVariable + "', label: '" + node.value.ToString().Replace("\r\n", @"\n") + "' } },";

                foreach (Node<BiolyCompiler.BlocklyParts.Block> edgeNode in node.Edges)
                {
                    edges += "{ data: { source: '" + node.value.OutputVariable + "', target: '" + edgeNode.value.OutputVariable + "' } },";
                }
            }

            nodes = "[" + nodes + "]";
            edges = "[" + edges + "]";

            return (nodes, edges);
        }
    }
}
