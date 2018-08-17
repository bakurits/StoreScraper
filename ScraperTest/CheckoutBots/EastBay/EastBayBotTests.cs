using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckoutBot.CheckoutBots.EastBay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScraperTest.Helpers;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace CheckoutBotTests
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