using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
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
        private ChromeDriver driver;

        [TestMethod]
        public void FootSitesDetectionTest()
        {
            this.driver = ClientFactory.CreateProxiedChromeDriver();
            this.driver.Navigate().GoToUrl("https://www.footaction.com");
            driver.ExecuteScript("document.cookie =\"_abck=; expires=Thu, 01 Jan 1970 00:00:01 GMT;\"");
            driver.ExecuteScript(
                "document.cookie = \"_abck=null\"");
            this.driver.Navigate().Refresh();


            var sorryBanner = this.driver.FindElements(By.Id("backendErrorHeader"));

            Assert.IsTrue(sorryBanner.Count == 0, "Bot is probably detected");
        }


        [TestCleanup]
        public void CleanUp()
        { 
            this.driver.Close();
            this.driver.Quit();
        }
    }
}
