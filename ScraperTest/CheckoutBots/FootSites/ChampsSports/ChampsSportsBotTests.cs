﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.Core;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.Wpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
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
            AccountCheckoutSettings settings =
                new AccountCheckoutSettings()
                {
                    UserPassword = "giorgi121",
                    UserLogin = "gbagh16@freeuni.edu.ge",
                    UserCcv2 = "123",
                    ProductToBuy = new FootsitesProduct(new ChampsSportsBot()
                        , "yle",
                        "https://www.champssports.com/product/model:283446/sku:A7097514",
                        0, "", "A7097514")
                    {
                        Sku = "A7097514",
                        Model = "283446",
                    },
                    BuyOptions = new ProductBuyOptions()
                    {
                        Size = "S"
                    }
                };
            EOBrowserHelper.BotTester(new ChampsSportsBot(){DelayInSecond = 7}, bot =>
            {
                bot.AccountCheckout(settings, CancellationToken.None);
            });
        }

        [TestMethod()]
        public void LoginTest()
        {
            EOBrowserHelper.BotTester(new ChampsSportsBot() { DelayInSecond = 7 }, bot =>
            {
                FootSitesBotBase.Browser.NewTab("loginTab");
                bool a = bot.Login("gbagh16@freeuni.edu.ge", "giorgi121", CancellationToken.None);
                Console.WriteLine(a);
                return a;
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
