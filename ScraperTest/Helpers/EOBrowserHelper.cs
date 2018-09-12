using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.CheckoutBots.FootSites;
using EO.WebBrowser;
using EO.WebEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EO.WinForm;
using ScraperTest.Controls;

namespace ScraperTest.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class EOBrowserHelper
    {
     
        public static void BotTester<T>(T bot, Action<T> action) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EngineOptions.Default.ExtraCommandLineArgs = "--incognito --start-maximized";
            EOTestForm form = new EOTestForm();
            bot.Driver = form.Driver;
            form.Driver.CertificateError += (sender, args) => args.Continue();
            form.Driver.CustomUserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            Task.Delay(5000).ContinueWith(delay =>
            {  
                action(bot);
            });
            
            form.Visible = true;
            form.Focus();
            Application.Run(form);
        }



    }
}
