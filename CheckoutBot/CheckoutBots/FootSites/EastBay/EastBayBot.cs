using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.WebBrowser;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";
        private const string CartUrl = "https://www.eastbay.com/shoppingcart";
        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }

        public int DelayInSecond { private get; set; } = 2;

        public override bool Login(string username, string password, CancellationToken token)
        {
            Driver.LoadUrlAndWait(WebsiteBaseUrl);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            

            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            string isWrong = (string) Driver.EvalScript(@"document.getElementById(""login_password_error"").innerHTML");
            //Console.WriteLine(isWrong);
            return isWrong.Length == 0;
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            while (!Login(settings.UserLogin, settings.UserPassword, token))
            {
                Logger.Instance.WriteErrorLog("Wrong password");
            }

            FootsitesProduct arbitraryProduct = GetArbitraryItem(token);
            AddArbitraryItemToCart(arbitraryProduct, token);
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            AddToCart(Driver, settings, token);
            RemoveArbitraryItem(DriverForArbitraryProduct, arbitraryProduct, token);
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
        }
        
        /// <summary>
        /// This method adds product to cart
        /// </summary>
        /// <param name="driver"> driver from which scripts are called </param>
        /// <param name="settings"> Checkout settings </param>
        /// <param name="token"></param>
        private void AddToCart(WebView driver, AccountCheckoutSettings settings, CancellationToken token)
        {
            //Console.WriteLine(settings);
            driver.LoadUrlAndWait(settings.ProductToBuy.Url);
            
            driver.EvalScript(AjaxGetRequest($@"'https://www.eastbay.com/pdp/gateway?requestKey=' +
                            requestKey +
                            '&action=add&qty={settings.BuyOptions.Quantity}&sku={settings.ProductToBuy.Sku}&size={settings.BuyOptions.Size}&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=' +
                            date"));
            Task.Delay(2000, token).Wait(token);
        }

        /// <summary>
        /// This method gets random item from releases page
        /// </summary>
        private FootsitesProduct GetArbitraryItem(CancellationToken token)
        {
            FootsitesProduct product = ScrapeReleasePage(token)[0];
            GetProductSizes(product, token);
            return product;
        }
        
        /// <summary>
        /// This method adds Arbitrary item to cart
        /// </summary>
        /// <param name="product"> Item to add </param>
        /// <param name="token"></param>
        private void AddArbitraryItemToCart(FootsitesProduct product, CancellationToken token)
        {
            AddToCart(DriverForArbitraryProduct, new AccountCheckoutSettings()
            {
                ProductToBuy = product,
                BuyOptions = new ProductBuyOptions()
                {
                    Quantity = 1,
                    Size = product.Sizes[0]
                }
            }, token);
        }

        /// <summary>
        /// This method removes item from cart
        /// </summary>
        /// <param name="driver"> driver from which scripts are called </param>
        /// <param name="product"> product to remove </param>
        /// <param name="token"></param>
        private void RemoveArbitraryItem(WebView driver, FootsitesProduct product, CancellationToken token)
        {
            driver.LoadUrlAndWait(CartUrl);   
            string lineItemIdScript = GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']") + ".getAttribute(\"data-lineitemid\")";
            driver.EvalScript($@"
                    var xhr = new XMLHttpRequest();
                    var date = Date.now();
                    var requestKey = window.frames.accountGateway._requestKey;
                    var itemId = {lineItemIdScript}
                    xhr.open('GET', 'https://www.eastbay.com/pdp/gateway?requestKey=' + requestKey + '&action=delete&lineItemId='+ itemId + '&_=' + date);
                    xhr.onload = function() {{
                        if (xhr.status === 200) {{
                            console.log(xhr.responseText);
                        }}
                        else {{
                            alert('Request failed.  Returned status of ' + xhr.status);
                        }}
                    }};
                    xhr.send();
            ");
        }


        /// <summary>
        /// Blocks current thread until product will be released
        /// </summary>
        /// <param name="model">unique model code of product to wait until release</param>
        /// <param name="token"></param>
        private void WaitBeforeRelease(string model, CancellationToken token)
        {
            bool released = false;

            do
            {
                var allReleases = ScrapeReleasePage(token);
                released = !allReleases.Find(p => p.Model == model).LaunchCountdownEnabled;
                Task.Delay(25, token).Wait(token);
            } while (!released);
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
    }
}