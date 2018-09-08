﻿using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Models;

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
            ChampsSportsBot bot = new ChampsSportsBot();
            bot.Login("bakuricucxashvili@gmail.com", "Yrf7B2RHW", CancellationToken.None);
        }

        [TestMethod()]
        public void ScrapeReleasePageTest()
        {
            ChampsSportsBot bot = new ChampsSportsBot();
            List<Product> res = bot.ScrapeReleasePage(CancellationToken.None);
            Helper.PrintFindItemsResults(res);
        }
    }
}