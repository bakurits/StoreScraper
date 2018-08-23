using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperCore.Http;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace ScraperTest.MinorTests
{
    [TestClass]
    public class SeleniumTests
    {
        [TestMethod]
        public void FootSitesDetectionTest()
        {
            var driver = ClientFactory.CreateProxiedChromeDriver();
            driver.Navigate().GoToUrl("https://eastbay.com");
            var cookies = driver.Manage().Cookies.AllCookies;
            var weAreSorryBanner = driver.FindElementById("backendErrorHeader");
            Assert.AreNotEqual(weAreSorryBanner, null, "Selenium Detected");

            foreach (var cookie in cookies)
            {
                Console.WriteLine($"{cookie.Name} : {cookie.Value}");
            }
        }

       
    }
}
