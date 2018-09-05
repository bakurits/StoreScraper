using System;
using System.Diagnostics;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                ProductToBuy = new Product(bot,
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
            throw new NotImplementedException();
        }

        [TestMethod()]
        public void LoginTest()
        {
            _bot.Login("chudo", "chudisimo", CancellationToken.None);
        }

        [TestMethod()]
        public void FootActionBotTest()
        {
            throw new NotImplementedException();
        }
    }
}