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
            new ReleasedProductsMonitor()
            {
                Token = CancellationToken.None,
                MinutesToMonitor = 10
            }.ProductsMonitoringTask();
            Thread.Sleep(TimeSpan.FromHours(3));
        }
        
    }
}