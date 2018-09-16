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
          
        }

        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public override bool Login(string username, string password, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public FootActionBot() : base("FootAction", "https://www.footaction.com/", ApiUrl)
        {
        }
    }
}
