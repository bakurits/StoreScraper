using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using EO.WebBrowser;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Core;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "http://www.eastbay.com", ApiUrl)
        {
        }

        public override HttpClient Login(string username, string password, CancellationToken token)
        {

            WebView webView = new WebView {Url = WebsiteBaseUrl};
            webView.EvalScript(@"
                                document.evaluate(""//div[@id='header_account_button']/a/span"", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click();
                                ");

            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var loginPopupButton = GetClickableElementByXPath("//div[@id='header_account_button']/a/span", wait, token);
            loginPopupButton.Click();

            var emailTextBox = GetVisibleElementByXPath("//input[@id='login_email']", wait, token);
            emailTextBox.SendKeys(username);


            var passwordTextBox = GetVisibleElementByXPath("//input[@id='login_password']", wait, token);
            passwordTextBox.SendKeys(password);

            var signinButton = GetClickableElementByXPath("//input[@id='login_submit']", wait, token);
            signinButton.Click();

            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var cartContainer = GetClickableElementByXPath("//div[@id = 'header_cart_button']", wait, token);
            cartContainer.Click();

            var select = new SelectElement(GetVisibleElementByXPath("//select[@id = 'billCountry']", wait, token));
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


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}