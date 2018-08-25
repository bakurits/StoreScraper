using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckoutBot.Farctory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutBot.Farctory
{
    [TestClass]
    public class DriverFactoryTests
    {
        [TestMethod]
        public void CreateFirefoxDriverTest()
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl("https://www.footaction.com");
        }
    }
}