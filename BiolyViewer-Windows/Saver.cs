using CefSharp.Wpf;
using CefSharp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyViewer_Windows
{
    internal class Saver
    {
        private readonly ChromiumWebBrowser Browser;

        public Saver(ChromiumWebBrowser browser)
        {
            this.Browser = browser;
        }


        internal void Save()
        {

        }

        internal void SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.
            dialog.DefaultExt = "";
        }

        internal void Load()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                string fileData = File.ReadAllText(dialog.FileName);
                Browser.ExecuteScriptAsync($"loadWorkspace({fileData});");
            }
        }
    }
}
