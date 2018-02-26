using CefSharp;
using CefSharp.OffScreen;
using CefSharp.SchemeHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiolyTests
{
    internal static class TestTools
    {
        private static ChromiumWebBrowser Browser = null;

        internal static ChromiumWebBrowser CreateBrowser()
        {
            if (Browser != null)
            {
                return Browser;
            }

            var settings = new CefSettings();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "costum",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(rootFolder: @"../../../../webpage"),
                IsSecure = true
            });
            Cef.Initialize(settings);

            Browser = new ChromiumWebBrowser();
            Browser.Load("costum://index.html");

            WaitForBrowserLoading();

            return Browser;
        }

        private static async void WaitForBrowserLoading()
        {
            AutoResetEvent waiter = new AutoResetEvent(false);
            Browser.LoadingStateChanged += (s, e) =>
            {
                if (!e.IsLoading)
                {
                    waiter.Set();
                }
            };
            await Task.Run(() => waiter.WaitOne());
        }
    }
}
