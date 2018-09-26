using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helper = ScraperTest.Helpers.Helper;

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

            ChampsSportsBot bot = new ChampsSportsBot() { DelayInSecond = 5 };
            bot.Start(proxy: "123.176.34.159:58737");
            bot.AccountCheckout(settings, CancellationToken.None);
        }

        [TestMethod()]
        public void LoginTest()
        {
            ChampsSportsBot bot = new ChampsSportsBot() { DelayInSecond = 5 };
            bot.Start(proxy: "123.176.34.159:58737");
            bot.Browser.NewTab("loginTab");
            bool a = bot.Login("gbagh16@freeuni.edu.ge", "giorgi121", CancellationToken.None);
            Console.WriteLine(a);
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
