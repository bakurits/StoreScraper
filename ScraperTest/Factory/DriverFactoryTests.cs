using CheckoutBot.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.Factory
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