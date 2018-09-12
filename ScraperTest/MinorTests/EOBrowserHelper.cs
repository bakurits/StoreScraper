using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.CheckoutBots.FootSites;
using EO.WebBrowser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EO.WinForm;
using ScraperTest.Controls;

namespace ScraperTest.MinorTests
{
    public static class EOBrowserHelper
    {
     
        public static void BotTester<T>(T bot, Action<T> action) where T : FootSitesBotBase
        {
            var form = new EOTestForm();
            bot.Driver = form.Driver;
            form.Shown += (sender, args) => Task.Run(() => action(bot));
            Application.EnableVisualStyles();
            Application.Run(form);
        }

    }
}
