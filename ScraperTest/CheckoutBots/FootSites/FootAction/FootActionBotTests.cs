using System;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Html.Sticky_bit.ChampsSports_EastBay;

namespace ScraperTest.CheckoutBots.FootSites.FootAction
{
    [TestClass()]
    public class FootActionBotTests
    {
        private FootActionBot _bot = new FootActionBot();

        [TestMethod()]
        public void GuestCheckOutTest()
        {
            FootActionBot bot = new FootActionBot();
            bot.Start();
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

        [TestMethod()]
        public void AccountCheckoutTest()
        {
            AccountCheckoutSettings settings =
                new AccountCheckoutSettings()
                {
                    UserPassword = "kohabitacia",
                    UserLogin = "datobejanishvili@gmail.com",
                    UserCcv2 = "123",
                    ProductToBuy = new FootsitesProduct(new FootSimpleBase.EastBayScraper()
                        , "yle",
                        "https://www.eastbay.com/product/model:283446/sku:A7097514",
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
            FootActionBot bot = new FootActionBot(){DelayInSecond = 5};
            bot.Start();
            bot.AccountCheckout(settings, CancellationToken.None);
        }
        [TestMethod]
        public void LoginTestSuc()
        {
            FootActionBot bot = new FootActionBot(){DelayInSecond = 5};
            bot.Start(false, "123.176.34.159:58737");
            bot.Browser.NewTab("MainTab");
            var logged = bot.Login("datobejanishvili@gmail.com", "kohabitacia", CancellationToken.None);
            bot.Stop();
            Assert.IsTrue(logged);
        }
        
        [TestMethod]
        public void LoginTestErr()
        {
            FootActionBot bot = new FootActionBot(){DelayInSecond = 5};
            bot.Start();
            bot.Browser.NewTab("MainTab");
            var logged = bot.Login("datobejanishvili@gmail.com", "kohabitaci13123a", CancellationToken.None);
            bot.Stop();
            Assert.IsFalse(logged);
        }

        [TestMethod()]
        public void FootActionBotTest()
        {
            throw new NotImplementedException();
        }
    }
}