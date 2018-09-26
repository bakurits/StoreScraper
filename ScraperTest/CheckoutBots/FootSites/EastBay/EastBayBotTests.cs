using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using CheckoutBot;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using EO.WebBrowser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
using Helper = ScraperTest.Helpers.Helper;

namespace ScraperTest.CheckoutBots.FootSites.EastBay
{
    [TestClass]
    public class EastBayBotTests
    {
        [TestMethod]
        public void GuestCheckOutTest()
        {
            EastBayBot bot = new EastBayBot();
            bot.GuestCheckOut(new GuestCheckoutSettings()
            {
                Shipping = new ShippinInfo()
                {
                    AddressLine1 = "kutaisi",
                    AddressLine2 = "tbilisi",
                    City = "kutaisi",
                    Country = Countries.Canada,
                    FirstName = "Bakuri",
                    LastName = "Tsutskhashvili",
                    State = States.Georgia,
                    ZipCode = "4600"
                },
                BuyOptions = new ProductBuyOptions(),
                Card = new Card()
                {
                    CardHolderName = "Bakuri Tsutskhashvili",
                    CSC = "111",
                    TypeOfCard = CardType.MaterCard,
                    Id = "123123",
                    TypeOfPayment = PaymentType.Card,
                    ValidUntil = DateTime.MaxValue,
                },
                ProductToBuy = new FootsitesProduct(bot,
                    "ASICS TIGER GEL-DIABLO - MEN'S",
                    "https://www.eastbay.com/product/model:293596/sku:1A129001/",
                    120,
                    "http://images.eastbay.com/pi/1A129001/zoom/",
                    "https://www.eastbay.com/product/model:293596/sku:1A129001/",
                    "USD")
            }, CancellationToken.None);
        }

        [TestMethod]
        public void AccountCheckoutTest()
        {
            AccountCheckoutSettings settings =
                new AccountCheckoutSettings()
                {
                    UserPassword = "VgnYiiY3t6",
                    UserLogin = "bakuricucxashvili@gmail.com",
                    UserCcv2 = "123",
                    ProductToBuy = new FootsitesProduct(new FootSimpleBase.EastBayScraper()
                        , "yle",
                        "https://www.eastbay.com/product/model:288213/sku:CN2980",
                        0, "", "CN2980")
                    {
                        Sku = "CN2980",
                        Model = "288213",
                    },
                    BuyOptions = new ProductBuyOptions()
                    {
                        Size = "05.0"
                    }
                };
           EastBayBot bot = new EastBayBot(){DelayInSecond = 5};
           bot.Start();
            WebView.ShowDebugUI();
           bot.AccountCheckout(settings, CancellationToken.None);
        }

        [TestMethod]
        public void LoginTestSuc()
        {
            EastBayBot bot = new EastBayBot(){DelayInSecond = 5};
            bot.Start();
            bot.Browser.NewTab("MainTab");
            var logged = bot.Login("bakuricucxashvili@gmail.com", "VgnYiiY3t6", CancellationToken.None);
            bot.Stop();
            Assert.IsTrue(logged);
        }
        
        [TestMethod]
        public void LoginTestErr()
        {
            EastBayBot bot = new EastBayBot(){DelayInSecond = 5};
            bot.Start();
            bot.Browser.NewTab("MainTab");
            var logged = bot.Login("bakuricucxashvili@gmail.com", "tqWg3WXkg1234", CancellationToken.None);
            bot.Stop();
            Assert.IsFalse(logged);
        }

        [TestMethod]
        public void ScrapeReleasePageTest()
        {
            EastBayBot bot = new EastBayBot();
            List<FootsitesProduct> res = bot.ScrapeReleasePage(CancellationToken.None);
            Helper.PrintFindItemsResults(res);
        }

        [TestMethod]
        public void TestSizes()
        {
            EastBayBot bot = new EastBayBot();
            FootsitesProduct product = new FootsitesProduct(new FootSimpleBase.EastBayScraper(), "JORDAN RETRO 13",
                "https://www.eastbay.com/product/model:150074/sku:84129035/",
                0, "", "")
            {
                Sku = "84129035",
                Model = "150074",
                Color = ""
            };
            
            bot.GetProductSizes(product, CancellationToken.None);
            Debug.WriteLine(string.Join("\n", product.Sizes));
        }

        [TestMethod]
        public void ChooseBestProxiesTest()
        {
            EastBayBot bot = new EastBayBot();
            List<WebProxy> lst = new List<WebProxy>
            {
                new WebProxy("81.198.103.228:8080"),
                new WebProxy("78.25.98.114:8080"),
                new WebProxy("178.248.71.120:33930"),
                new WebProxy("181.236.246.224:33969"),
                new WebProxy("46.228.13.42:3128"),
                new WebProxy("187.84.191.34:61900"),
                new WebProxy("123.176.34.159:58737"),
                new WebProxy("89.23.194.174:8080")
            };
            Debug.WriteLine(bot.ChooseBestProxies(lst, 4));
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
           AppData.Session = AppData.Load();
        }

    }
}
