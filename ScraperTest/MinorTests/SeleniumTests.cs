using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using ScraperCore.Http;

using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace ScraperTest.MinorTests
{
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;

    [TestClass]
    public class SeleniumTests
    {
        private FirefoxDriver _driver;

        [TestMethod]
        public void FootSitesDetectionTest()
        {
            this._driver = DriverFactory.CreateFirefoxDriver();
            this._driver.Navigate().GoToUrl("https://www.footaction.com");
            _driver.ExecuteScript("document.cookie =\"_abck=; expires=Thu, 01 Jan 1970 00:00:01 GMT;\"");
            _driver.ExecuteScript(
                "document.cookie = \"_abck=null\"");
            this._driver.Navigate().Refresh();


            var sorryBanner = this._driver.FindElements(By.Id("backendErrorHeader"));

            Assert.IsTrue(sorryBanner.Count == 0, "Bot is probably detected");
        }


        [TestCleanup]
        public void CleanUp()
        { 
            this._driver.Close();
            this._driver.Quit();
        }
    }
}
