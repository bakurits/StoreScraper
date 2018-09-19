using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.Core;
using CheckoutBot.Models;
using EO.Wpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using Helper = ScraperTest.Helpers.Helper;
using WebView = EO.WebBrowser.WebView;

namespace ScraperTest.CheckoutBots.FootSites.ChampsSports
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod()]
        public void GuestCheckOutTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod()]
        public void AccountCheckoutTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod()]
        public void LoginTest()
        {
            EOBrowserHelper.BotTester(new ChampsSportsBot() { DelayInSecond = 7 }, bot =>
            {
                FootSitesBotBase.Browser.NewTab("loginTab");
                return bot.Login("gbagh16@freeuni.edu.ge", "giorgi121", CancellationToken.None);
            });
        }

        [TestMethod()]
        public void ScrapeReleasePageTest()
        {
            ChampsSportsBot bot = new ChampsSportsBot();
            List<FootsitesProduct> res = bot.ScrapeReleasePage(CancellationToken.None);
            Helper.PrintFindItemsResults(res);
        }
    }
}
