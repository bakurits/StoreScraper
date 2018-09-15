using System;
using System.Threading;
using CheckoutBot.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ScraperTest.CheckoutBots.Core
{
    [TestClass]
    public class ReleasedProductsMonitorTest
    {
        [TestMethod]
        public void TestMonitor()
        {
            var releasedProductsMonitor = ReleasedProductsMonitor.Default;
            Thread.Sleep(TimeSpan.FromHours(3));
        }
        
    }
}