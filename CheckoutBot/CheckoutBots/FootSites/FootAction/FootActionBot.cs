using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    public class FootActionBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public override HttpClient Login(string username, string password, CancellationToken token)
        {
            var driver = ClientFactory.CreateProxiedChromeDriver(true);
            driver.Navigate().GoToUrl(this.WebsiteBaseUrl);
            Thread.Sleep(2000);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var loginPopupButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(text(),'Sign In')]")));
            token.ThrowIfCancellationRequested();
            loginPopupButton.Click();

            var emailTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='EMAIL']")));
            token.ThrowIfCancellationRequested();
            emailTextBox.SendKeys(username);


            var passwordTextBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='PASSWORD']")));
            token.ThrowIfCancellationRequested();
            passwordTextBox.SendKeys(password);


            var signinButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space(text())='Sign In']")));
            token.ThrowIfCancellationRequested();
            signinButton.Click();

            throw new NotImplementedException();
        }

        public FootActionBot() : base("FootAction", "https://www.footaction.com/", ApiUrl)
        {
        }
    }
}
