using System;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
#pragma warning disable CS0618 // Type or member is obsolete
using static OpenQA.Selenium.Support.UI.ExpectedConditions;
#pragma warning restore CS0618 // Type or member is obsolete

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl  = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "http://www.eastbay.com", ApiUrl)
        {

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

        public override HttpClient Login(string username, string password, CancellationToken token)
        {
            var driver = ClientFactory.CreateProxiedFirefoxDriver(true);
            driver.Navigate().GoToUrl(this.WebsiteBaseUrl);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var loginPopupButton = GetClickableElementByXPath(wait, "//div[@id='header_account_button']/a/span", token);
            loginPopupButton.Click();

            var emailTextBox = GetVisibleElementByXPath(wait, "//input[@id='login_email']", token);
            emailTextBox.SendKeys(username);


            var passwordTextBox = GetVisibleElementByXPath(wait, "//input[@id='login_password']", token);
            passwordTextBox.SendKeys(password);

            var signinButton = GetClickableElementByXPath(wait, "//input[@id='login_submit']", token);
            signinButton.Click();

            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            string url = "https://www.eastbay.com/checkout/?uri=checkout";

            var driver = ClientFactory.CreateProxiedFirefoxDriver(true);
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            SelectElement select = new SelectElement(GetVisibleElementByXPath(wait, "//select[@id = 'billCountry']", token));
            try
            {
                select.SelectByText(settings.Shipping.Country.ToString());
            }
            catch 
            {
                Logger.Instance.WriteErrorLog("This country isn't available");
            }
            
            
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }



        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName, webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}