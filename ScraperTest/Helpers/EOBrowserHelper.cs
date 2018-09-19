using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Controls;
using CheckoutBot.Core;
using EO.Base;
using EO.Internal;
using EO.WebBrowser;
using EO.WebBrowser.DOM;
using EO.WebEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
namespace ScraperTest.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class EOBrowserHelper
    {   


        public static TResult BotTester<TResult, T>(T bot, Func<T, TResult> action, string proxyAddr = null) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TResult flag = default(TResult);
            Task.Delay(5000).ContinueWith(delay =>
            {
                flag = action(bot);
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            });

            FootSitesBotBase.Browser =  new EOBrowserDriver();
            WebView.ShowDebugUI();
            Application.Run();
            return flag;
        }
        
        
        public static void BotTester<T>(T bot, System.Action<T> action) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Task.Delay(5000).ContinueWith(delay =>
            {  
                action(bot);
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            });
            
            FootSitesBotBase.Browser =  new EOBrowserDriver();
            WebView.ShowDebugUI();
            Application.Run();
        }



    }
}
