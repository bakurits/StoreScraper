using System;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.CheckoutBots.FootSites.FootLocker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;

namespace ScraperTest.CheckoutBots.FootSites.FootLocker
{
    [TestClass()]
    public class FootLockerBotTests
    {
        private readonly FootLockerBot _bot = new FootLockerBot();

        [TestMethod]
        public void LoginTestSuc()
        {
            bool v = EOBrowserHelper.BotTester(new FootLockerBot(),
                bot => bot.Login("bakuricucxashvili@gmail.com", "P4rxKi47VHfSwDa", CancellationToken.None));
            
            Assert.IsTrue(v);
        }
        
        [TestMethod]
        public void LoginTestErr()
        {
            bool v = EOBrowserHelper.BotTester(new FootLockerBot(),
                bot => bot.Login("bakuricucxashvili@gmail.com", "tqWg3WXkg1234", CancellationToken.None));
            Assert.IsFalse(v);
        }

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
        public void FootLockerBotTest()
        {
            throw new NotImplementedException();
        }
    }
}