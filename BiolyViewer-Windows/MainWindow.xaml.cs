using CefSharp;
using CefSharp.SchemeHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(rootFolder: @"../../../webpage"),
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
        }
    }
}
