using System;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.CheckoutBots.FootSites.FootAction
{
    [TestClass()]
    public class FootActionBotTests
    {
        private FootActionBot _bot = new FootActionBot();

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
            _bot.Login("chudo", "chudisimo", CancellationToken.None);
        }

        [TestMethod()]
        public void FootActionBotTest()
        {
            throw new NotImplementedException();
        }
    }
}