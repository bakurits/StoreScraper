using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using EO.WebBrowser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using ScraperTest.MinorTests;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
using StoreScraper.Models;

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
                ProductToBuy = new Product(bot,
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
                    UserPassword = "Yrf7B2RHW",
                    UserLogin = "bakuricucxashvili@gmail.com",
                    UserCcv2 = "123",
                    ProductToBuy = new FootsitesProduct(new FootSimpleBase.EastBayScraper(), "yle",
                        "https://www.eastbay.com/product/model:283446/sku:A7097514/nike-nba-swingman-jersey-mens/lebron-james/los-angeles-lakers/purple/",
                        0, "", "A7097514"),
                    BuyOptions = new ProductBuyOptions()
                    {
                        Size = "XS/S"
                    }
                };
            EOBrowserHelper.BotTester(new EastBayBot(){DelayInSecond = 7}, bot => bot.AccountCheckout(settings, CancellationToken.None));
        }

        [TestMethod]
        public void LoginTest()
        {
            EOBrowserHelper.BotTester(new EastBayBot(){DelayInSecond = 10}, bot => bot.Login("bakuricucxashvili@gmail.com", "Yrf7B2RHW", CancellationToken.None));
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
    }
}
