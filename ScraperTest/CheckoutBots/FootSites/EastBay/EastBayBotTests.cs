using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void LoginTest()
        {
            EastBayBot bot = new EastBayBot();
            bot.Login("bakuricucxashvili@gmail.com", "Yrf7B2RHW", CancellationToken.None);
        } 

        [TestMethod]
        public void ScrapeReleasePageTest()
        {
            EastBayBot bot = new EastBayBot();
            List<Product> res = bot.ScrapeReleasePage(CancellationToken.None);
            Helper.PrintFindItemsResults(res);
        }
    }
}
