using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Http.Factory;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public abstract partial class FootSitesBotBase
    {
        /// <summary>
        /// Generates session cookies and logs in to account. Called only in AccountCheckout mode.
        /// Will be called before several mins of product release.
        /// </summary>
        /// <param name="username">username or email of user</param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>HttpClient which contains session cookies of logged user</returns>
        protected HttpClient Login(string username, string password, CancellationToken token)
        {
            var driver = ClientFactory.CreateProxiedFirefoxDriver(true);
            driver.Navigate().GoToUrl(this.WebsiteBaseUrl);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            var loginPupupButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(text(),'Sign In')]")));
            loginPupupButton.Click();

            var emailTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='EMAIL']")));
            emailTextBox.SendKeys(username);


            var passwordTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='PASSWORD']")));
            passwordTextBox.SendKeys(password);


            var singinButon = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space(text())='Sign In']")));
            singinButon.Click();

            throw new NotImplementedException();
        }
    }
}
