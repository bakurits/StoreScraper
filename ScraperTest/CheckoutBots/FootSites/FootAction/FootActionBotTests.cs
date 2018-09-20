using System;
using System.Diagnostics;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
using StoreScraper.Models;

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
            EOBrowserHelper.BotTester(new FootActionBot() { DelayInSecond = 7 }, bot => bot.AccountCheckout(settings, CancellationToken.None));
        }

        [TestMethod()]
        public void LoginTest()
        {
            bool v = EOBrowserHelper.BotTester(new FootActionBot() { DelayInSecond = 10 },
                bot => bot.Login("datobejanishvili@gmail.com", "kohabitacia", CancellationToken.None), "81.198.103.228:8080");
            Assert.IsTrue(v);
        }

        [TestMethod()]
        public void FootActionBotTest()
        {
            throw new NotImplementedException();
        }
    }
}