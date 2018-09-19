using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.Controls;
using EO.WebBrowser;
using EO.WebEngine;
using EO.WinForm;

namespace CheckoutBot.Core
{
    public class EOBrowserDriver : IDisposable
    {
        public EOBrowserWindow MainWindow { get; } = new EOBrowserWindow();

        public WebView ActiveTab { get; private set; }



        public EOBrowserDriver()
        {
            MainWindow.Show();
        }

        /// <summary>
        /// Creates new tab and automatically sets it as active tab
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public WebView NewTab(string tabName)
        {
            MainWindow.tabControl1.Invoke((MethodInvoker)(() =>
            {
                MainWindow.tabControl1.TabPages.Add(tabName, tabName);
                var tab = MainWindow.tabControl1.TabPages[tabName]; 
                var view = new WebView()
                {
                    Engine = Engine.Default,
                    CustomUserAgent =  "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36",
                    AcceptLanguage = "en-US,en;q=0.9",
                    AllowDropLoad = true
                };
                var control = new WebControl()
                {
                    WebView = view,
                    Dock = DockStyle.Fill,
                };
                tab.Controls.Add(control);
                MainWindow.tabControl1.SelectTab(tabName);
                ActiveTab = view;
            }));
            
            return ActiveTab; 
        }


        /// <summary>
        /// Sets tab specified with name as active
        /// </summary>
        /// <param name="tabName">unique name to indentify tab</param>
        /// <returns></returns>
        public WebView SwitchToTab(string tabName)
        {
            MainWindow.tabControl1.Invoke((MethodInvoker) (() =>
            {
                ActiveTab = ((WebControl)MainWindow.tabControl1.TabPages[tabName].Controls[0]).WebView;
                MainWindow.tabControl1.SelectTab(tabName);
            }));
            
            return ActiveTab;
        }



        /// <summary>
        /// Sets tab specified with index as active
        /// </summary>
        /// <param name="tabIndex">index to indentify tab</param>
        /// <returns></returns>
        public WebView SwitchToTab(int tabIndex)
        {
            MainWindow.tabControl1.Invoke((MethodInvoker) (() =>
            {
                ActiveTab = ((WebControl)MainWindow.tabControl1.TabPages[tabIndex].Controls[0]).WebView;
                MainWindow.tabControl1.SelectTab(tabIndex);
            }));
            
            return ActiveTab;
        }

        public void Dispose()
        {

            foreach (TabPage tab in MainWindow.tabControl1.TabPages)
            {
                var webControl = (WebControl) tab.Controls[0];
                tab.Controls.Remove(webControl);
                webControl.WebView.Destroy();
            }

            MainWindow.Dispose();
        }
    }
}
