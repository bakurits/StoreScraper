using System;
using System.CodeDom;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Shipping;
using EO.WebBrowser;
using StoreScraper.Core;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    public class FootActionBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";
        public int DelayInSecond { private get; set; } = 2;

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
          
        }

        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Logger.Instance.WriteVerboseLog($"Checkout process ({settings.ProductToBuy.Name}) started", Color.DarkOrange);
            Browser.NewTab("MainTab");

            Logger.Instance.WriteVerboseLog($"Signing in (username={settings.UserLogin}...");
            if (!Login(settings.UserLogin, settings.UserPassword, token))
            {
                Logger.Instance.WriteErrorLog("Wrong password");
            }
            Logger.Instance.WriteVerboseLog($"Login successful!", Color.DarkOliveGreen);

            Logger.Instance.WriteVerboseLog($"Clearing cart...", Color.Black);
            RemoveAllItems(Browser.ActiveTab, token);
            Logger.Instance.WriteVerboseLog($"Cart cleared!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog($"Preparing for checkout...", Color.Black);
            FootsitesProduct arbitraryProduct = GetArbitraryItem(token);
            Browser.ActiveTab.LoadUrlAndWait(arbitraryProduct.Url);
            Task.Delay(4000, token).Wait(token);
            AddArbitraryItemToCart(arbitraryProduct, token);
            Task.Delay(2000, token).Wait(token);
            GoToCheckoutPage(token);
            var cartTab = Browser.NewTab("Cart");
            RemoveArbitraryItem(cartTab, arbitraryProduct, token);
            Browser.ActiveTab.LoadUrlAndWait(settings.ProductToBuy.Url);
            Task.Delay(4000, token).Wait(token);
            Logger.Instance.WriteVerboseLog("Preparation Done!", Color.DarkOliveGreen);
            var secondsLeft = (settings.ProductToBuy.ReleaseTime - DateTime.UtcNow)?.Seconds;
            Logger.Instance.WriteVerboseLog($"Waiting product to be released (about {secondsLeft} seconds left...");

            WaitBeforeRelease(settings.ProductToBuy.Model, token);
            Logger.Instance.WriteVerboseLog($"Product release detected!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog("Adding product to cart..", Color.Black);
            AddToCart(settings, token);
            Logger.Instance.WriteVerboseLog("Product successfully added to cart!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog("Checkout product...", Color.Black);
            Browser.SwitchToTab(0).Reload().WaitOne();
            FinalCheckout(settings, token);
            Logger.Instance.WriteVerboseLog("CHECKOUT SUCCESS!!!", Color.DarkGreen);
            Logger.Instance.WriteVerboseLog("Signing out from account...");
            LogOut(token);
            Logger.Instance.WriteVerboseLog("Sign out success!", Color.Green);
        }

        private void LogOut(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void FinalCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void WaitBeforeRelease(string model, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void RemoveArbitraryItem(WebView cartTab, FootsitesProduct arbitraryProduct, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void GoToCheckoutPage(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void RemoveAllItems(WebView browserActiveTab, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            webView.EvalScript($"{GetScriptByXpath("//div[@class='c-header-ribbon__user']/button")}.click();");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            ImitateTyping(webView, "//input[@name='email.email']", username, token);
            ImitateTyping(webView, "//input[@name='password.password']", password, token);
           
            webView.EvalScript($"{GetScriptByXpath("//div[contains(@class,'c-sign-in-form__actions')]//button[contains(@class,'c-btn--primary')]")}.click()");
            Task.Delay(10 * 1000, token).Wait(token);
            return webView.EvalScript($"{GetScriptByXpath("//p[text() ='Invalid email and/or password. Please try again']")}") is JSNull;
        }

        

        public FootActionBot() : base("FootAction", "https://www.footaction.com/", ApiUrl)
        {
        }
    }
}
