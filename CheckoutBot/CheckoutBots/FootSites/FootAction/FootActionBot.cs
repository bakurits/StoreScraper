using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.WebBrowser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    [JsonObject]
    public class FootActionBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";
        public int DelayInSecond { private get; set; } = 2;

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
          
        }

        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            
        }

        protected override void AddArbitraryItem(FootsitesProduct product, CancellationToken token)
        {
            AddToCart(new AccountCheckoutSettings()
            {
                ProductToBuy = product,
                BuyOptions = new ProductBuyOptions()
                {
                    Quantity = 1,
                    Size = product.Sizes[0]
                }
            }, token);
        }

        protected override void RemoveArbitraryItem(FootsitesProduct product, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void LogOut(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveAllItems(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override FootsitesProduct GetArbitraryItem(CancellationToken token)
        {
            FootsitesProduct product = ScrapeReleasePage(token)[0];
            GetProductSizes(product, token);
            return product;
        }

        protected override void FinalCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void WaitBeforeRelease(string model, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void RemoveArbitraryItem(WebView cartTab, FootsitesProduct arbitraryProduct, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void GoToCheckoutPage(CancellationToken token)
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

        public void GetProductSizes(FootsitesProduct product, CancellationToken token)
        {
            List<string> infos = new List<string>();
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);
            var document = client.GetDoc(product.Url, token).DocumentNode;
            int ind = document.InnerHtml.IndexOf("var styles = ", StringComparison.Ordinal);
            var sizeData = Utils.GetFirstJson(document.InnerHtml.Substring(ind));
            var sizesForCurProd = (JArray)sizeData[product.Sku][7];

            foreach (var item in sizesForCurProd)
            {
                var t = (JArray) item;
                var s = (string)t[0];
                infos.Add(s.Trim());
            }

            product.Sizes = infos;
        }

        public FootActionBot() : base("FootAction", "https://www.footaction.com/", ApiUrl)
        {
        }
    }
}
