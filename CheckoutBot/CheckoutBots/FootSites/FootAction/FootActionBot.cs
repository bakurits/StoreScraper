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
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            GetClickableElementByXPath("//a[contains(@href,'/cart')]/span",wait, token).Click();

            ShippingAddress(settings, token, wait);
            Payment(settings, token, wait);
            throw new NotImplementedException();
        }

        private void Payment(GuestCheckoutSettings settings, CancellationToken token, WebDriverWait wait)
        {
            InputCardNumber(settings.Card.Id, wait, token);
            SelectExpirationDate(settings.Card.ValidUntil, wait, token);
            InputCsc(settings.Card.CSC, wait, token);
        }

        private void InputCardNumber(string cardId, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'cardNumber')]", wait, token).SendKeys(cardId);
        }

        private void SelectExpirationDate(DateTime cardValidUntil, WebDriverWait wait, CancellationToken token)
        {
            var month = $"{cardValidUntil:MM}";
            var year = $"{cardValidUntil:yy}";
            var selectMonth = new SelectElement( GetVisibleElementByXPath("//select[contains(@name,'expiryMonth')]", wait, token));
            var selectYear = new SelectElement( GetVisibleElementByXPath("//select[contains(@name,'expiryYear')]", wait, token));
            try
            {
                selectMonth.SelectByText(month);
                selectYear.SelectByText(year);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"couldn't select expiration dates");
                throw;
            }

        }

        private void InputCsc(string cardCsc, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'CSC')]", wait, token).SendKeys(cardCsc);
        }

        private void ShippingAddress(GuestCheckoutSettings settings, CancellationToken token, WebDriverWait wait)
        {
            SelectAddressType(settings.Shipping.AddressType, wait, token);
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
            GetVisibleElementByXPath("//input[contains(@name,'email')]", wait, token).SendKeys(email);
        }

        private void InputTelephone(string telephone, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'phone')]", wait, token).SendKeys(telephone);
        }

        private void InputZipCode(string zipCode, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'postalCode')]",wait,token).SendKeys(zipCode);
        }

        private void InputStreetAddress(string address, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'line1')]",wait,token).SendKeys(address);
        }

        private void InputLastName(string lastName, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'lastName')]", wait, token).SendKeys(lastName);
        }

        private void InputFirstName(string firstName, WebDriverWait wait, CancellationToken token)
        {
            GetVisibleElementByXPath("//input[contains(@name,'firstName')]",wait,token).SendKeys(firstName);
        }

        private void SelectCountry(Countries country, WebDriverWait wait, CancellationToken token)
        {
            var select =new SelectElement(GetVisibleElementByXPath("//select[contains(@name, 'country')]", wait, token));
            try
            {
                select.SelectByText(country.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(@"error while selecting country");
                throw;
            }
        }

        private void SelectAddressType(AddressTypes type, WebDriverWait wait, CancellationToken token)
        {
            if (type.Equals(AddressTypes.HomeBusiness))
            {
                GetClickableElementByXPath("//span[contains(text(),'Home/Business')]", wait, token).Click();
            }
            else
            {
                GetClickableElementByXPath("//span[contains(text(),'APO/FPO')]", wait, token).Click();
            }
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
