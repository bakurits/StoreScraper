﻿using System;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.FootLocker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.CheckoutBots.FootSites.FootLocker
{
    [TestClass()]
    public class FootLockerBotTests
    {
        private readonly FootLockerBot _bot = new FootLockerBot();

        [TestMethod()]
        public void LoginTest()
        {
            _bot.Login("bakuricucxashvili@gmail.com", "Yrf7B2RHW", CancellationToken.None);
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