using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Core;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.WebBrowser;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.ChampsSports
{
    public class ChampsSportsBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/20";
        private const string CartUrl = "https://www.champssports.com/shoppingcart";
        public int DelayInSecond { private get; set; } = 5;

        public ChampsSportsBot() : base("ChampsSports", "https://www.champssports.com/", ApiUrl)
        {
        }


        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Browser.NewTab("MainTab");

            if (!Login("gbagh16@freeuni.edu.ge", "giorgi121", token))
            {
                Logger.Instance.WriteErrorLog("Wrong password");
                return;
            }

            FootsitesProduct arbitraryProduct = GetArbitraryItem(token);
            var cartTab = Browser.NewTab("Cart");
            AddArbitraryItemToCart(arbitraryProduct, token);
            Task.Delay(10 * 1000, token).Wait(token);
            AddToCart(Browser.ActiveTab, settings, token);
            Task.Delay(5 * 1000).Wait(token);

            RemoveArbitraryItem(cartTab, arbitraryProduct, token);
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Browser.SwitchToTab(0).Reload().WaitOne();
            Task.Delay(10 * 1000, token).Wait(token);
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
            Task.Delay(4000, token).Wait(token);
            Console.WriteLine(GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']/div/span/div/a[@data-btntype = 'remove']/span[2]") + ".click()");
            driver.EvalScript(GetScriptByXpath("//*[@id='page_cart']/ul/li[@data-sku='"+ product.Sku + "']/a/span") + ".click()");
            /*driver.EvalScript($@"
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
            ");*/
        }

        
        /// <summary>
        /// This method adds Arbitrary item to cart
        /// </summary>
        /// <param name="product"> Item to add </param>
        /// <param name="token"></param>
        private void AddArbitraryItemToCart(FootsitesProduct product, CancellationToken token)
        {
            AddToCart(Browser.ActiveTab, new AccountCheckoutSettings()
            {
                ProductToBuy = product,
                BuyOptions = new ProductBuyOptions()
                {
                    Quantity = 1,
                    Size = product.Sizes[0]
                }
            }, token);
            Browser.ActiveTab.LoadUrlAndWait(CartUrl);
            Browser.ActiveTab.EvalScript("document.getElementById(\"cta_button\").click();");
            Task.Delay(10000, token).Wait(token);
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
            var a = settings.ProductToBuy.Url;
            driver.LoadUrlAndWait(settings.ProductToBuy.Url);
            
            driver.EvalScript(AjaxGetRequest($@"'https://www.champssports.com/pdp/gateway?requestKey=' + requestKey + '&action=add&qty={settings.BuyOptions.Quantity}&sku={settings.ProductToBuy.Sku}&size={settings.BuyOptions.Size}&fulfillmentType=SHIP_TO_HOME&storeNumber=00000&storeCostOfGoods=0.00&_=1537363416933'"));
            //https://www.eastbay.com/pdp/gateway?requestKey=DF4D9B7FFB6EiB7D&action=add&qty=1&sku=31498357&size=M&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=1537373905543
            
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

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            webView.EvalScript(GetScriptByXpath("//div[@id='header_login']") + ".click();");
            Task.Delay(10 * 1000, token).Wait(token);
            webView.EvalScript($"{getElementById("login_email")}.value='{username}'");
            Debug.WriteLine(webView.LastJSException);
            webView.EvalScript($"{getElementById("login_password")}.value='{password}'");
            Task.Delay(10 * 1000).Wait(token);
            webView.EvalScript($"var a = document.getElementById('loginIFrame').contentDocument.getElementsByClassName('button'); a[0].click()");
            Task.Delay(10 * 1000, token).Wait(token);
            var a =  webView.EvalScript($"{getElementById("emptyFieldErrorContainer")}");
            Task.Delay(2 * 1000).Wait(token);
            return a == null;
        }

        private string getElementById(string id)
        {
            return
                $"document.getElementById('loginIFrame').contentDocument.getElementById('{id}')";
        }
    }
}
