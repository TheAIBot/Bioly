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
    public class Saver
    {
        private readonly ChromiumWebBrowser Browser;
        private const string DEFAULT_FILE_EXTENSION = ".txt";
        private const string TEMP_FILE_NAME = "temp" + DEFAULT_FILE_EXTENSION;

        public Saver(ChromiumWebBrowser browser)
        {
            this.Browser = browser;
        }


        public void QuickSave(string xml)
        {
            if (xml != "<xml xmlns=\"http://www.w3.org/1999/xhtml\"><variables></variables></xml>")
            {
                File.WriteAllText(TEMP_FILE_NAME, xml);
            }
        }

        public void SaveAs(string xml)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = DEFAULT_FILE_EXTENSION;
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, xml);
            }
        }

        public void QuickLoad()
        {
            if (File.Exists(TEMP_FILE_NAME))
            {
                LoadFileToBrowser(File.ReadAllText(TEMP_FILE_NAME));
            }
        }

        public void LoadFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = DEFAULT_FILE_EXTENSION;
            if (dialog.ShowDialog() == true)
            {
                LoadFileToBrowser(File.ReadAllText(dialog.FileName));
            }
        }

        private void LoadFileToBrowser(string xml)
        {
            string js = $"loadWorkspace('{xml}');";
            Browser.ExecuteScriptAsync(js);
        }
    }
}
