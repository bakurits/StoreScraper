using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Shipping;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    public class FootActionBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
          
        }

        private void Payment(GuestCheckoutSettings settings, CancellationToken token, WebDriverWait wait)
        {
           
        }

        private void InputCardNumber(string cardId, WebDriverWait wait, CancellationToken token)
        {
            
        }

        private void SelectExpirationDate(DateTime cardValidUntil, WebDriverWait wait, CancellationToken token)
        {
            

        }

        private void InputCsc(string cardCsc, WebDriverWait wait, CancellationToken token)
        {
           
        }

        private void ShippingAddress(GuestCheckoutSettings settings, CancellationToken token, WebDriverWait wait)
        {
            SelectCountry(settings.Shipping.Country, wait, token);
            InputFirstName(settings.Shipping.FirstName, wait, token);
            InputLastName(settings.Shipping.LastName, wait, token);
            InputStreetAddress(settings.Shipping.AddressLine1, wait, token);
            InputZipCode(settings.Shipping.ZipCode, wait, token);
            InputTelephone(settings.Shipping.Telephone, wait, token);
            InputEmail(settings.Shipping.Email, wait, token);
        }

        private void InputEmail(string email, WebDriverWait wait, CancellationToken token)
        {
           
        }

        private void InputTelephone(string telephone, WebDriverWait wait, CancellationToken token)
        {
           
        }

        private void InputZipCode(string zipCode, WebDriverWait wait, CancellationToken token)
        {
          
        }

        private void InputStreetAddress(string address, WebDriverWait wait, CancellationToken token)
        {
            
        }

        private void InputLastName(string lastName, WebDriverWait wait, CancellationToken token)
        {
           
        }

        private void InputFirstName(string firstName, WebDriverWait wait, CancellationToken token)
        {
           
        }

        private void SelectCountry(Countries country, WebDriverWait wait, CancellationToken token)
        {
          
        }



        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public override bool Login(string username, string password, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
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
