using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.CheckoutBots.FootSites;
using EO.Internal;
using EO.WebBrowser;
using EO.WebBrowser.DOM;
using EO.WebEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EO.WinForm;
using ScraperTest.Controls;

namespace ScraperTest.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class EOBrowserHelper
    {   
        public static EOTestForm MainForm;
        public static TResult BotTester<TResult, T>(T bot, Func<T, TResult> action) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EngineOptions.Default.ExtraCommandLineArgs = "--incognito --start-maximized";
            EngineOptions.Default.SetDefaultBrowserOptions(new BrowserOptions()
            {
                LoadImages = false,
            });

            EOTestForm form = new EOTestForm();
            
            bot.Driver = form.Driver;
            form.Driver.CertificateError += (sender, args) => args.Continue();
            form.Driver.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            bot.DriverForArbitraryProduct = form.Driver2;
            form.Driver2.CertificateError += (sender, args) => args.Continue();
            form.Driver2.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

            bot.Driver3 = form.Driver3;
            form.Driver3.CertificateError += (sender, args) => args.Continue();
            form.Driver3.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

            TResult flag = default(TResult);
            Task.Delay(5000).ContinueWith(delay =>
            {
                flag = action(bot);
                Application.Exit();
            });
            
            form.Visible = true;
            form.Focus();
            MainForm = form;
            Application.Run(form);
            return flag;
        }
        
        
        public static void BotTester<T>(T bot, Action<T> action) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EngineOptions.Default.ExtraCommandLineArgs = "--incognito --start-maximized";
            EngineOptions.Default.SetDefaultBrowserOptions(new BrowserOptions()
            {
                LoadImages = false,
            });

            EOTestForm form = new EOTestForm();
            
            bot.Driver = form.Driver;
            form.Driver.CertificateError += (sender, args) => args.Continue();
            form.Driver.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            bot.DriverForArbitraryProduct = form.Driver2;
            form.Driver2.CertificateError += (sender, args) => args.Continue();
            form.Driver2.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

            bot.Driver3 = form.Driver3;
            form.Driver3.CertificateError += (sender, args) => args.Continue();
            form.Driver3.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            Task.Delay(5000).ContinueWith(delay =>
            {  
                action(bot);
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            });
            
            form.Visible = true;
            form.Focus();
            MainForm = form;
            Application.Run(form);
        }



    }
}
