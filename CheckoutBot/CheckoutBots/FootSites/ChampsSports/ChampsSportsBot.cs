using System;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Farctory;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Http.Factory;
using static SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace CheckoutBot.CheckoutBots.FootSites.ChampsSports
{
    public class ChampsSportsBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";

        public ChampsSportsBot() : base("ChampsSports", "https://www.champssports.com/", ApiUrl)
        {
        }


        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override HttpClient Login(string username, string password, CancellationToken token)
        {

            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(this.WebsiteBaseUrl);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var loginPopupButton = GetClickableElementByXPath(wait, "//div[@id='header_login']/a", token);
            loginPopupButton.Click();

            var emailTextBox = GetVisibleElementByXPath(wait, "//input[@id='login_email']", token);
            emailTextBox.SendKeys(username);


            var passwordTextBox = GetVisibleElementByXPath(wait, "//input[@id='login_password']", token);
            passwordTextBox.SendKeys(password);

            var signinButton = GetClickableElementByXPath(wait, "//div[@class='submit']/button", token);
            signinButton.Click();
            throw new NotImplementedException();
        }

        private IWebElement GetVisibleElementByXPath(WebDriverWait wait, string xPath, CancellationToken token)
        {
            var element = wait.Until(ElementIsVisible(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }

        private IWebElement GetClickableElementByXPath(WebDriverWait wait, string xPath, CancellationToken token)
        {
            var element = wait.Until(ElementToBeClickable(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }
    }
}
