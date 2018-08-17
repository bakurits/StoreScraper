using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Models;

namespace ScraperTest.CheckoutBots.EastBay
{
    [TestClass()]
    public class EastBayBotTests
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
        public void ScrapeReleasePageTest()
        {
            EastBayBot bot = new EastBayBot();
            List<Product> res = bot.ScrapeReleasePage(CancellationToken.None);
            Helper.PrintFindItemsResults(res);
        }
    }
}
