using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace ScraperTest.ScraperTests.GiorgiBaghdavadze
{
    [TestClass]
    public class SeleniumTest
    {
        [TestMethod]
        public void selenium()
        {
            FirefoxOptions options = new FirefoxOptions();
            options.SetPreference("dom.webnotifications.enabled", false);
            //FirefoxProfile prodile = new FirefoxProfile();
            //options.Profile = prodile;
            //options.Profile.SetPreference("dom.webnotifications.enabled", false);
            IWebDriver browser = new FirefoxDriver(options);
            browser.Navigate().GoToUrl("https://www.facebook.com/");
            var element = browser.FindElement(By.Id("email"));
            string email = "salome.javashvili@yahoo.com";
            element.SendKeys(email);
            string password = "rTuliparoli";
            element = browser.FindElement(By.Id("pass"));
            element.SendKeys(password);
            element = browser.FindElement(By.Id("u_0_2"));
            element.Click();
            browser.Navigate().GoToUrl("https://www.facebook.com/giorgi.bagdu.9");
            IJavaScriptExecutor jse = (IJavaScriptExecutor)browser;
            jse.ExecuteScript("window.scrollBy(0,5000);");
            ReadOnlyCollection<IWebElement> elements = browser.FindElements(By.ClassName("userContent"));
            foreach (IWebElement ele in elements)
            {
                string className = ele.FindElement(By.XPath(".//p")).Text;
                Console.WriteLine(className);
            }
            browser.Close();
            browser.Quit();
        }
    }
}
