using System;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium.Support.UI;

namespace CheckoutBot.CheckoutBots.FootSites.FootLocker
{
    public class FootLockerBot : FootSitesBotBase
    {
        public override bool Login(string username, string password, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            var loginPopupButton = GetClickableElementByXPath("//header/nav[contains(@class, 'c-header-ribbon')]/div/button", wait, token);
            loginPopupButton.Click();

            var emailTextBox = GetVisibleElementByXPath("//input[@type = 'EMAIL']", wait, token);
            emailTextBox.SendKeys(username);


            var passwordTextBox = GetVisibleElementByXPath("//input[@type = 'PASSWORD']", wait, token);
            passwordTextBox.SendKeys(password);

            var signinButton = GetClickableElementByXPath("//button[normalize-space(text())='Sign In']", wait, token);
            signinButton.Click();

            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public const string ReleasePageUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";

        public FootLockerBot() : base("Footlocker", "https://www.footlocker.com",
            ReleasePageUrl)
        {
        }
    }
}